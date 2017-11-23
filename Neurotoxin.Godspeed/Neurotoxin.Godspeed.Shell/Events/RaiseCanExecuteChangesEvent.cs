using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class RaiseCanExecuteChangesEvent : CompositePresentationEvent<RaiseCanExecuteChangesEventArgs> { }

    public class RaiseCanExecuteChangesEventArgs : EventArgsBase
    {
        public RaiseCanExecuteChangesEventArgs(object sender) : base(sender)
        {
        }
    }
}