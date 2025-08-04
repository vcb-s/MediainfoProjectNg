using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using MediaInfoLib;

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

        public GeneralInfo(string filename, string fullPath, string format, long bitrate, long videoCount, long audioCount, long textCount, long chapterCount)
        {
            Filename = filename;
            FullPath = fullPath;
            Format = format;
            Bitrate = bitrate;
            VideoCount = videoCount;
            AudioCount = audioCount;
            TextCount = textCount;
            ChapterCount = chapterCount;
        }
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
        public string ColorSpace { get; set; }

        public VideoInfo(string format, string formatProfile, string fpsMode, string fps, long bitrate, long bitDepth, long duration, long height, long width, string language, long delay, ProfileInfo profile, string colorSpace)
        {
            Format = format;
            FormatProfile = formatProfile;
            FpsMode = fpsMode;
            Fps = fps;
            Bitrate = bitrate;
            BitDepth = bitDepth;
            Duration = duration;
            Height = height;
            Width = width;
            Language = language;
            Delay = delay;
            Profile = profile;
            ColorSpace = colorSpace;
        }
    }

    public class AudioInfo
    {
        public string Format { get; set; }
        public long BitDepth { get; set; }
        public long Bitrate { get; set; }
        public long Duration { get; set; }
        public string Language { get; set; }
        public long Delay { get; set; }

        public AudioInfo(string format, long bitDepth, long bitrate, long duration, string language, long delay)
        {
            Format = format;
            BitDepth = bitDepth;
            Bitrate = bitrate;
            Duration = duration;
            Language = language;
            Delay = delay;
        }
    }

    public class ChapterInfo
    {
        public int Timespan { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }

        public ChapterInfo(int timespan, string name, string language)
        {
            Timespan = timespan;
            Name = name;
            Language = language;
        }
    }

    public class ErrorInfo
    {
        public ErrorLevel Level { get; set; }
        public string Description { get; set; }
        public Brush Brush { get; set; }

        public ErrorInfo(ErrorLevel level, string description, Brush brush)
        {
            Level = level;
            Description = description;
            Brush = brush;
        }
    }

    public class FileInfo
    {
        public GeneralInfo GeneralInfo { get; }
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
            MediaInfo? MI = null;
            try
            {
                MI = new MediaInfo();
                MI.Open(url);
                MI.Option("Complete");
                Summary = MI.Inform();

                GeneralInfo = new GeneralInfo(
                    filename:     Path.GetFileNameWithoutExtension(url),
                    fullPath:     url,
                    format:       MI.Get(StreamKind.General, 0, "Format"),
                    bitrate:      MI.Get(StreamKind.General, 0, "OverallBitRate").TryParseAsLong() / 1000,
                    videoCount:   MI.Get(StreamKind.General, 0, "VideoCount").TryParseAsLong(),
                    audioCount:   MI.Get(StreamKind.General, 0, "AudioCount").TryParseAsLong(),
                    textCount:    MI.Get(StreamKind.General, 0, "TextCount").TryParseAsLong(),
                    chapterCount: -1
                );

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
                        break;
                }

                for (var i = 0; i < GeneralInfo.VideoCount; i++)
                {
                    var colorSpaceRaw = MI.Get(StreamKind.Video, i, "ColorSpace");
                    var chromaSubsampling = MI.Get(StreamKind.Video, i, "ChromaSubsampling");
                    string colorSpace = string.Empty;

                    if (!string.IsNullOrWhiteSpace(colorSpaceRaw) && !string.IsNullOrWhiteSpace(chromaSubsampling))
                    {
                        colorSpace = colorSpaceRaw.ToUpper() + chromaSubsampling.Replace(":", "");
                    }

                    VideoInfos.Add(new VideoInfo(
                        format:        MI.Get(StreamKind.Video, i, "Format"),
                        formatProfile: MI.Get(StreamKind.Video, i, "Format_Profile"),
                        fpsMode:       MI.Get(StreamKind.Video, i, "FrameRate_Mode"),
                        fps:           MI.Get(StreamKind.Video, i, "FrameRate/String").Replace(" FPS", ""),
                        bitrate:       MI.Get(StreamKind.Video, i, "BitRate").TryParseAsLong() / 1000,
                        bitDepth:      MI.Get(StreamKind.Video, i, "BitDepth").TryParseAsLong(),
                        duration:      MI.Get(StreamKind.Video, i, "Duration").TryParseAsLong(),
                        height:        MI.Get(StreamKind.Video, i, "Height").TryParseAsLong(),
                        width:         MI.Get(StreamKind.Video, i, "Width").TryParseAsLong(),
                        language:      MI.Get(StreamKind.Video, i, "Language/String3").ToUpper(),
                        delay:         MI.Get(StreamKind.Video, i, "Delay").TryParseAsLong(),
                        profile:       new ProfileInfo(MI.Get(StreamKind.Video, i, "Format_Profile")),
                        colorSpace:    colorSpace
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
                    AudioInfos.Add(new AudioInfo(
                        format:   MI.Get(StreamKind.Audio, i, "Format"),
                        bitDepth: MI.Get(StreamKind.Audio, i, "BitDepth").TryParseAsLong(),
                        bitrate:  MI.Get(StreamKind.Audio, i, "BitRate").TryParseAsLong() / 1000,
                        duration: MI.Get(StreamKind.Audio, i, "Duration").TryParseAsLong(),
                        language: MI.Get(StreamKind.Audio, i, "Language/String3").ToUpper(),
                        delay:    MI.Get(StreamKind.Audio, i, "Delay").TryParseAsLong()
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
                                    language = name.Substring(0, idx).Trim();
                                }
                            }
                            ChapterInfos.Add(new ChapterInfo(
                                timespan: MI.Get(StreamKind.Menu, 0, i, InfoKind.Name).TryParseAsMillisecond(),
                                language: language,
                                name:     name
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
