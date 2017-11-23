using System;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public abstract class DialogViewModelBase<T> : ViewModelBase, IDialogViewModelBase
    {
        private bool _isClosing;

        #region Properties

        protected const string TITLE = "Title";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        protected const string MESSAGE = "Message";
        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; NotifyPropertyChanged(MESSAGE); }
        }

        protected abstract T DefaultResult { get; }

        public T DialogResult { get; private set; }

        #endregion

        #region Commands

        public DelegateCommand<T> CloseCommand { get; private set; }

        protected virtual bool CanExecuteCloseCommand(T payload)
        {
            return true;
        }

        protected virtual void ExecuteCloseCommand(T payload)
        {
            DialogResult = payload;
            if (_isClosing) return;
            _isClosing = true;
            NotifyClosing();
        }

        #endregion

        public event EventHandler Closing;

        private void NotifyClosing()
        {
            var handler = Closing;
            if (handler != null) handler.Invoke(this, new EventArgs());
        }

        protected DialogViewModelBase()
        {
            CloseCommand = new DelegateCommand<T>(ExecuteCloseCommand, CanExecuteCloseCommand);
        }

        public virtual void Close()
        {
            if (_isClosing) return;
            _isClosing = true;
            ExecuteCloseCommand(DefaultResult);
        }
    }
}