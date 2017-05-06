using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using media_info_project_ng.Annotations;
using MediaInfoLib;

namespace media_info_project_ng
{
    public class GeneralInfo
    {
        public string Filename { get; set; }
        public string Format { get; set; }
        public string Bitrate { get; set; }
        public int VideoCount { get; set; }
        public int AudioCount { get; set; }
    }

    public class VideoInfo
    {
        public int TrackId { get; set; }
        public string Fps { get; set; }
        public string Bitrate { get; set; }
    }

    public class AudioInfo
    {
        public int TrackId { get; set; }
        public string Format { get; set; }
        public string BitDepth { get; set; }
        public string Bitrate { get; set; }
    }

    public class FileInfoDetail
    {
        public string Filename { get; set; }
        public string Fps { get; set; }
        public string Format1 { get; set; }
        public string Format2 { get; set; }
    }

    public class FileInfo
    {
        private GeneralInfo GeneralInfo { get; } = new GeneralInfo();
        private List<VideoInfo> VideoInfos { get; } = new List<VideoInfo>();
        private List<AudioInfo> AudioInfos { get; } = new List<AudioInfo>();
        public string Summary { get; }

        // TODO: Is it better to use property than method?
        public FileInfoDetail GetDetail() => new FileInfoDetail
        {
            Filename = GeneralInfo.Filename,
            // TODO: more elegant
            Fps = GeneralInfo.VideoCount > 0 ? VideoInfos[0].Fps : "",
            Format1 = GeneralInfo.AudioCount > 0 ? AudioInfos[0]?.Format : "",
            Format2 = GeneralInfo.AudioCount > 1 ? AudioInfos[1]?.Format : ""
        };

        public FileInfo(string url)
        {
            var MI = new MediaInfo();
            MI.Open(url);
            MI.Option("Complete");
            Summary = MI.Inform();
            {
                GeneralInfo.Filename = MI.Get(StreamKind.General, 0, "FileName");
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
            }

            for (var i = 0; i < GeneralInfo.VideoCount; i++)
            {
                VideoInfos.Add(new VideoInfo
                {
                    TrackId = i,
                    Fps = MI.Get(StreamKind.Video, 0, "FrameRate/String"),
                    Bitrate = MI.Get(StreamKind.Video, 0, "BitRate")
                });
            }

            for (var i = 0; i < GeneralInfo.AudioCount; i++)
            {
                AudioInfos.Add(new AudioInfo
                {
                    TrackId = i,
                    Format = MI.Get(StreamKind.Audio, i, "Format"),
                    BitDepth = MI.Get(StreamKind.Audio, i, "BitDepth/String"),
                    Bitrate = MI.Get(StreamKind.Audio, i, "BitRate") == ""
                        ? ""
                        : (int.Parse(MI.Get(StreamKind.Audio, i, "BitRate")) / 1000).ToString()
                });
            }
        }
    }

    public class FileInfoModel : INotifyPropertyChanged
    {
        public List<FileInfo> FileInfos { get; } = new List<FileInfo>();

        public List<FileInfoDetail> FileInfoDetails => FileInfos.Select(fileInfo => fileInfo.GetDetail()).ToList();
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