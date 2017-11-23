using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class InputDialogViewModel : ViewModelBase
    {
        private const string TITLE = "Title";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        private const string MESSAGE = "Message";
        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; NotifyPropertyChanged(MESSAGE); }
        }

        private const string OPTIONS = "Options";
        private IList<InputDialogOptionViewModel> _options;
        public IList<InputDialogOptionViewModel> Options
        {
            get { return _options; }
            set { _options = value; NotifyPropertyChanged(OPTIONS); }
        }

        private const string DEFAULTVALUE = "DefaultValue";
        private object _defaultValue;
        public object DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; NotifyPropertyChanged(DEFAULTVALUE); }
        }

        private const string MODE = "Mode";
        private InputDialogMode _mode;
        public InputDialogMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                NotifyPropertyChanged(MODE);
                NotifyPropertyChanged(ISTEXTBOX);
                NotifyPropertyChanged(ISCOMBOBOX);
                NotifyPropertyChanged(ISRADIOGROUP);
            }
        }

        private const string ISTEXTBOX = "IsTextBox";
        public bool IsTextBox
        {
            get { return _mode == InputDialogMode.Text; }
        }

        private const string ISCOMBOBOX = "IsComboBox";
        public bool IsComboBox
        {
            get { return _mode == InputDialogMode.ComboBox; }
        }

        private const string ISRADIOGROUP = "IsRadioGroup";
        public bool IsRadioGroup
        {
            get { return _mode == InputDialogMode.RadioGroup; }
        }

    }
}
