using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Neurotoxin.Godspeed.Core.Constants;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class ContentTypeConverter : IValueConverter
    {
        //private static Dictionary<ContentType, Color> _colors = new Dictionary<ContentType, Color>
        //                                                            {
        //                                                                { ContentType.SavedGame, Colors.LightBlue },
        //                                                                { ContentType.DownloadableContent, Colors.LightGreen },
        //                                                                { ContentType.Publisher, Colors.AliceBlue },

        //                                                                { ContentType.Xbox360Title, Colors.LightCoral },
        //                                                                { ContentType.IptvPauseBuffer, Colors.Beige },
        //                                                                { ContentType.InstalledGame, Colors.LightSkyBlue },
        //                                                                { ContentType.XboxOriginalGame, Colors.AntiqueWhite },
        //                                                                { ContentType.GameOnDemand, Colors.LightSeaGreen },
        //                                                                { ContentType.AvatarAssetPack, Colors.LightGoldenrodYellow },
        //                                                                { ContentType.AvatarItem, Colors.LightSalmon },

        //                                                                { ContentType.Profile, Colors.PaleGoldenrod },
        //                                                                { ContentType.GamerPicture, Colors.PaleGreen },
        //                                                                { ContentType.Theme, Colors.PaleTurquoise },
        //                                                                { ContentType.CacheFile, Colors.LightGray },
        //                                                                { ContentType.StorageDownload, Colors.LemonChiffon },
        //                                                                { ContentType.XboxSavedGame, Colors.LavenderBlush },
        //                                                                { ContentType.XboxDownload, Colors.PapayaWhip },
        //                                                                { ContentType.GameDemo, Colors.MistyRose },
        //                                                                { ContentType.Video, Colors.Honeydew },
        //                                                                { ContentType.XboxLiveArcadeGame, Colors.PaleVioletRed },
        //                                                                { ContentType.GamerTitle, Colors.Wheat },
        //                                                                { ContentType.TitleUpdate, Colors.LightPink },
        //                                                                { ContentType.GameTrailer, Colors.Thistle },
        //                                                                { ContentType.XNA, Colors.SeaShell },
        //                                                                { ContentType.LicenseStore, Colors.Gainsboro },

        //                                                                { ContentType.Movie, Colors.Bisque },
        //                                                                { ContentType.Television, Colors.Cornsilk },
        //                                                                { ContentType.MusicVideo, Colors.LightYellow },
        //                                                                { ContentType.GameVideo, Colors.WhiteSmoke },
        //                                                                { ContentType.PodcastVideo, Colors.MintCream },
        //                                                                { ContentType.ViralVideo, Colors.Lavender },
        //                                                                { ContentType.CommunityGame, Colors.NavajoWhite },
        //                                                            }; 

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Resx.ResourceManager.GetString(value + "Singular");
            //return typeof (Brush).IsAssignableFrom(targetType)
            //           ? (object) new SolidColorBrush(_colors[(ContentType) value]) { Opacity = 0.7 }
            //           : Resx.ResourceManager.GetString(value + "Singular");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
