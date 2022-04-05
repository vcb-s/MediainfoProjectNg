using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace MediainfoProjectNg.Converter
{
    [ValueConversion(typeof(FileInfo), typeof(Brush))]
    public class InfoToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return null;
            if (!(value is FileInfo info)) return null;
            // TODO: Unify two checks
            var errorInfos = Utils.CheckFile(info);

            var errorInfo = errorInfos.FirstOrDefault();

            return errorInfo == null ? Brushes.White : errorInfo.Brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}