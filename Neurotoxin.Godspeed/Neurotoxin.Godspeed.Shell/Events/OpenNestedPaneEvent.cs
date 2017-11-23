using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class OpenNestedPaneEvent : CompositePresentationEvent<OpenNestedPaneEventArgs> { }

    public class OpenNestedPaneEventArgs
    {
        public IPaneViewModel Opener { get; private set; }
        public IPaneViewModel Openee { get; private set; }

        public OpenNestedPaneEventArgs(IPaneViewModel opener, IPaneViewModel openee)
        {
            Opener = opener;
            Openee = openee;
        }
    }
}