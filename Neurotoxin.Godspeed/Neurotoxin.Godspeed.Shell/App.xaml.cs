using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Reporting;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell
{

    public partial class App : Application
    {
        private Bootstrapper _bootstrapper;

        private static string _applicationVersion;
        public static string ApplicationVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_applicationVersion))
                {
                    var assembly = Assembly.GetAssembly(typeof(App));
                    var assemblyName = assembly.GetName();
                    _applicationVersion = assemblyName.Version.ToString();
                }
                return _applicationVersion;
            }
        }

        private static string _frameworkVersion;

        public static string FrameworkVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_frameworkVersion))
                {
                    var applicationAssembly = Assembly.GetAssembly(typeof(Application));
                    var fvi = FileVersionInfo.GetVersionInfo(applicationAssembly.Location);
                    _frameworkVersion = fvi.ProductVersion;
                }
                return _frameworkVersion;
            }
        }

        public static string DataDirectory { get; set; }
        public static string PostDirectory { get; set; }
        public static bool ShellInitialized { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Dispatcher.CurrentDispatcher.UnhandledException += UnhandledThreadingException;
            ShutdownMode = ShutdownMode.OnMainWindowClose;

#if (DEBUG)
            RunInDebugMode();
#else
            RunInReleaseMode();
#endif
        }

        private void RunInDebugMode()
        {
            _bootstrapper = new Bootstrapper();
            _bootstrapper.Run();
        }

        private void RunInReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            try
            {
                RunInDebugMode();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private void UnhandledThreadingException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            HandleException(e.Exception);
        }

        private void HandleException(Exception ex)
        {
            ex = ex is TargetInvocationException ? ex.InnerException : ex;
            var dialog = _bootstrapper.Resolve<ErrorMessage>(new ParameterOverride("exception", ex));
            dialog.ShowDialog();
            Shutdown(ex.GetType().Name.GetHashCode());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_bootstrapper != null)
            {
                var fileManager = _bootstrapper.Resolve<FileManagerViewModel>();
                if (!fileManager.IsDisposed) fileManager.Dispose();
                var statistics = _bootstrapper.Resolve<StatisticsViewModel>();
                if (e.ApplicationExitCode != 0) statistics.ApplicationCrashed++;
                statistics.PersistData();

                var userSettings = _bootstrapper.Container.Resolve<IUserSettingsProvider>();
                userSettings.PersistData();

                //TODO: Better detection
                if (!Debugger.IsAttached && userSettings.DisableUserStatisticsParticipation != true)
                {
                    var commandUsage = new StringBuilder();
                    foreach (var kvp in statistics.CommandUsage)
                    {
                        commandUsage.AppendLine(string.Format("{0}={1}", kvp.Key, kvp.Value));
                    }

                    var serverUsage = new StringBuilder();
                    foreach (var kvp in statistics.ServerUsage)
                    {
                        serverUsage.AppendLine(string.Format("{0}={1}", kvp.Key, kvp.Value));
                    }

                    var utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(statistics.UsageStart);
                    HttpForm.Post("stats.php", new List<IFormData>
                        {
                            new RawPostData("client_id", userSettings.ClientId),
                            new RawPostData("version", ApplicationVersion),
                            new RawPostData("wpf", FrameworkVersion),
                            new RawPostData("os", Environment.OSVersion.VersionString),
                            new RawPostData("culture", CultureInfo.CurrentCulture.Name),
                            new RawPostData("uiculture", CultureInfo.CurrentUICulture.Name),
                            new RawPostData("osculture", CultureInfo.InstalledUICulture.Name),
                            new RawPostData("date", statistics.UsageStart.ToUnixTimestamp()),
                            new RawPostData("timezone", string.Format("{0}{1:D2}:{2:D2}", utcOffset.Hours >= 0 ? "+" : string.Empty, utcOffset.Hours, utcOffset.Minutes)),
                            new RawPostData("usage", Math.Floor(statistics.UsageTime.TotalSeconds)),
                            new RawPostData("exit_code", e.ApplicationExitCode),
                            new RawPostData("games_recognized", statistics.GamesRecognizedFully),
                            new RawPostData("partially_recognized", statistics.GamesRecognizedPartially),
                            new RawPostData("svod_recognized", statistics.SvodPackagesRecognized),
                            new RawPostData("stfs_recognized", statistics.StfsPackagesRecognized),
                            new RawPostData("transferred_bytes", statistics.BytesTransferred),
                            new RawPostData("transferred_files", statistics.FilesTransferred),
                            new RawPostData("transfer_time", Math.Floor(statistics.TimeSpentWithTransfer.TotalSeconds)),
                            new RawPostData("command_usage", commandUsage),
                            new RawPostData("server_usage", serverUsage),
                            new RawPostData("successful_update", statistics.SuccessfulUpdate),
                            new RawPostData("unsuccessful_update", statistics.UnsuccessfulUpdate),
                        });
                }
            }
            base.OnExit(e);
        }

    }
}