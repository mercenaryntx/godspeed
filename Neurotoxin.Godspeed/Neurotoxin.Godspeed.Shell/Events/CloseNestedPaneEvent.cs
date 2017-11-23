using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class CloseNestedPaneEvent : CompositePresentationEvent<CloseNestedPaneEventArgs> { }

    public class CloseNestedPaneEventArgs
    {
        public IPaneViewModel Pane { get; private set; }
        public object Payload { get; private set; }

        public CloseNestedPaneEventArgs(IPaneViewModel pane, object payload)
        {
            Pane = pane;
            Payload = payload;
        }
    }
}