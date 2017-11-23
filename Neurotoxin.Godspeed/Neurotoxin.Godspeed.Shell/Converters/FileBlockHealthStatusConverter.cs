using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class FileBlockHealthStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var e = (FileBlockHealthStatus)value;
            Color c;
            switch (e)
            {
                case FileBlockHealthStatus.Ok:
                    c = Colors.PaleGreen;
                    break;
                case FileBlockHealthStatus.Collision:
                    c = Colors.PaleGoldenrod;
                    break;
                case FileBlockHealthStatus.Unallocated:
                    c = Colors.MediumPurple;
                    break;
                case FileBlockHealthStatus.Missing:
                    c = Colors.PaleVioletRed;
                    break;
                default:
                    throw new NotSupportedException("Invalid status code: " + e);
            }
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}