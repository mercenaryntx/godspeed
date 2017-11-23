using System;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public class AsyncResult<TResult, TArguments> where TArguments : IAsyncCallArguments
    {
        public TResult Result { get; set; }
        public Exception Exception { get; set; }
        public TArguments Args { get; set; }
    }
}