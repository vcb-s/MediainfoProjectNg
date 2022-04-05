using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace mediainfo_project_ng.Converter
{
    [ValueConversion(typeof(VideoInfo), typeof(string))]
    public class FpsModeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) return null;
            if (!(value is VideoInfo info)) return null;
            return info.FpsMode == "VFR" ? "VFR" : info.Fps;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
