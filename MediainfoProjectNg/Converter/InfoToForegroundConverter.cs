using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MediainfoProjectNg.Converter
{
    [ValueConversion(typeof(FileInfo), typeof(Brush))]
    public class InfoToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return DependencyProperty.UnsetValue;
            if (value is not FileInfo info) return DependencyProperty.UnsetValue;

            return info.GeneralInfo.TextCount > 1 ? Brushes.Blue : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
