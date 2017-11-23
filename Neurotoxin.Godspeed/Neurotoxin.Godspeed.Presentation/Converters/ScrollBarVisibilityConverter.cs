using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;

namespace Neurotoxin.Godspeed.Presentation.Converters
{
    public class ScrollBarVisibilityConverter : IMultiValueConverter
    {
        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var element = values[0] as ScrollViewer;
            var control = values[1] as ContentControl;
            if (element == null || control == null) return ScrollBarVisibility.Auto;
            var content = control.Content as FrameworkElement;
            if (content == null) return ScrollBarVisibility.Auto;
            var p = parameter as string;
            if (p == "Width") return (element.ActualWidth < content.MinWidth) ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            if (p == "Height") return (element.ActualHeight < content.MinHeight) ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            return ScrollBarVisibility.Auto;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[] { };
        }

        #endregion
    }
}