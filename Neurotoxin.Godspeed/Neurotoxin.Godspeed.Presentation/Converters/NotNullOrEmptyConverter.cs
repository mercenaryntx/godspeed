using System;
using System.Windows.Data;
using System.Windows;

namespace Neurotoxin.Godspeed.Presentation.Converters
{
    /// <summary>
    /// Returns Visibility/Opacity/Bool values about the given value
    /// </summary>
    public class NotNullOrEmptyConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool v = value is string ? !String.IsNullOrEmpty((string)value) : (value != null);
            bool p = false;
            if (parameter != null)
            {
                if (parameter is string) Boolean.TryParse((string)parameter, out p);
                else if (parameter is bool) p = (bool)parameter;
                else p = value != null;
            }
            if (p) v = !v;

            if (targetType == typeof(Visibility)) return v ? Visibility.Visible : Visibility.Collapsed;
            if (targetType == typeof(int)) return v ? 1 : 0;
            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
