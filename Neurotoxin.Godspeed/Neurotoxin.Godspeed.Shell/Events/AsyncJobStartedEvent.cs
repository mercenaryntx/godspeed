using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class AsyncJobStartedEvent : CompositePresentationEvent<AsyncJobStartedEventArgs> { }

    public class AsyncJobStartedEventArgs : EventArgsBase
    {
        public AsyncJobStartedEventArgs(object sender) : base(sender)
        {
        }
    }
}