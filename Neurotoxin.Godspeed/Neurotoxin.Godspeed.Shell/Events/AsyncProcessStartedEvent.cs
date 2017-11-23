using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class AsyncProcessStartedEvent : CompositePresentationEvent<AsyncProcessStartedEventArgs> { }

    public class AsyncProcessStartedEventArgs
    {
        public IProgressViewModel ViewModel { get; private set; }

        public AsyncProcessStartedEventArgs(IProgressViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}