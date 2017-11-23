using System;
using System.Windows.Data;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.Converters
{
    /// <summary>
    /// Creates an Image from the given ImageSource path of a View.
    /// </summary>
    public class ViewIconConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var view = value as ModuleViewBase;
            return view == null ? null : new Image { Source = view.Icon };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
