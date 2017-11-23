using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class ViewModelGeneratedEvent : CompositePresentationEvent<ViewModelGeneratedEventArgs> { }

    public class ViewModelGeneratedEventArgs
    {
        public ViewModelBase ViewModel { get; private set; }

        public ViewModelGeneratedEventArgs(ViewModelBase viewModel)
        {
            ViewModel = viewModel;
        }
    }
}