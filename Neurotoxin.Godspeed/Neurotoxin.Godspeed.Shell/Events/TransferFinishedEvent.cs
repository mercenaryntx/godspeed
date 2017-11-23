using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class TransferFinishedEvent : CompositePresentationEvent<TransferFinishedEventArgs> { }

    public class TransferFinishedEventArgs : EventArgsBase
    {
        public Shutdown Shutdown { get; set; }
        public FtpContentViewModel Ftp { get; set; }
        public IFileListPaneViewModel Source { get; private set; }
        public IFileListPaneViewModel Target { get; private set; }
        public bool IsAborted { get; set; }

        public TransferFinishedEventArgs(object sender, IFileListPaneViewModel source, IFileListPaneViewModel target) : base(sender)
        {
            Source = source;
            Target = target;
        }

    }
}