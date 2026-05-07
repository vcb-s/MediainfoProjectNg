using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MediainfoProjectNg.Converter
{
    [ValueConversion(typeof(VideoInfo), typeof(Brush))]
    public class FpsToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return DependencyProperty.UnsetValue;
            if (value is not VideoInfo info) return DependencyProperty.UnsetValue;
            if (info.FpsMode == "VFR") return Brushes.DarkViolet;
            return info.Fps switch
            {
                "23.976 (24000/1001)" => DependencyProperty.UnsetValue,
                "29.970 (30000/1001)" or "59.940 (60000/1001)" => Brushes.Olive,
                "23.976 (23976/1000)" or "29.970 (29970/1000)" => Brushes.SlateBlue,
                _ => Brushes.Maroon,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
