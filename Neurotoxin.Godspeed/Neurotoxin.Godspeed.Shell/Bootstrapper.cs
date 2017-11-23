using System;
using System.Globalization;
using System.Windows;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Controllers;
using Neurotoxin.Godspeed.Shell.Database;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using WPFLocalizeExtension.Engine;

namespace Neurotoxin.Godspeed.Shell
{
    public class Bootstrapper : UnityBootstrapper
    {
        private FileManagerWindow _shell;

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            UnityInstance.Container = Container;
            //Container.RegisterType<IGeneralController, ModuleController>(new ContainerControlledLifetimeManager());

            // Infrastructure
            Container.RegisterType<IWorkHandler, AsyncWorkHandler>(new ContainerControlledLifetimeManager());

            // Managers
            Container.RegisterType<IResourceManager, ResourceManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWindowManager, WindowManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<NotificationService>(new ContainerControlledLifetimeManager());

            // Content providers
            Container.RegisterType<IDbContext, OrmLiteDbContext>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IUserSettingsProvider, UserSettingsProvider>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITitleRecognizer, TitleRecognizer>();
            Container.RegisterType<FtpContent>();
            Container.RegisterType<LocalFileSystemContent>();
            Container.RegisterType<StfsPackageContent>();
            Container.RegisterType<CompressedFileContent>();
            Container.RegisterType<ICacheManager, CacheManager>(new ContainerControlledLifetimeManager());

            // ViewModels
            Container.RegisterType<FileManagerViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITransferManagerViewModel, TransferManagerViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISettingsViewModel, SettingsViewModel>();
            Container.RegisterType<IStatisticsViewModel, StatisticsViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ConnectionsViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<FtpContentViewModel>();
            Container.RegisterType<LocalFileSystemContentViewModel>();
            Container.RegisterType<StfsPackageContentViewModel>();
            Container.RegisterType<CompressedFileContentViewModel>();
            Container.RegisterType<FreestyleDatabaseCheckerViewModel>();
            Container.RegisterType<ILoginViewModel, LoginViewModel>();

            // Views
            Container.RegisterType<FileManagerWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<SettingsWindow>();
            Container.RegisterType<StatisticsWindow>();
            Container.RegisterType<FreestyleDatabaseCheckerWindow>();
            Container.RegisterType<ErrorMessage>();

            // Helpers
            Container.RegisterType<SanityCheckerViewModel>(new ContainerControlledLifetimeManager());
        }

        protected override IModuleCatalog GetModuleCatalog()
        {
            // Gets the catalog from the app.config
            var catalog = new ConfigurationModuleCatalog();
            catalog.Load();
            return catalog;
        }

        protected override DependencyObject CreateShell()
        {
            _shell = Container.Resolve<FileManagerWindow>();
            var windowManager = Container.Resolve<IWindowManager>();
            var sanityChecker = Container.Resolve<SanityCheckerViewModel>();
            sanityChecker.CheckAsync(SanityCheckCallback);
            return _shell;
        }

        private void SanityCheckCallback(SanityCheckResult result)
        {
            var notificationService = Container.Resolve<NotificationService>();
            if (result.UserMessages != null) notificationService.QueueMessages(result.UserMessages);
            InitializeShell();
        }

        private void InitializeShell()
        {
            App.ShellInitialized = true;
            var viewModel = Container.Resolve<FileManagerViewModel>();
            var userSettings = Container.Resolve<IUserSettingsProvider>();
            LocalizeDictionary.Instance.Culture = userSettings.Language ?? CultureInfo.CurrentCulture;
            _shell.Initialize(viewModel);
            Application.Current.Dispatcher.BeginInvoke(new Action(_shell.Show));
        }

        public T Resolve<T>(params ResolverOverride[] overrides)
        {
            return Container.Resolve<T>(overrides);
        }
    }
}