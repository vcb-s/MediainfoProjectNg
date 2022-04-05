using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MediainfoProjectNg
{
    static class Utils
    {
        // TODO: Determine what should be excluded
        private static readonly List<string> ExcludeDirs = new List<string>
        {
            "CDs",
            "Scans"
        };

        private static readonly List<string> ExcludeExts = new List<string>
        {
            ".txt",
            ".log",
            ".torrent"
        };

        private static readonly string[] Matroska = {".mkv", ".mka", ".mks"};
        private static readonly string[] MPEG_4 = {".mp4", ".m4a", ".m4v"};

        public static IEnumerable<string> EnumerateFolder(string folderPath)
        {
            foreach (var file in Directory.GetFiles(folderPath))
            {
                yield return file;
            }

            var folderQueue = new Queue<string>();
            folderQueue.EnqueueRange(Directory.GetDirectories(folderPath));
            while (folderQueue.Count > 0)
            {
                var currentFolder = folderQueue.Dequeue();
                if (ExcludeDirs.Contains(Path.GetFileName(currentFolder))) continue;
                foreach (var file in Directory.GetFiles(currentFolder))
                {
                    yield return file;
                }

                folderQueue.EnqueueRange(Directory.GetDirectories(currentFolder));
            }
        }

        public static async Task<(IEnumerable<FileInfo> info, long duration)> Load(string[] urls,
            Func<string, bool> filter = null, Action<string> progressCallback = null)
        {
            var fileInfos = new List<FileInfo>();
            var sw = new Stopwatch();
            sw.Start();
            foreach (var file in urls.Where(File.Exists))
            {
                var info = await LoadFile(file, filter, progressCallback);
                fileInfos.Add(info);
            }

            foreach (var dir in urls.Where(Directory.Exists))
            {
                if (ExcludeDirs.Contains(Path.GetFileName(dir))) continue;
                foreach (var file in EnumerateFolder(dir))
                {
                    var info = await LoadFile(file, filter, progressCallback);
                    fileInfos.Add(info);
                }
            }

            sw.Stop();
            return (fileInfos.Where(item => item != null), sw.ElapsedMilliseconds);
        }

        public static async Task<FileInfo> LoadFile(string path, Func<string, bool> filter = null,
            Action<string> progressCallback = null)
        {
            if (!File.Exists(path)) return null;
            if (ExcludeExts.Contains(Path.GetExtension(path))) return null;
            if (filter?.Invoke(path) ?? false) return null;
            progressCallback?.Invoke(path);
            return await Task.Run(() => new FileInfo(path));
        }

        public static long TryParseAsLong(this string s)
        {
            return decimal.TryParse(s, out var i) ? (long) i : 0;
        }

        public static bool FileNameContentMatched(FileInfo info)
        {
            var filenameReg =
                new Regex(
                    @"^\[[^\[\]]*VCB\-S(?:tudio)?[^\[\]]*\] [^\[\]]+ (?:\[[^\[\]]*\d*\])?\[(?<profile>.*?)_(?<resolution>.*?)\]\[(?<vencoder>.*?)(?<aencoders>(?:_\d*.*?)*)\]\.mkv$");
            var match = filenameReg.Match(Path.GetFileName(info.GeneralInfo.FullPath));
            if (!match.Success) return true;
            var profile = GenerateProfileString(info.VideoInfos[0].Profile);
            if (profile == "") return true;
            var vencoder = GenerateVencoderString(info.VideoInfos[0]);
            if (vencoder == "") return true;
            return match.Groups["profile"].Value == profile && match.Groups["vencoder"].Value == vencoder
                                                            && match.Groups["aencoders"].Value ==
                                                            GenerateAencodersString(info.AudioInfos);
        }

        public static int TryParseAsMillisecond(this string s)
        {
            return TimeSpan.TryParse(s, out var ts) ? (int) ts.TotalMilliseconds : 0;
        }

        private static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                queue.Enqueue(item);
            }
        }

        private static string GenerateProfileString(ProfileInfo info)
        {
            switch (info.Profile)
            {
                case "Main 10":
                    return "Ma10p";
                case "High 10":
                    return "Hi10p";
                case "High 4:4:4 Predictive":
                    return "Hi444pp";
                default:
                    return "";
            }
        }

        // TODO: Proper resolution calculation
        private static string GenerateResolutionString(int width, int height)
        {
            if (width == 1920 || height == 1080)
                return "1080p";
            if (height == 480)
                return "480p";
            return "";
        }

        private static string GenerateVencoderString(VideoInfo info)
        {
            switch (info.Format)
            {
                case "HEVC":
                    return "x265";
                case "AVC":
                    return "x264";
                default:
                    return "";
            }
        }

        private static string GenerateAencodersString(List<AudioInfo> infos)
        {
            var audios = new Dictionary<string, int>();
            var ret = "";
            foreach (var info in infos)
            {
                if (!audios.ContainsKey(info.Format))
                {
                    audios.Add(info.Format, 1);
                }
                else
                {
                    audios[info.Format]++;
                }
            }

            foreach (var key in audios.Keys)
            {
                ret +=
                    $"_{(audios[key] > 1 ? audios[key].ToString() : string.Empty)}{Regex.Replace(key, "[^a-zA-Z0-9]+", "", RegexOptions.Compiled).ToLower()}";
            }

            return ret;
        }

        public static List<ErrorInfo> CheckFile(FileInfo info)
        {
            var ret = new List<ErrorInfo>();
            var extension = Path.GetExtension(info.GeneralInfo.FullPath);

            if (info.GeneralInfo.Format == "Matroska" && !Matroska.Contains(extension)
                || info.GeneralInfo.Format == "MPEG-4" && !MPEG_4.Contains(extension))
            {
                ret.Add(new ErrorInfo
                {
                    Level = ErrorLevel.Error,
                    Description = $"文件后缀和与容器不符。后缀：{extension}，容器{info.GeneralInfo.Format}",
                    Brush = Brushes.Red
                });
            }

            if (info.VideoInfos.Any(o => o.Delay != 0) || info.AudioInfos.Any(o => o.Delay != 0))
            {
                ret.Add(new ErrorInfo
                {
                    Level = ErrorLevel.Warning,
                    Description = "容器中含有延时非 0 的轨道。",
                    Brush = new SolidColorBrush(Color.FromRgb(0, 164, 172))
                });
            }

            var duration = new List<long>();
            duration.AddRange(info.VideoInfos.Select(videoInfo => videoInfo.Duration));
            duration.AddRange(info.AudioInfos.Select(audioInfo => audioInfo.Duration));
            if (duration.Count > 0)
            {
                if (duration.Max() - duration.Min() > 600)
                {
                    ret.Add(new ErrorInfo
                    {
                        Level = ErrorLevel.Warning,
                        Description = "轨道间长度相差过大。",
                        Brush = Brushes.PaleVioletRed
                    });
                }

                if (info.GeneralInfo.ChapterCount != 0)
                {
                    if (info.GeneralInfo.ChapterCount == 1)
                    {
                        ret.Add(new ErrorInfo
                        {
                            Level = ErrorLevel.Warning,
                            Description = "文件只有一个章节。",
                            Brush = Brushes.Yellow
                        });
                    }
                    else if (info.GeneralInfo.ChapterCount == -1)
                    {
                        ret.Add(new ErrorInfo
                        {
                            Level = ErrorLevel.Warning,
                            Description = "文件存在多组章节。",
                            Brush = Brushes.Yellow
                        });
                    }
                    else if (info.ChapterInfos.Last().Timespan > duration.Max() - 1100)
                    {
                        ret.Add(new ErrorInfo
                        {
                            Level = ErrorLevel.Warning,
                            Description = "文件末尾有无用章节。",
                            Brush = Brushes.Yellow
                        });
                    }
                    else if (info.ChapterInfos.First().Timespan != 0)
                    {
                        ret.Add(new ErrorInfo
                        {
                            Level = ErrorLevel.Warning,
                            Description = "首个章节时间戳非零。",
                            Brush = Brushes.Yellow
                        });
                    }
                }

                if (!FileNameContentMatched(info))
                {
                    ret.Add(new ErrorInfo
                    {
                        Level = ErrorLevel.Error,
                        Description = "内容物和文件名描述不符。",
                        Brush = Brushes.Violet
                    });
                }

                if (info.AudioInfos.Count > 2)
                {
                    ret.Add(new ErrorInfo
                    {
                        Level = ErrorLevel.Info,
                        Description = "文件含有多条音轨。",
                        Brush = Brushes.GreenYellow
                    });
                }
            }

            return ret;
        }
    }
}
