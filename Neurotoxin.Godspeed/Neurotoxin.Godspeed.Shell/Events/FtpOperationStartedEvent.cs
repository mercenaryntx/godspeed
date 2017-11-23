using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class FtpOperationStartedEvent : CompositePresentationEvent<FtpOperationStartedEventArgs> { }

    public class FtpOperationStartedEventArgs
    {
        public bool BinaryTransfer { get; private set; }

        public FtpOperationStartedEventArgs(bool binaryTransfer)
        {
            BinaryTransfer = binaryTransfer;
        }
    }
}