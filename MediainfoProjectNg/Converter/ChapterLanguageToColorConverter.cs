using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using MediainfoProjectNg;

namespace MediainfoProjectNg.Converter
{
    [ValueConversion(typeof(List<ChapterInfo>), typeof(Brush))]
    public class ChapterLanguageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return DependencyProperty.UnsetValue;
            if (value is not List<ChapterInfo> chapterInfos || chapterInfos.Count == 0)
                return DependencyProperty.UnsetValue;

            var firstLang = chapterInfos[0].Language ?? "";
            bool allSame = chapterInfos.All(chap =>
                string.Equals(chap.Language, firstLang, StringComparison.OrdinalIgnoreCase));

            return allSame ? Binding.DoNothing : Brushes.Yellow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
