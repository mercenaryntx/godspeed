using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class ActivePaneChangedEvent : CompositePresentationEvent<ActivePaneChangedEventArgs> { }

    public class ActivePaneChangedEventArgs
    {
        public IPaneViewModel ActivePane { get; private set; }

        public ActivePaneChangedEventArgs(IPaneViewModel activePane)
        {
            ActivePane = activePane;
        }
    }
}