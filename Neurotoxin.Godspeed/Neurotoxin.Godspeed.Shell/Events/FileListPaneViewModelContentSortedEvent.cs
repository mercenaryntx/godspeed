using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class FileListPaneViewModelContentSortedEvent : CompositePresentationEvent<FileListPaneViewModelContentSortedEventArgs>
    {
    }

    public class FileListPaneViewModelContentSortedEventArgs : EventArgsBase
    {
        public FileListPaneViewModelContentSortedEventArgs(object sender) : base(sender)
        {
        }
    }
}
