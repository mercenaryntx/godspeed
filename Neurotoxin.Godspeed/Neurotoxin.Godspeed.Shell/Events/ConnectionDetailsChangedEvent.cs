using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class ConnectionDetailsChangedEvent : CompositePresentationEvent<ConnectionDetailsChangedEventArgs> { }

    public class ConnectionDetailsChangedEventArgs
    {
        public IStoredConnectionViewModel Connection { get; private set; }

        public ConnectionDetailsChangedEventArgs(IStoredConnectionViewModel connection)
        {
            Connection = connection;
        }
    }
}