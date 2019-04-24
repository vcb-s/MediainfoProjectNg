using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using MediaInfoLib;

namespace mediainfo_project_ng
{
    public enum ErrorLevel
    {
        info,
        warning,
        error
    }

    public class ProfileInfo
    {
        public string Profile { get; }
        public string Level { get; }
        public ProfileInfo(string profileString)
        {
            var strs = profileString.Split('@');
            if (strs.Length > 0)
                Profile = strs[0];
            if (strs.Length > 1)
                Level = strs[1];
        }
    }

    public class GeneralInfo
    {
        public string Filename { get; set; }
        public string FullPath { get; set; }
        public string Format { get; set; }
        public int Bitrate { get; set; }
        public int VideoCount { get; set; }
        public int AudioCount { get; set; }
        public int TextCount { get; set; }
        public int ChapterCount { get; set; }
    }

    // TODO: Using actual type instead of string
    public class VideoInfo
    {
        public string Format { get; set; }
        public string FormatProfile { get; set; }
        public string FpsMode { get; set; }
        public string Fps { get; set; }
        public int Bitrate { get; set; }
        public int BitDepth { get; set; }
        public int Duration { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Language { get; set; }
        public int Delay { get; set; }
        public ProfileInfo Profile { get; set; }
    }

    public class AudioInfo
    {
        public string Format { get; set; }
        public int BitDepth { get; set; }
        public int Bitrate { get; set; }
        public int Duration { get; set; }
        public string Language { get; set; }
        public int Delay { get; set; }
    }

    public class ChapterInfo
    {
        public int Timespan { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
    }

    public class ErrorInfo
    {
        public ErrorLevel Level { get; set; }
        public string Description { get; set; }
        public Brush Brush { get; set; }
    }

    public class FileInfo
    {
        public GeneralInfo GeneralInfo { get; } = new GeneralInfo();
        public List<VideoInfo> VideoInfos { get; } = new List<VideoInfo>();
        public List<AudioInfo> AudioInfos { get; } = new List<AudioInfo>();
        public List<ChapterInfo> ChapterInfos { get; } = new List<ChapterInfo>();
//        public List<ErrorInfo> ErrorInfos { get; set; } = null;
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
                GeneralInfo.Bitrate    = MI.Get(StreamKind.General, 0, "OverallBitRate").TryParseAsInt() / 1000;
                GeneralInfo.VideoCount = MI.Get(StreamKind.General, 0, "VideoCount").TryParseAsInt();
                GeneralInfo.AudioCount = MI.Get(StreamKind.General, 0, "AudioCount").TryParseAsInt();
                GeneralInfo.TextCount  = MI.Get(StreamKind.General, 0, "TextCount").TryParseAsInt();
                switch (MI.Get(StreamKind.General, 0, "MenuCount").TryParseAsInt())
                {
                    case 0:
                        GeneralInfo.ChapterCount = 0;
                        break;
                    case 1:
                        GeneralInfo.ChapterCount = MI.Get(StreamKind.Menu, 0, "Chapters_Pos_End").TryParseAsInt() -
                                                   MI.Get(StreamKind.Menu, 0, "Chapters_Pos_Begin").TryParseAsInt();
                        break;
                    default:
                        GeneralInfo.ChapterCount = -1;
                        break;
                }

                for (var i = 0; i < GeneralInfo.VideoCount; i++)
                {
                    VideoInfos.Add(new VideoInfo
                    {
                        Format        = MI.Get(StreamKind.Video, i, "Format"),
                        FormatProfile = MI.Get(StreamKind.Video, i, "Format_Profile"),
                        FpsMode       = MI.Get(StreamKind.Video, i, "FrameRate_Mode"),
                        Fps           = MI.Get(StreamKind.Video, i, "FrameRate/String").Replace(" FPS", ""),
                        Bitrate       = MI.Get(StreamKind.Video, i, "BitRate").TryParseAsInt() / 1000,
                        BitDepth      = MI.Get(StreamKind.Video, i, "BitDepth").TryParseAsInt(),
                        Duration      = MI.Get(StreamKind.Video, i, "Duration").TryParseAsInt(),
                        Height        = MI.Get(StreamKind.Video, i, "Height").TryParseAsInt(),
                        Width         = MI.Get(StreamKind.Video, i, "Width").TryParseAsInt(),
                        Language      = MI.Get(StreamKind.Video, i, "Language/String3").ToUpper(),
                        Delay         = MI.Get(StreamKind.Video, i, "Delay").TryParseAsInt(),
                        Profile       = new ProfileInfo(MI.Get(StreamKind.Video, i, "Format_Profile"))
                    });
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
                    AudioInfos.Add(new AudioInfo
                    {
                        Format   = MI.Get(StreamKind.Audio, i, "Format"),
                        BitDepth = MI.Get(StreamKind.Audio, i, "BitDepth").TryParseAsInt(),
                        Bitrate  = MI.Get(StreamKind.Audio, i, "BitRate").TryParseAsInt() / 1000,
                        Duration = MI.Get(StreamKind.Audio, i, "Duration").TryParseAsInt(),
                        Language = MI.Get(StreamKind.Audio, i, "Language/String3").ToUpper(),
                        Delay    = MI.Get(StreamKind.Audio, i, "Delay").TryParseAsInt()
                    });
                }

                if (GeneralInfo.ChapterCount > 0)
                {
                    for (var i = MI.Get(StreamKind.Menu, 0, "Chapters_Pos_Begin").TryParseAsInt();
                        i < MI.Get(StreamKind.Menu, 0, "Chapters_Pos_End").TryParseAsInt();
                        i++)
                    {
                        var a = GeneralInfo.Format == "Matroska" ? MI.Get(StreamKind.Menu, 0, i, InfoKind.Text).Split(new[] {':'}, 2) : new[] {"", MI.Get(StreamKind.Menu, 0, i, InfoKind.Text)};
                        ChapterInfos.Add(new ChapterInfo
                        {
                            Timespan = MI.Get(StreamKind.Menu, 0, i, InfoKind.Name).TryParseAsMillisecond(),
                            Language  = a[0],
                            Name      = a[1]
                        });
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