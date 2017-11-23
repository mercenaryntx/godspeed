using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FileBlockViewModel : ViewModelBase
    {
        private const string BLOCKNUMBER = "BlockNumber";
        private int? _blockNumber;
        public int? BlockNumber
        {
            get { return _blockNumber; }
            set { _blockNumber = value; NotifyPropertyChanged(BLOCKNUMBER); }
        }

        private const string HEALTH = "Health";
        private FileBlockHealthStatus _health;
        public FileBlockHealthStatus Health
        {
            get { return _health; }
            set { _health = value; NotifyPropertyChanged(HEALTH); }
        }

        public FileBlockViewModel(int? blockNumber, FileBlockHealthStatus health)
        {
            _blockNumber = blockNumber;
            _health = health;
        }
    }
}