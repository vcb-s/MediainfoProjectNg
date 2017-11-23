using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace mediainfo_project_ng.Converter
{
    [ValueConversion(typeof(FileInfo), typeof(Brush))]
    public class InfoToForegroundConverter : IValueConverter
    {
        private static readonly string[] Matroska = { ".mkv", ".mka", ".mks" };
        private static readonly string[] MPEG_4 = { ".mp4", ".m4a", ".m4v" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return null;
            if (!(value is FileInfo info)) return null;
            var extension = Path.GetExtension(info.GeneralInfo.FullPath);

            if (info.GeneralInfo.Format == "Matroska" && !Matroska.Contains(extension)
                || info.GeneralInfo.Format == "MPEG-4" && !MPEG_4.Contains(extension))
            {
                return Brushes.White;
            }
            if (info.AudioInfos.Count > 2)
            {
                return Brushes.White;
            }

            if (info.GeneralInfo.TextCount > 0)
            {
                return Brushes.Blue;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
