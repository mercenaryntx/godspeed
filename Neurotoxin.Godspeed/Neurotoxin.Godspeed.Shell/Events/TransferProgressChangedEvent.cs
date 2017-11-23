using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class TransferProgressChangedEvent : CompositePresentationEvent<TransferProgressChangedEventArgs> {}

    public class TransferProgressChangedEventArgs
    {
        public int Percentage { get; private set; }
        public long Transferred { get; private set; }
        public long TotalBytesTransferred { get; private set; }
        public long ResumeStartPosition { get; private set; }

        public TransferProgressChangedEventArgs(int percentage, long transferred, long totalBytesTransferred, long resumeStartPosition)
        {
            Percentage = percentage;
            Transferred = transferred;
            TotalBytesTransferred = totalBytesTransferred;
            ResumeStartPosition = resumeStartPosition;
        }
    }
}