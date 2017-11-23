using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.ViewModels
{
    public class ComboBoxItemViewModel<T> : ViewModelBase
    {
        private const string VALUE = "Value";
        private T _value;
        public T Value
        {
            get { return _value; }
            set { _value = value; NotifyPropertyChanged(VALUE); }
        }

        private const string ISSELECTABLE = "IsSelectable";
        private bool _isSelectable = true;
        public bool IsSelectable
        {
            get { return _isSelectable; }
            set { _isSelectable = value; NotifyPropertyChanged(ISSELECTABLE); }
        }

        public ComboBoxItemViewModel(T value)
        {
            _value = value;
        }
    }
}