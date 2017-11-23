using System;
using System.Windows;
using System.Windows.Data;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class FileOperationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var e = (FileOperation)value;
            return e == FileOperation.Copy || e == FileOperation.Move
                       ? Visibility.Visible
                       : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}