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
            if (targetType != typeof(Brush)) return null;
            if (!(value is VideoInfo info)) return null;
            if (info.FpsMode == "VFR") return Brushes.DarkViolet;
            switch (info.Fps)
            {
                case "23.976 (24000/1001)":
                    return DependencyProperty.UnsetValue;
                case "29.970 (30000/1001)":
                case "59.940 (60000/1001)":
                    return Brushes.Olive;
                case "23.976 (23976/1000)":
                case "29.970 (29970/1000)":
                    return Brushes.SlateBlue;
                default:
                    return Brushes.Maroon;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
