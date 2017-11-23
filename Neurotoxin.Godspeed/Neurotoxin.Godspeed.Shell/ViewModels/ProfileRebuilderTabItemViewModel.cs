using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class ProfileRebuilderTabItemViewModel : ViewModelBase
    {
        private const string HEADER = "Header";
        public string _header;
        public string Header
        {
            get { return _header; }
            set { _header = value; NotifyPropertyChanged(HEADER); }
        }

        private const string CONTENT = "Content";
        public object _content;
        public object Content
        {
            get { return _content; }
            set { _content = value; NotifyPropertyChanged(CONTENT); }
        }

        public ProfileRebuilderTabItemViewModel(string header, object content)
        {
            _header = header;
            _content = content;
        }
    }
}