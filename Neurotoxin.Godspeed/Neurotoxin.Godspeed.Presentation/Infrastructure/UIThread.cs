using System;
using System.Windows;
using System.Windows.Threading;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public static class UIThread
    {
        private static readonly Dispatcher Dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;

        public static void BeginRun(Action work)
        {
            if (Dispatcher != null)
                Dispatcher.BeginInvoke(work);
            else
                work();
        }

        public static void Run(Action work)
        {
            if (Dispatcher != null && !Dispatcher.CheckAccess())
                Dispatcher.BeginInvoke(work);
            else
                work();
        }

        public static void Run<T>(Action<T> work, T p1)
        {
            if (Dispatcher != null && !Dispatcher.CheckAccess())
                Dispatcher.BeginInvoke(work, p1);
            else
                work(p1);
        }

        public static void RunSync(Action work)
        {
            if (Dispatcher != null && !Dispatcher.CheckAccess())
                Dispatcher.Invoke(work);
            else
                work();
        }
    }
}