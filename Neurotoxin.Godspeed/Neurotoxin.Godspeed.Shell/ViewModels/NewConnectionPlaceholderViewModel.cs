using System.Windows.Media;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class NewConnectionPlaceholderViewModel : CommonViewModelBase, IStoredConnectionViewModel
    {
        public string Name { get; set; }
        public ImageSource Thumbnail { get; private set; }

        public NewConnectionPlaceholderViewModel()
        {
            var thumbnail = ResourceManager.GetContentByteArray("/Resources/Connections/AddConnection.png");
            Thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(thumbnail);
            Name = Resx.NewConnection + Strings.DotDotDot;
        }
    }
}