using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class PaneHeaderBackgroundConverter : DependencyObject, IValueConverter
    {
        public Brush ActivePaneBrush
        {
            get { return (Brush)GetValue(ActivePaneBrushProperty); }
            set { SetValue(ActivePaneBrushProperty, value); }
        }

        public static readonly DependencyProperty ActivePaneBrushProperty = DependencyProperty.Register("ActivePaneBrush", typeof(Brush), typeof(PaneHeaderBackgroundConverter));

        public Brush InactivePaneBrush
        {
            get { return (Brush)GetValue(InactivePaneBrushProperty); }
            set { SetValue(InactivePaneBrushProperty, value); }
        }

        public static readonly DependencyProperty InactivePaneBrushProperty = DependencyProperty.Register("InactivePaneBrush", typeof(Brush), typeof(PaneHeaderBackgroundConverter));

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var isActive = (bool) value;
            return isActive ? ActivePaneBrush : InactivePaneBrush; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}