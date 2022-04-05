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
        Info,
        Warning,
        Error
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
        public long Bitrate { get; set; }
        public long VideoCount { get; set; }
        public long AudioCount { get; set; }
        public long TextCount { get; set; }
        public long ChapterCount { get; set; }
    }

    // TODO: Using actual type instead of string
    public class VideoInfo
    {
        public string Format { get; set; }
        public string FormatProfile { get; set; }
        public string FpsMode { get; set; }
        public string Fps { get; set; }
        public long Bitrate { get; set; }
        public long BitDepth { get; set; }
        public long Duration { get; set; }
        public long Height { get; set; }
        public long Width { get; set; }
        public string Language { get; set; }
        public long Delay { get; set; }
        public ProfileInfo Profile { get; set; }
    }

    public class AudioInfo
    {
        public string Format { get; set; }
        public long BitDepth { get; set; }
        public long Bitrate { get; set; }
        public long Duration { get; set; }
        public string Language { get; set; }
        public long Delay { get; set; }
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
                GeneralInfo.Bitrate    = MI.Get(StreamKind.General, 0, "OverallBitRate").TryParseAsLong() / 1000;
                GeneralInfo.VideoCount = MI.Get(StreamKind.General, 0, "VideoCount").TryParseAsLong();
                GeneralInfo.AudioCount = MI.Get(StreamKind.General, 0, "AudioCount").TryParseAsLong();
                GeneralInfo.TextCount  = MI.Get(StreamKind.General, 0, "TextCount").TryParseAsLong();
                switch (MI.Get(StreamKind.General, 0, "MenuCount").TryParseAsLong())
                {
                    case 0:
                        GeneralInfo.ChapterCount = 0;
                        break;
                    case 1:
                        GeneralInfo.ChapterCount = MI.Get(StreamKind.Menu, 0, "Chapters_Pos_End").TryParseAsLong() -
                                                   MI.Get(StreamKind.Menu, 0, "Chapters_Pos_Begin").TryParseAsLong();
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
                        Bitrate       = MI.Get(StreamKind.Video, i, "BitRate").TryParseAsLong() / 1000,
                        BitDepth      = MI.Get(StreamKind.Video, i, "BitDepth").TryParseAsLong(),
                        Duration      = MI.Get(StreamKind.Video, i, "Duration").TryParseAsLong(),
                        Height        = MI.Get(StreamKind.Video, i, "Height").TryParseAsLong(),
                        Width         = MI.Get(StreamKind.Video, i, "Width").TryParseAsLong(),
                        Language      = MI.Get(StreamKind.Video, i, "Language/String3").ToUpper(),
                        Delay         = MI.Get(StreamKind.Video, i, "Delay").TryParseAsLong(),
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
                        BitDepth = MI.Get(StreamKind.Audio, i, "BitDepth").TryParseAsLong(),
                        Bitrate  = MI.Get(StreamKind.Audio, i, "BitRate").TryParseAsLong() / 1000,
                        Duration = MI.Get(StreamKind.Audio, i, "Duration").TryParseAsLong(),
                        Language = MI.Get(StreamKind.Audio, i, "Language/String3").ToUpper(),
                        Delay    = MI.Get(StreamKind.Audio, i, "Delay").TryParseAsLong()
                    });
                }

                if (GeneralInfo.ChapterCount > 0)
                {
                    var chapPosBegin = (int)MI.Get(StreamKind.Menu, 0, "Chapters_Pos_Begin").TryParseAsLong();
                    var chapPosEnd = (int)MI.Get(StreamKind.Menu, 0, "Chapters_Pos_End").TryParseAsLong();
                    for (var i = chapPosBegin; i < chapPosEnd; i++)
                    {
                        ChapterInfos.Add(new ChapterInfo
                        {
                            Timespan = MI.Get(StreamKind.Menu, 0, i, InfoKind.Name).TryParseAsMillisecond(),
                            Language = "",
                            Name     = MI.Get(StreamKind.Menu, 0, i, InfoKind.Text)
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