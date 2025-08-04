using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MediainfoProjectNg.Converter
{
    [ValueConversion(typeof(VideoInfo), typeof(Brush))]
    public class ColorSpaceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return DependencyProperty.UnsetValue;
            if (value is not VideoInfo info) return DependencyProperty.UnsetValue;
            switch (info.ColorSpace)
            {
                case "YUV420":
                    return Binding.DoNothing;
                default:
                    return Brushes.Orange;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
