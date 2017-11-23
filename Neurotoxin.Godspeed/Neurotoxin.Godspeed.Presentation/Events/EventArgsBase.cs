namespace Neurotoxin.Godspeed.Presentation.Events
{
    public abstract class EventArgsBase : IPayload
    {
        public object Sender { get; private set; }

        protected EventArgsBase(object sender)
        {
            Sender = sender;
        }
    }

    public interface IPayload
    {
        object Sender { get; }
    }
}