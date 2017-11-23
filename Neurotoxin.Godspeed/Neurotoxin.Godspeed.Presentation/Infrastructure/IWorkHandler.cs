using System;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public interface IWorkHandler
    {
        void Run(Action work);
        void Run<T>(Func<T> work, Action<T> success = null, Action<Exception> error = null);
    }
}
