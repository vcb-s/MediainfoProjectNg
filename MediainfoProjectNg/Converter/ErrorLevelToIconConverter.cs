using System;
using System.Globalization;
using System.Windows.Data;

namespace MediainfoProjectNg.Converter
{
    [ValueConversion(typeof(object), typeof(object))]
    class ErrorLevelToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
