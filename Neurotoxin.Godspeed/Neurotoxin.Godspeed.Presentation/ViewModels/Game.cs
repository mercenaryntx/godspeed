using System.Windows.Media;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.ViewModels
{
    public class Game : ViewModelBase
    {
        //UNDONE
        public string TitleId { get; set; }
        public string Title { get; set; }
        public string Achievements { get; set; }
        public string Gamerscore { get; set; }
        public ImageSource Thumbnail { get; set; }
    }
}