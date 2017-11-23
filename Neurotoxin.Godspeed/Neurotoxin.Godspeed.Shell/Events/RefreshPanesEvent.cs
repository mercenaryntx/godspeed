using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class RefreshPanesEvent : CompositePresentationEvent<RefreshPanesEventArgs> { }

    public class RefreshPanesEventArgs : EventArgsBase
    {
        public RefreshPanesEventArgs(object sender) : base(sender)
        {
        }
    }
}