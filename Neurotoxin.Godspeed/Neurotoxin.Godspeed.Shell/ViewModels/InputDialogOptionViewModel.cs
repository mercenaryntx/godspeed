using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class InputDialogOptionViewModel : ViewModelBase
    {

        private const string VALUE = "Value";
        private object _value;
        public object Value
        {
            get { return _value; }
            set { _value = value; NotifyPropertyChanged(VALUE); }
        }

        private const string DISPLAYNAME = "DisplayName";
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; NotifyPropertyChanged(DISPLAYNAME); }
        }

        private const string ISSELECTED = "IsSelected";
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyPropertyChanged(ISSELECTED); }
        }
    }
}