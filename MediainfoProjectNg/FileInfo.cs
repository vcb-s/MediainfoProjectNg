using MediaInfoLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace MediainfoProjectNg
{
    public enum ErrorLevel
    {
        Info,
        Warning,
        Error
    }

    public class ProfileInfo
    {
        public string? Profile { get; }
        public string? Level { get; }
        public ProfileInfo(string profileString)
        {
            var strs = profileString.Split('@');
            if (strs.Length > 0)
                Profile = strs[0];
            if (strs.Length > 1)
                Level = strs[1];
        }
    }

    public class GeneralInfo(string filename, string fullPath, string format, long bitrate, long videoCount, long audioCount, long textCount, long chapterCount)
    {
        public string Filename { get; set; } = filename;
        public string FullPath { get; set; } = fullPath;
        public string Format { get; set; } = format;
        public long Bitrate { get; set; } = bitrate;
        public long VideoCount { get; set; } = videoCount;
        public long AudioCount { get; set; } = audioCount;
        public long TextCount { get; set; } = textCount;
        public long ChapterCount { get; set; } = chapterCount;
    }

    // TODO: Using actual type instead of string
    public class VideoInfo(string format, string formatProfile, string fpsMode, string fps, long bitrate, long bitDepth, long duration, long height, long width, string language, long delay, ProfileInfo profile, string colorSpace, string isDefault)
    {
        public string Format { get; set; } = format;
        public string FormatProfile { get; set; } = formatProfile;
        public string FpsMode { get; set; } = fpsMode;
        public string Fps { get; set; } = fps;
        public long Bitrate { get; set; } = bitrate;
        public long BitDepth { get; set; } = bitDepth;
        public long Duration { get; set; } = duration;
        public long Height { get; set; } = height;
        public long Width { get; set; } = width;
        public string Language { get; set; } = language;
        public long Delay { get; set; } = delay;
        public ProfileInfo Profile { get; set; } = profile;
        public string ColorSpace { get; set; } = colorSpace;
        public string Default { get; set; } = isDefault;
        public string Resolution => $"{Width}x{Height}";
    }

    public class AudioInfo(string format, long bitDepth, long bitrate, long duration, string language, long delay, string isDefault)
    {
        public string Format { get; set; } = format;
        public long BitDepth { get; set; } = bitDepth;
        public long Bitrate { get; set; } = bitrate;
        public long Duration { get; set; } = duration;
        public string Language { get; set; } = language;
        public long Delay { get; set; } = delay;
        public string Default { get; set; } = isDefault;
    }

    public class ChapterInfo(int timespan, string name, string language)
    {
        public int Timespan { get; set; } = timespan;
        public string Name { get; set; } = name;
        public string Language { get; set; } = language;
    }

    public class SubInfo(string format, string isDefault, string language)
    {
        public string Format { get; set; } = format;
        public string Default { get; set; } = isDefault;
        public string Language { get; set; } = language;
    }

    public class ErrorInfo(ErrorLevel level, string description, Brush brush)
    {
        public ErrorLevel Level { get; set; } = level;
        public string Description { get; set; } = description;
        public Brush Brush { get; set; } = brush;
    }

    public class FileInfo
    {
        public GeneralInfo GeneralInfo { get; }
        public List<VideoInfo> VideoInfos { get; } = [];
        public List<AudioInfo> AudioInfos { get; } = [];
        public List<ChapterInfo> ChapterInfos { get; } = [];
        public List<SubInfo> SubInfos { get; } = [];
        //        public List<ErrorInfo> ErrorInfos { get; set; } = null;
        public string Summary { get; }
        [Browsable(false)]
        public VideoInfo? Video0 => GetAtOrDefault(VideoInfos, 0);
        [Browsable(false)]
        public AudioInfo? Audio0 => GetAtOrDefault(AudioInfos, 0);
        [Browsable(false)]
        public AudioInfo? Audio1 => GetAtOrDefault(AudioInfos, 1);
        [Browsable(false)]
        public SubInfo? Sub0 => GetAtOrDefault(SubInfos, 0);

        private static T? GetAtOrDefault<T>(IReadOnlyList<T> source, int index)
            where T : class
        {
            return index < source.Count ? source[index] : null;
        }

        public FileInfo(string url)
        {
#if DEBUG
            var length = new System.IO.FileInfo(url).Length;
            var sw = new Stopwatch();
            sw.Start();
#endif
            MediaInfo? MI = null;
            try
            {
                MI = new MediaInfo();
                MI.Open(url);
                MI.Option("Complete");
                Summary = MI.Inform();

                GeneralInfo = new GeneralInfo(
                    filename: Path.GetFileNameWithoutExtension(url),
                    fullPath: url,
                    format: MI.Get(StreamKind.General, 0, "Format"),
                    bitrate: MI.Get(StreamKind.General, 0, "OverallBitRate").TryParseAsLong() / 1000,
                    videoCount: MI.Get(StreamKind.General, 0, "VideoCount").TryParseAsLong(),
                    audioCount: MI.Get(StreamKind.General, 0, "AudioCount").TryParseAsLong(),
                    textCount: MI.Get(StreamKind.General, 0, "TextCount").TryParseAsLong(),
                    chapterCount: MI.Get(StreamKind.General, 0, "MenuCount").TryParseAsLong() switch
                    {
                        0 => 0,
                        1 => MI.Get(StreamKind.Menu, 0, "Chapters_Pos_End").TryParseAsLong() - MI.Get(StreamKind.Menu, 0, "Chapters_Pos_Begin").TryParseAsLong(),
                        _ => -1
                    }
                );

                for (var i = 0; i < GeneralInfo.VideoCount; i++)
                {
                    var colorSpaceRaw = MI.Get(StreamKind.Video, i, "ColorSpace");
                    var chromaSubsampling = MI.Get(StreamKind.Video, i, "ChromaSubsampling");
                    string colorSpace = string.Empty;
                    colorSpace = colorSpaceRaw.ToUpper() + chromaSubsampling.Replace(":", "");

                    var defaultRaw = MI.Get(StreamKind.Video, i, "Default").ToLower();
                    string isDefault = (defaultRaw == "yes" || defaultRaw == "1") ? "Yes" : "No";
                    VideoInfos.Add(new VideoInfo(
                        format: MI.Get(StreamKind.Video, i, "Format"),
                        formatProfile: MI.Get(StreamKind.Video, i, "Format_Profile"),
                        fpsMode: MI.Get(StreamKind.Video, i, "FrameRate_Mode"),
                        fps: MI.Get(StreamKind.Video, i, "FrameRate/String").Replace(" FPS", ""),
                        bitrate: MI.Get(StreamKind.Video, i, "BitRate").TryParseAsLong() / 1000,
                        bitDepth: MI.Get(StreamKind.Video, i, "BitDepth").TryParseAsLong(),
                        duration: MI.Get(StreamKind.Video, i, "Duration").TryParseAsLong(),
                        height: MI.Get(StreamKind.Video, i, "Height").TryParseAsLong(),
                        width: MI.Get(StreamKind.Video, i, "Width").TryParseAsLong(),
                        language: string.IsNullOrWhiteSpace(MI.Get(StreamKind.Video, i, "Language/String3"))
                                        ? "UND"
                                        : MI.Get(StreamKind.Video, i, "Language/String3").ToUpper(),
                        delay: MI.Get(StreamKind.Video, i, "Delay").TryParseAsLong(),
                        profile: new ProfileInfo(MI.Get(StreamKind.Video, i, "Format_Profile")),
                        colorSpace: colorSpace,
                        isDefault: isDefault
                    ));
#if DEBUG
                    Debug.WriteLine(MI.Get(StreamKind.Video, i, "Stored_Width"));
                    Debug.WriteLine(MI.Get(StreamKind.Video, i, "Stored_Height"));
                    Debug.WriteLine(MI.Get(StreamKind.Video, i, "Sampled_Width"));
                    Debug.WriteLine(MI.Get(StreamKind.Video, i, "Sampled_Height"));
                    Debug.WriteLine(MI.Get(StreamKind.Video, i, "PixelAspectRatio"));
                    Debug.WriteLine(MI.Get(StreamKind.Video, i, "PixelAspectRatio/String"));
                    Debug.WriteLine(MI.Get(StreamKind.Video, i, "PixelAspectRatio_Original"));
                    Debug.WriteLine("ScanType:" + MI.Get(StreamKind.Video, i, "ScanType"));
                    Debug.WriteLine("ScanType/String:" + MI.Get(StreamKind.Video, i, "ScanType/String"));
                    Debug.WriteLine("FormatProfile:" + MI.Get(StreamKind.Video, i, "Format_Profile"));
                    Debug.WriteLine("FormatLevel:" + MI.Get(StreamKind.Video, i, "Format_Level"));
                    Debug.WriteLine("FormatTier:" + MI.Get(StreamKind.Video, i, "Format_Tier"));
#endif
                }

                for (var i = 0; i < GeneralInfo.AudioCount; i++)
                {
                    var defaultRaw = MI.Get(StreamKind.Audio, i, "Default").ToLower();
                    string isDefault = (defaultRaw == "yes" || defaultRaw == "1") ? "Yes" : "No";
                    AudioInfos.Add(new AudioInfo(
                        format: MI.Get(StreamKind.Audio, i, "Format"),
                        bitDepth: MI.Get(StreamKind.Audio, i, "BitDepth").TryParseAsLong(),
                        bitrate: MI.Get(StreamKind.Audio, i, "BitRate").TryParseAsLong() / 1000,
                        duration: MI.Get(StreamKind.Audio, i, "Duration").TryParseAsLong(),
                        language: MI.Get(StreamKind.Audio, i, "Language/String3").ToUpper(),
                        delay: MI.Get(StreamKind.Audio, i, "Delay").TryParseAsLong(),
                        isDefault: isDefault
                    ));
                }

                for (var i = 0; i < GeneralInfo.TextCount; i++)
                {
                    var defaultRaw = MI.Get(StreamKind.Text, i, "Default").ToLower();
                    string isDefault = (defaultRaw == "yes" || defaultRaw == "1") ? "Yes" : "No";
                    SubInfos.Add(new SubInfo(
                        format: MI.Get(StreamKind.Text, i, "Format"),
                        isDefault: isDefault,
                        language: MI.Get(StreamKind.Text, i, "Language/String3").ToUpper()
                    ));
                }

                if (GeneralInfo.ChapterCount > 0)
                {
                    var chapPosBegin = (int)MI.Get(StreamKind.Menu, 0, "Chapters_Pos_Begin").TryParseAsLong();
                    var chapPosEnd = (int)MI.Get(StreamKind.Menu, 0, "Chapters_Pos_End").TryParseAsLong();
                    for (var i = chapPosBegin; i < chapPosEnd; i++)
                    {
                        var name = MI.Get(StreamKind.Menu, 0, i, InfoKind.Text);
                        string language = "";

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            var idx = name.IndexOf(':');
                            if (idx > 0)
                            {
                                language = name[..idx].Trim();
                                language = language.ToLower() switch
                                {
                                    "en" => "ENG",
                                    "ja" => "JPN",
                                    "zh" => "CHI",
                                    _ => language.ToUpper(),
                                };
                            }
                        }
                        ChapterInfos.Add(new ChapterInfo(
                            timespan: MI.Get(StreamKind.Menu, 0, i, InfoKind.Name).TryParseAsMillisecond(),
                            language: language,
                            name: name
                        ));
                    }
                }

            }
            finally
            {
                MI?.Close();
            }
#if DEBUG
            sw.Stop();
            Debug.WriteLine($"Loading: {url}\r\nCost {sw.ElapsedMilliseconds}ms! Length: {length}bytes");
#endif
        }
    }
}
