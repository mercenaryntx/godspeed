using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Godspeed.Presentation.Events
{
    public class RequestWindowCloseEvent : CompositePresentationEvent<RequestWindowCloseEventArgs> { }
    
    public class RequestWindowCloseEventArgs : EventArgsBase
    {
        public bool? DialogResult { get; private set; }

        public RequestWindowCloseEventArgs(object sender, bool? dialogRes) : base(sender)
        {
            DialogResult = dialogRes;
        }
    }
}