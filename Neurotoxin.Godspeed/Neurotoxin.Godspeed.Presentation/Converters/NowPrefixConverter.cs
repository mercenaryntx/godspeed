using System;
using System.Windows.Data;

namespace Neurotoxin.Godspeed.Presentation.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class NowPrefixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "[" + DateTime.Now + "] " + value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
