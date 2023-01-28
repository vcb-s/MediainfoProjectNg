using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            var chapters = (List<ChapterInfo>)value;
            if (chapters.Count == 0)
            {
                return string.Empty;
            }

            var firstChapLang = chapters[0].Language;
            if (chapters.All(chapter => chapter.Language == firstChapLang))
            {
                if (firstChapLang == string.Empty)
                {
                    return "有";
                }
                return firstChapLang;
            }
            else
            {
                return "mix";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
