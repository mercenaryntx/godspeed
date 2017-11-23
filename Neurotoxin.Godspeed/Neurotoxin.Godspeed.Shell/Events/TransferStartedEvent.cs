using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class TransferStartedEvent : CompositePresentationEvent<TransferStartedEventArgs> { }

    public class TransferStartedEventArgs : EventArgsBase
    {
        public TransferStartedEventArgs(object sender) : base(sender)
        {
        }
    }
}