using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using MediainfoProjectNg;

namespace MediainfoProjectNg.Converter
{
    [ValueConversion(typeof(List<ChapterInfo>), typeof(string))]
    public class UnifiedLanguageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) return DependencyProperty.UnsetValue;

            if (value is List<ChapterInfo> chapterInfos && chapterInfos.Count > 0)
            {
                var firstLang = chapterInfos[0].Language ?? "";
                bool allSame = chapterInfos
                    .All(chap => string.Equals(chap.Language, firstLang, StringComparison.OrdinalIgnoreCase));

                return allSame ? firstLang : "";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
