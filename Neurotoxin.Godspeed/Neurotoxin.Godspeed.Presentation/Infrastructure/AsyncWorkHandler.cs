using System;
using System.Windows;
using System.Windows.Threading;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public class AsyncWorkHandler : IWorkHandler
    {
        private static readonly Dispatcher Dispatcher = Application.Current.Dispatcher;

        public void Run(Action work)
        {
            work.BeginInvoke(asyncResult =>
            {
                try
                {
                    work.EndInvoke(asyncResult);
                }
                catch (Exception ex)
                {
                    UIThread.Run(() =>
                    {
                        throw ex;
                    });
                }

            }, null);
        }

        public void Run<T>(Func<T> work, Action<T> success = null, Action<Exception> error = null)
        {
            work.BeginInvoke(asyncResult =>
                {
                    try
                    {
                        var result = work.EndInvoke(asyncResult);
                        if (success != null) UIThread.Run(success, result);
                    }
                    catch (Exception ex)
                    {
                        if (error != null) UIThread.Run(error, ex);
                    }
                
            }, null);
        }
    }
}