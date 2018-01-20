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
    public class InfoToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return null;
            if (!(value is FileInfo info)) return null;

            return info.GeneralInfo.TextCount > 0 ? Brushes.Blue : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
