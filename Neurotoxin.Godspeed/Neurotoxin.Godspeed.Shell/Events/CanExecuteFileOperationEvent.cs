using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class CanExecuteFileOperationEvent : CompositePresentationEvent<CanExecuteFileOperationEventArgs> { }

    public class CanExecuteFileOperationEventArgs : CancelableEventArgs
    {
        public FileOperation Action { get; private set; }

        public CanExecuteFileOperationEventArgs(object sender, FileOperation action) : base(sender)
        {
            Action = action;
        }
    }
}