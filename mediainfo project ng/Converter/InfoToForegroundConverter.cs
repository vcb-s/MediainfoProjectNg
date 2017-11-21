using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;

namespace mediainfo_project_ng.Converter
{
    [ValueConversion(typeof(FileInfo), typeof(Brush))]
    public class InfoToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return null;
            var info = (FileInfo)value;
            if (info.GeneralInfo.Format == "Matroska" &&
                !new List<string> { ".mkv", ".mka", ".mks" }.Contains(Path.GetExtension(info.GeneralInfo.FullPath))
                || info.GeneralInfo.Format == "MPEG-4" &&
                !new List<string> { ".mp4", ".m4a", ".m4v" }.Contains(Path.GetExtension(info.GeneralInfo.FullPath)))
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
