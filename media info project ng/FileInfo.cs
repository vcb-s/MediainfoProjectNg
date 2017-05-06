using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using media_info_project_ng.Annotations;
using MediaInfoLib;

namespace media_info_project_ng
{
    public class GeneralInfo
    {
        public string Filename { get; set; }
        public string FullPath { get; set; }
        public string Format { get; set; }
        public string Bitrate { get; set; }
        public int VideoCount { get; set; }
        public int AudioCount { get; set; }
        public int TextCount { get; set; }
        public int MenuCount { get; set; }
    }

    // TODO: Using actual type instead of string
    public class VideoInfo
    {
        public string Format { get; set; }
        public string FormatProfile { get; set; }
        public string Fps { get; set; }
        public string Bitrate { get; set; }
        public string BitDepth { get; set; }
        public string Duration { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public string Language { get; set; }
    }

    public class AudioInfo
    {
        public string Format { get; set; }
        public string BitDepth { get; set; }
        public string Bitrate { get; set; }
        public string Language { get; set; }
    }

    public class FileInfoDetail
    {
        public string Filename { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string VideoFormat { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public string VideoDepth { get; set; } = string.Empty;
        public string Fps { get; set; } = string.Empty;
        public string Audio1Format { get; set; } = string.Empty;
        public string Audio1Depth { get; set; } = string.Empty;
        public string Audio1Language { get; set; } = string.Empty;
        public string Audio2Format { get; set; } = string.Empty;
        public string Audio2Depth { get; set; } = string.Empty;
        public string Audio2Language { get; set; } = string.Empty;
        public string HasChapter { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        // TODO: Find and apply the default color.
        public SolidColorBrush ForegroundColorBrush { get; set; } = Brushes.Black;
        public SolidColorBrush BackgroundColorBrush { get; set; } = Brushes.White;
    }

    public class FileInfo
    {
        private GeneralInfo GeneralInfo { get; } = new GeneralInfo();
        private List<VideoInfo> VideoInfos { get; } = new List<VideoInfo>();
        private List<AudioInfo> AudioInfos { get; } = new List<AudioInfo>();
        public string Summary { get; }

        public FileInfoDetail FileInfoDetail
        {
            get
            {
                var detail = new FileInfoDetail();
                detail.Filename = GeneralInfo.Filename;
                detail.Format = GeneralInfo.Format;
                if (GeneralInfo.VideoCount > 0)
                {
                    detail.VideoFormat = VideoInfos[0].Format;
                    detail.Resolution = $"{VideoInfos[0].Width}x{VideoInfos[0].Height}";
                    detail.VideoDepth = VideoInfos[0].BitDepth;
                    detail.Fps = VideoInfos[0].Fps;
                }
                if (GeneralInfo.AudioCount > 0)
                {
                    detail.Audio1Format = AudioInfos[0].Format;
                    detail.Audio1Depth = AudioInfos[0].BitDepth;
                    detail.Audio1Language = AudioInfos[0].Language;
                }
                if (GeneralInfo.AudioCount > 1)
                {
                    detail.Audio2Format = AudioInfos[1].Format;
                    detail.Audio2Depth = AudioInfos[1].BitDepth;
                    detail.Audio2Language = AudioInfos[1].Language;
                }
                detail.HasChapter = GeneralInfo.MenuCount > 0 ? "有" : "";
                if (detail.Format == "Matroska" &&
                    !new List<String> {".mkv", ".mka", ".mks"}.Contains(Path.GetExtension(GeneralInfo.FullPath))
                    || detail.Format == "MPEG-4" &&
                    !new List<String> {".mp4", ".m4a", ".m4v"}.Contains(Path.GetExtension(GeneralInfo.FullPath)))
                {
                    detail.ForegroundColorBrush = Brushes.White;
                    detail.BackgroundColorBrush = Brushes.DarkRed;
                }
                else if (GeneralInfo.AudioCount > 2)
                {
                    detail.ForegroundColorBrush = Brushes.White;
                    detail.BackgroundColorBrush = Brushes.DarkGreen;
                }
                else if (GeneralInfo.TextCount > 0)
                {
                    detail.ForegroundColorBrush = Brushes.Blue;
                }
                return detail;
            }
        }

        public FileInfo(string url)
        {
            var MI = new MediaInfo();
            MI.Open(url);
            MI.Option("Complete");
            Summary = MI.Inform();
            {
                GeneralInfo.Filename = MI.Get(StreamKind.General, 0, "FileName");
                GeneralInfo.FullPath = url;
                GeneralInfo.Format = MI.Get(StreamKind.General, 0, "Format");
                GeneralInfo.Bitrate = MI.Get(StreamKind.General, 0, "OverallBitRate/String");
                {
                    GeneralInfo.VideoCount = int.TryParse(MI.Get(StreamKind.General, 0, "VideoCount"), out var i)
                        ? i
                        : 0;
                }
                {
                    GeneralInfo.AudioCount = int.TryParse(MI.Get(StreamKind.General, 0, "AudioCount"), out var i)
                        ? i
                        : 0;
                }
                {
                    GeneralInfo.TextCount = int.TryParse(MI.Get(StreamKind.General, 0, "TextCount"), out var i)
                        ? i
                        : 0;
                }
                {
                    GeneralInfo.MenuCount = int.TryParse(MI.Get(StreamKind.General, 0, "MenuCount"), out var i)
                        ? i
                        : 0;
                }
            }

            for (var i = 0; i < GeneralInfo.VideoCount; i++)
            {
                VideoInfos.Add(new VideoInfo
                {
                    Format = MI.Get(StreamKind.Video, i, "Format"),
                    FormatProfile = MI.Get(StreamKind.Video, i, "Format_Profile"),
                    Fps = MI.Get(StreamKind.Video, i, "FrameRate/String").Replace(" FPS", ""),
                    Bitrate = MI.Get(StreamKind.Video, i, "BitRate"),
                    BitDepth = MI.Get(StreamKind.Video, i, "BitDepth"),
                    Duration = MI.Get(StreamKind.Video, i, "Duration"),
                    Height = MI.Get(StreamKind.Video, i, "Height"),
                    Width = MI.Get(StreamKind.Video, i, "Width"),
                    Language = MI.Get(StreamKind.Video, i, "Language/String3").ToUpper()
                });
            }

            for (var i = 0; i < GeneralInfo.AudioCount; i++)
            {
                AudioInfos.Add(new AudioInfo
                {
                    Format = MI.Get(StreamKind.Audio, i, "Format"),
                    BitDepth = MI.Get(StreamKind.Audio, i, "BitDepth"),
                    Bitrate = MI.Get(StreamKind.Audio, i, "BitRate") == ""
                        ? ""
                        : (int.Parse(MI.Get(StreamKind.Audio, i, "BitRate")) / 1000).ToString(),
                    Language = MI.Get(StreamKind.Audio, i, "Language/String3").ToUpper()
                });
            }

            var a = new DataGridRow();
        }
    }

    public class FileInfoModel : INotifyPropertyChanged
    {
        public List<FileInfo> FileInfos { get; } = new List<FileInfo>();

        public List<FileInfoDetail> FileInfoDetails => FileInfos.Select(fileInfo => fileInfo.FileInfoDetail).ToList();
        public int ItemsCount => FileInfoDetails.Count;

        public FileInfoModel()
        {
        }

        public FileInfoModel(IEnumerable<string> urls)
        {
            FileInfos.AddRange(urls.Select(url => new FileInfo(url)).ToList());
        }

        public void AddItems(IEnumerable<string> urls)
        {
            FileInfos.AddRange(urls.Select(url => new FileInfo(url)).ToList());

            OnPropertyChanged(nameof(FileInfoDetails));
            OnPropertyChanged(nameof(ItemsCount));
        }

        public void Clear()
        {
            FileInfos.Clear();

            OnPropertyChanged(nameof(FileInfoDetails));
            OnPropertyChanged(nameof(ItemsCount));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}