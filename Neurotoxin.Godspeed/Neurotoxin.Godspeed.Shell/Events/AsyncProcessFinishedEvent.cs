using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class AsyncProcessFinishedEvent : CompositePresentationEvent<AsyncProcessFinishedEventArgs> { }

    public class AsyncProcessFinishedEventArgs
    {
        public IProgressViewModel ViewModel { get; private set; }

        public AsyncProcessFinishedEventArgs(IProgressViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}