using System;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Helpers;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FtpTraceViewModel : ViewModelBase
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly FtpTraceListener _traceListener;
        private bool _isClosing;

        private const string LOG = "Log";
        private string _log;
        public string Log
        {
            get { return _log; }
            set { _log = value; NotifyPropertyChanged(LOG); }
        }

        private const string TITLE = "Title";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        #region ClosingCommand

        public DelegateCommand ClosingCommand { get; private set; }

        private void ExecuteClosingCommand()
        {
            if (_isClosing) return;
            ExecuteCloseCommand(true);
        }

        #endregion

        #region CloseCommand

        public DelegateCommand<bool> CloseCommand { get; private set; }

        private void ExecuteCloseCommand(bool isClosing)
        {
            if (!isClosing) _isClosing = true;
            EventAggregator.GetEvent<CloseFtpTraceWindowEvent>().Publish(new CloseFtpTraceWindowEventArgs(_traceListener, isClosing));
        }

        #endregion

        public FtpTraceViewModel(FtpTraceListener traceListener, string connectionName)
        {
            _traceListener = traceListener;
            var log = traceListener.Log;
            for (var i = log.Count - 1; i >= 0; i--)
            {
                _stringBuilder.Append(log.ElementAt(i));
            }
            Log = _stringBuilder.ToString();
            Title = string.Format(Resx.FtpTraceWindowTitle, connectionName);
            _traceListener.LogChanged += TraceListenerOnLogChanged;

            CloseCommand = new DelegateCommand<bool>(ExecuteCloseCommand);
            ClosingCommand = new DelegateCommand(ExecuteClosingCommand);
        }

        private void TraceListenerOnLogChanged(object sender, LogChangedEventArgs e)
        {
            _stringBuilder.Append(e.Message);
            Log = _stringBuilder.ToString();
        }

        public override void Dispose()
        {
            base.Dispose();
            _traceListener.LogChanged -= TraceListenerOnLogChanged;
        }
    }
}