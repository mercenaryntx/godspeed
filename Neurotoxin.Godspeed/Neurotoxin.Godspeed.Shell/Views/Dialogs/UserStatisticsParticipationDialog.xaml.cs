using System;
using System.Collections.Generic;
using System.Windows;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Reporting;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class UserStatisticsParticipationDialog
    {
        private readonly IWorkHandler _workHandler;

        public UserStatisticsParticipationDialog(IWorkHandler workHandler, IWindowManager windowManager, IStatisticsViewModel statisticsViewModel)
        {
            _workHandler = workHandler;
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            if (!UserSettings.DisableUserStatisticsParticipation.HasValue) return;
            if (UserSettings.DisableUserStatisticsParticipation == true)
            {
                No.IsChecked = true;
            }
            else if (UserSettings.DisableUserStatisticsParticipation == false)
            {
                Yes.IsChecked = true;
            }
        }

        protected override void OkButtonClick(object sender, RoutedEventArgs e)
        {
            var p = No.IsChecked == true;
            if (UserSettings.DisableUserStatisticsParticipation != p)
            {
                _workHandler.Run(() => HttpForm.Post("clients.php", new List<IFormData>
                {
                    new RawPostData("client_id", UserSettings.ClientId),
                    new RawPostData("date", DateTime.Now.ToUnixTimestamp()),
                    new RawPostData("participates", p ? "yes" : "no"),
                }));
            }
            UserSettings.DisableUserStatisticsParticipation = p;
            base.OkButtonClick(sender, e);
        }
    }
}