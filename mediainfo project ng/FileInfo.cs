using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
#if DEBUG
            var length = new System.IO.FileInfo(url).Length;
            var sw = new Stopwatch();
            sw.Start();
#endif
            MediaInfo MI = null;
            try
            {
                MI = new MediaInfo();
                MI.Open(url);
                MI.Option("Complete");
                Summary = MI.Inform();

                GeneralInfo.Filename   = Path.GetFileNameWithoutExtension(url);
                GeneralInfo.FullPath   = url;
                GeneralInfo.Format     = MI.Get(StreamKind.General, 0, "Format");
                GeneralInfo.Bitrate    = MI.Get(StreamKind.General, 0, "OverallBitRate/String");
                GeneralInfo.VideoCount = MI.Get(StreamKind.General, 0, "VideoCount").TryParseAsInt();
                GeneralInfo.AudioCount = MI.Get(StreamKind.General, 0, "AudioCount").TryParseAsInt();
                GeneralInfo.TextCount  = MI.Get(StreamKind.General, 0, "TextCount").TryParseAsInt();
                GeneralInfo.MenuCount  = MI.Get(StreamKind.General, 0, "MenuCount").TryParseAsInt();

                for (var i = 0; i < GeneralInfo.VideoCount; i++)
                {
                    VideoInfos.Add(new VideoInfo
                    {
                        Format        = MI.Get(StreamKind.Video, i, "Format"),
                        FormatProfile = MI.Get(StreamKind.Video, i, "Format_Profile"),
                        Fps           = MI.Get(StreamKind.Video, i, "FrameRate/String").Replace(" FPS", ""),
                        Bitrate       = MI.Get(StreamKind.Video, i, "BitRate"),
                        BitDepth      = MI.Get(StreamKind.Video, i, "BitDepth"),
                        Duration      = MI.Get(StreamKind.Video, i, "Duration"),
                        Height        = MI.Get(StreamKind.Video, i, "Height"),
                        Width         = MI.Get(StreamKind.Video, i, "Width"),
                        Language      = MI.Get(StreamKind.Video, i, "Language/String3").ToUpper()
                    });
                }

                for (var i = 0; i < GeneralInfo.AudioCount; i++)
                {
                    AudioInfos.Add(new AudioInfo
                    {
                        Format   = MI.Get(StreamKind.Audio, i, "Format"),
                        BitDepth = MI.Get(StreamKind.Audio, i, "BitDepth"),
                        Bitrate  = (MI.Get(StreamKind.Audio, i, "BitRate").TryParseAsInt() / 1000).ToString(),
                        Language = MI.Get(StreamKind.Audio, i, "Language/String3").ToUpper()
                    });
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