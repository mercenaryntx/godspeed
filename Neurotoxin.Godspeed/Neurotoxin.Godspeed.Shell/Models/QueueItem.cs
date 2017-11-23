using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class QueueItem
    {
        public FileSystemItem FileSystemItem { get; private set; }
        public FileOperation Operation { get; set; }
        public CopyAction? CopyAction { get; set; }
        public object Payload { get; set; }
        public bool Confirmation { get; set; }

        public QueueItem(FileSystemItem fileSystemItem, FileOperation fileOperation, object payload = null)
        {
            FileSystemItem = fileSystemItem;
            Operation = fileOperation;
            Payload = payload;
        }
    }
}
