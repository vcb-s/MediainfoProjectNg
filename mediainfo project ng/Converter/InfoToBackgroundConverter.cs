using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace mediainfo_project_ng.Converter
{
    [ValueConversion(typeof(FileInfo), typeof(Brush))]
    public class InfoToBackgroundConverter : IValueConverter
    {
        private static readonly string[] Matroska = {".mkv", ".mka", ".mks"};
        private static readonly string[] MPEG_4 = {".mp4", ".m4a", ".m4v"};

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return null;
            if (!(value is FileInfo info)) return null;
            var extension = Path.GetExtension(info.GeneralInfo.FullPath);

            if (info.GeneralInfo.Format == "Matroska" && !Matroska.Contains(extension)
                || info.GeneralInfo.Format == "MPEG-4" && !MPEG_4.Contains(extension))
            {
                return Brushes.Red;
            }

            var duration = new List<int>();
            duration.AddRange(info.VideoInfos.Select(videoInfo => videoInfo.Duration));
            duration.AddRange(info.AudioInfos.Select(audioInfo => audioInfo.Duration));
            if (duration.Count > 0)
            {
                if (duration.Max() - duration.Min() > 600)
                {
                    return Brushes.PaleVioletRed;
                }

                if (info.GeneralInfo.ChapterCount != 0 &&
                    (info.GeneralInfo.ChapterCount == 1 || info.GeneralInfo.ChapterCount == -1 ||
                     info.ChapterInfos.Last().Timespan > duration.Max() - 1100 || info.ChapterInfos.First().Timespan != 0))
                {
                    return Brushes.Yellow;
                }
            }

            if (info.AudioInfos.Count > 2)
            {
                return Brushes.GreenYellow;
            }

            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
