using System;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public class SyncWorkHandler : IWorkHandler
    {
        public void Run(Action work)
        {
            work.Invoke();
        }

        public void Run<T>(Func<T> work, Action<T> success = null, Action<Exception> error = null)
        {
            try
            {
                var result = work.Invoke();
                if (success != null) success.Invoke(result);
            }
            catch (Exception ex)
            {
                if (error != null) error.Invoke(ex);
            }
        }
    }
}