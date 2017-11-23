using System;
using System.Reflection;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public class DelegateCommand : IDelegateCommand
    {
        public static Action<MethodInfo> BeforeAction;

        private readonly Action _executeAction;
        private readonly Func<bool> _canExecuteAction;

        public DelegateCommand(Action executeAction, Func<bool> canExecuteAction = null)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public bool CanExecute(object parameter = null)
        {
            return _canExecuteAction == null || _canExecuteAction();
        }

        public void Execute(object parameter = null)
        {
            if (!CanExecute(parameter)) return;
            if (BeforeAction != null) BeforeAction(_executeAction.Method);
            _executeAction();
        }

        private void OnCanExecuteChanged(object sender, EventArgs args)
        {
            var handler = CanExecuteChanged;
            if (handler != null) handler(sender, args);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class DelegateCommand<T> : IDelegateCommand
    {
        private readonly Action<T> _executeAction;
        private readonly Func<T, bool> _canExecuteAction;

        public DelegateCommand(Action<T> executeAction, Func<T, bool> canExecuteAction = null)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteAction == null || _canExecuteAction((T)parameter);
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;
            if (DelegateCommand.BeforeAction != null) DelegateCommand.BeforeAction(_executeAction.Method);
            var type = typeof (T);
            _executeAction(type.IsValueType && parameter == null ? default(T) : (T)parameter);
        }

        private void OnCanExecuteChanged(object sender, EventArgs args)
        {
            var handler = CanExecuteChanged;
            if (handler != null) handler(sender, args);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }

}