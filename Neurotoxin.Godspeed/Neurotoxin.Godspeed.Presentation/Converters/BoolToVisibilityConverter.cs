using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace Neurotoxin.Godspeed.Presentation.Converters
{
    /// <summary>
    /// Bool to Visibility Converter. If converter parameter is set to true the result will be inversed.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter, IMultiValueConverter
    {
        private static bool ParseParameter(object parameter)
        {
            if (parameter == null) return false;

            var p = false;
            var s = parameter as string;
            if (s != null) Boolean.TryParse(s, out p);
            else if (parameter is bool) p = (bool)parameter;
            return p;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof (Visibility))
            {
                var v = false;
                if (value != null) v = !(value is bool) || (bool)value;
                if (ParseParameter(parameter)) v = !v;
                return (v ? Visibility.Visible : Visibility.Collapsed);
            }
            if (targetType == typeof(bool) || Nullable.GetUnderlyingType(targetType) == typeof(bool))
            {
                if (value is Visibility)
                {
                    return ((Visibility) value) == Visibility.Visible;
                }
                throw new NotSupportedException();
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //TODO: or
            //TODO: inverse

            var v = values.All(value => value != DependencyProperty.UnsetValue && value != null && (!(value is bool) || (bool) value));
            if (ParseParameter(parameter)) v = !v;
            return (v ? Visibility.Visible : Visibility.Collapsed);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}