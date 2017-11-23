using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Reporting;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Application = System.Windows.Application;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ErrorMessage
    {
        private readonly string _details;

        public ErrorMessage(Exception exception)
        {
            if (Application.Current.MainWindow.IsVisible) Owner = Application.Current.MainWindow;
            InitializeComponent();
            Message.Text = exception.Message;

            var sb = new StringBuilder();
            var ex = exception is SomethingWentWrongException ? exception.InnerException : exception;
            do
            {
                sb.AppendLine("Error: " + ex.Message);
                sb.AppendLine(ex.StackTrace);
                sb.AppendLine(String.Empty);
                ex = ex.InnerException;
            }
            while (ex != null);
            _details = sb.ToString();
            Details.Content = _details;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Ok.Focus();
            Loaded -= OnLoaded;
        }

        private void ReportButtonClick(object sender, RoutedEventArgs e)
        {
            var userSettings = UnityInstance.Container.Resolve<IUserSettingsProvider>();
            var formData = new List<IFormData>
                {
                    new ErrorReport
                    {
                        Name = "log",
                        ClientId = userSettings.ClientId.ToString(),
                        ApplicationVersion = App.ApplicationVersion,
                        FrameworkVersion = App.FrameworkVersion,
                        OperatingSystemVersion = Environment.OSVersion.VersionString,
                        Details = _details,
                        FtpLog = GetFtpLog()
                    }
                };
            var iw = 0;
            foreach (Window window in Application.Current.Windows)
            {
                formData.Add(new WindowScreenshotReport
                {
                    Name = "window" + iw,
                    Window = window
                });
                iw++;
            }

            HttpForm.Post("report.php", formData);
            OkButtonClick(sender, e);
        }

        private static string GetFtpLog()
        {
            var sb = new StringBuilder();
            var w = Application.Current.MainWindow as FileManagerWindow;
            if (w != null)
            {
                var ftp = w.ViewModel.RightPane as FtpContentViewModel;
                if (ftp == null)
                {
                    var connections = w.ViewModel.RightPane as ConnectionsViewModel;
                    if (connections != null) ftp = connections.ConnectedFtp;
                }
                if (ftp != null)
                {
                    lock (ftp.Log)
                    {
                        for (var i = ftp.Log.Count - 1; i >= 0; i--)
                        {
                            sb.Append(ftp.Log.ElementAt(i));
                        }
                    }
                    return sb.ToString();
                }
            }
            return null;
        }

    }
}