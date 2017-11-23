using System;
using System.Management;
using System.Threading;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class ShutdownDialogViewModel : DialogViewModelBase<Shutdown?>
    {
        private readonly IWindowManager _windowManager;
        private readonly FtpContentViewModel _ftp;
        private readonly string _innerText;
        private Timer _timer;
        private TimeSpan _counter;

        private const string MODE = "Mode";
        private Shutdown? _mode;
        public Shutdown? Mode
        {
            get { return _mode; }
            set { _mode = value; NotifyPropertyChanged(MODE); NotifyPropertyChanged(ISBOTH); }
        }

        private const string ISBOTH = "IsBoth";
        public bool IsBoth
        {
            get { return Mode == Shutdown.Both; }
        }

        private const string CAPTION = "Caption";
        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set { _caption = value; NotifyPropertyChanged(CAPTION); }
        }

        protected override Shutdown? DefaultResult
        {
            get { return Shutdown.Disabled; }
        }

        protected override void ExecuteCloseCommand(Shutdown? payload)
        {
            _timer.Dispose();
            _timer = null;
            _windowManager.CloseWindowOf(GetType());

            var v = payload ?? Mode ?? Shutdown.Disabled;
            if (v.HasFlag(Shutdown.PC)) ShutdownComputer();
            if (v.HasFlag(Shutdown.Xbox)) _ftp.Shutdown();
        }

        public ShutdownDialogViewModel(IWindowManager windowManager, Shutdown mode, FtpContentViewModel ftp = null)
        {
            _windowManager = windowManager;
            _ftp = ftp;
            Mode = mode;
            Title = Resx.Shutdown;
            switch (mode)
            {
                case Shutdown.Both:
                    _innerText = Resx.ShutdownDialogModeComputerAndXbox;
                    break;
                case Shutdown.PC:
                    _innerText = Resx.ShutdownDialogModeComputer;
                    break;
                case Shutdown.Xbox:
                    _innerText = Resx.ShutdownDialogModeXbox;
                    break;
                default:
                    throw new NotSupportedException("Invalid Shutdown mode: " + mode);
            }
            Caption = string.Format(Resx.ShutdownDialogCaption, _innerText);
            _counter = new TimeSpan(0, 0, 30); //TODO: config
            _timer = new Timer(TimerCallback, null, 0, 1000);
        }

        private void TimerCallback(object state)
        {
            Message = string.Format(Resx.ShutdownDialogMessage, _innerText, _counter.Seconds);
            _counter = _counter.Subtract(new TimeSpan(0, 0, 1));
            if (_counter.Seconds == 0)
            {
                UIThread.Run(() => CloseCommand.Execute(Mode));
            }
        }

        private static void ShutdownComputer()
        {
            var mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            var mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system. Use "2" to reboot.
            mboShutdownParams["Flags"] = "1";
            mboShutdownParams["Reserved"] = "0";
            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
            }
        }

    }
}