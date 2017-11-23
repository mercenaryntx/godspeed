namespace Neurotoxin.Godspeed.Shell.Models
{
    public class LoadDataAsyncParameters
    {
        public FileListPaneSettings Settings { get; private set; }
        public object Payload { get; private set; }

        public LoadDataAsyncParameters(FileListPaneSettings settings, object payload = null)
        {
            Settings = settings;
            Payload = payload;
        }
    }
}
