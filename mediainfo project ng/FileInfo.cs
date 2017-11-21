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
using MediaInfoLib;

namespace mediainfo_project_ng
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

    public class FileInfo
    {
        public GeneralInfo GeneralInfo { get; } = new GeneralInfo();
        public List<VideoInfo> VideoInfos { get; } = new List<VideoInfo>();
        public List<AudioInfo> AudioInfos { get; } = new List<AudioInfo>();
        public string Summary { get; }

        public FileInfo(string url)
        {
            var MI = new MediaInfo();
            MI.Open(url);
            MI.Option("Complete");
            Summary = MI.Inform();
            {
                GeneralInfo.Filename = Path.GetFileNameWithoutExtension(url);
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

            MI.Close();
        }
    }
}