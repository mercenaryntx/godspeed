using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class AsyncJobFinishedEvent : CompositePresentationEvent<AsyncJobFinishedEventArgs> { }

    public class AsyncJobFinishedEventArgs : EventArgsBase
    {
        public AsyncJobFinishedEventArgs(object sender) : base(sender)
        {
        }
    }
}