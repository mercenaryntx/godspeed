using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Extensions
{
    public static class ProgressViewModelExtensions
    {
        public static void NotifyProgressStarted(this IProgressViewModel vm)
        {
            UIThread.Run(() => vm.EventAggregator.GetEvent<AsyncProcessStartedEvent>().Publish(new AsyncProcessStartedEventArgs(vm)));
        }

        public static void NotifyProgressFinished(this IProgressViewModel vm)
        {
            UIThread.Run(() => vm.EventAggregator.GetEvent<AsyncProcessFinishedEvent>().Publish(new AsyncProcessFinishedEventArgs(vm)));
        }
    }
}