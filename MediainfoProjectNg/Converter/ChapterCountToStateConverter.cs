using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediainfoProjectNg.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class ChapterCountToStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) return DependencyProperty.UnsetValue;
            var chapter = int.Parse(value?.ToString() ?? "0");
            return chapter != 0 ? "有" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
