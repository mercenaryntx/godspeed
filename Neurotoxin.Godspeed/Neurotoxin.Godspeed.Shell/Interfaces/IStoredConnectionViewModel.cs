using System.Windows.Media;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IStoredConnectionViewModel
    {
        string Name { get; set; }
        ImageSource Thumbnail { get; }
    }
}