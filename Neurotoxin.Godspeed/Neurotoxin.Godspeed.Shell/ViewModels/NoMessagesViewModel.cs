using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class NoMessagesViewModel : ViewModelBase, IUserMessageViewModel
    {
        public string Message { get { return Resx.NoMessages; } }
        public bool IsRead { get; set; }

        public NoMessagesViewModel()
        {
            IsRead = true;
        }
    }
}