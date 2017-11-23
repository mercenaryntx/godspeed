using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class FreestyleDatabaseCheckedEvent : CompositePresentationEvent<FreestyleDatabaseCheckedEventArgs> { }

    public class FreestyleDatabaseCheckedEventArgs
    {
        public FreestyleDatabaseCheckerViewModel ViewModel { get; private set; }
        public string Message { get; private set; }

        public FreestyleDatabaseCheckedEventArgs(FreestyleDatabaseCheckerViewModel viewModel, string message)
        {
            ViewModel = viewModel;
            Message = message;
        }
    }
}