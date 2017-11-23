using System;
using System.Globalization;
using System.Windows.Data;

namespace Neurotoxin.Godspeed.Presentation.Converters
{
    public class StringFormatter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var format = parameter as string;
            return string.IsNullOrEmpty(format) ? string.Join(string.Empty, values) : string.Format(format, values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
