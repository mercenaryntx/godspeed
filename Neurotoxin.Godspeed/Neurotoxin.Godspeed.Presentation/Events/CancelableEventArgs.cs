namespace Neurotoxin.Godspeed.Presentation.Events
{
    public class CancelableEventArgs : EventArgsBase
    {
        public bool Cancelled { get; set; }

        public CancelableEventArgs(object sender) : base(sender)
        {
        }
    }
}
