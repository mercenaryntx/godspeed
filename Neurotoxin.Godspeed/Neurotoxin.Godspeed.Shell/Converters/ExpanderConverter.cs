using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class ExpanderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (Visibility) value;
            return v == Visibility.Visible ? "-" : "+";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}