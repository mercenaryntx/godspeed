using System;
using System.Windows.Media;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.ViewModels
{
    public class Achievement : ViewModelBase
    {
        //UNDONE
        public int AchievementId { get; set; }
        public ImageSource Thumbnail { get; set; }
        public int Gamerscore { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public DateTime UnlockTime { get; set; }
    }
}