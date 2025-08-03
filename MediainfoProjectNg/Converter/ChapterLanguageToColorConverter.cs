using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;

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

            var chapterLanguages = chapterInfos
                .Select(chap => chap.Language ?? "")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            bool hasLanguageIssues = chapterLanguages.Count > 1 || (chapterLanguages.Count == 1 && chapterLanguages[0] == "");

            return hasLanguageIssues ? Brushes.Yellow : Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
