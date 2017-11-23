using System;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public class EventInformation<TEventArgsType> where TEventArgsType : EventArgs
    {
        public object Sender { get; set; }
        public TEventArgsType EventArgs { get; set; }
        public object CommandArgument { get; set; }
    }

}