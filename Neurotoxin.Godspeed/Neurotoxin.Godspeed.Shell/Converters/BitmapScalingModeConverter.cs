using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class BitmapScalingModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as FileSystemItemViewModel;
            return item != null && item.Thumbnail != null && !item.IsUpDirectory
                ? BitmapScalingMode.HighQuality
                : BitmapScalingMode.NearestNeighbor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}