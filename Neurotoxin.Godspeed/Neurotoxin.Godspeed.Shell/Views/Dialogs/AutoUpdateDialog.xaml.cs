using System;
using System.Windows;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Common;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class AutoUpdateDialog
    {
        private readonly IWorkHandler _workHandler;
        private readonly IWindowManager _windowManager;
        private readonly IStatisticsViewModel _statistics;

        public AutoUpdateDialog(IWorkHandler workHandler, IWindowManager windowManager, IStatisticsViewModel statistics)
        {
            _workHandler = workHandler;
            _windowManager = windowManager;
            _statistics = statistics;
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var updManager = UpdateManager.Instance;
            _workHandler.Run(() =>
            {
                updManager.ReportProgress += UpdateManagerReportProgress;
                updManager.PrepareUpdates();
                return true;
            }, b =>
            {
                _statistics.SuccessfulUpdate++;
                updManager.ReportProgress -= UpdateManagerReportProgress;
                updManager.ApplyUpdates();
            }, ex =>
            {
                _statistics.UnsuccessfulUpdate++;
                updManager.ReportProgress -= UpdateManagerReportProgress;
                _windowManager.ShowErrorMessage(new Exception(Resx.UpdateErrorOccurred, ex));
                Close();
            });
        }

        private void UpdateManagerReportProgress(UpdateProgressInfo currentStatus)
        {
            UIThread.BeginRun(() =>
            {
                Message.Text = currentStatus.Message;
                Progress.Value = currentStatus.Percentage;
            });
        }
    }
}