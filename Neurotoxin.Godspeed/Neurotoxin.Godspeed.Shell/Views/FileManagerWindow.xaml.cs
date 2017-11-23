using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Io;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Commands;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FileManagerWindow : IView<FileManagerViewModel>
    {
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWorkHandler _workHandler;
        private Queue<Timer> _userMessageReadTimers;

        public FileManagerViewModel ViewModel
        {
            get { return (FileManagerViewModel) DataContext; }
        }

        public FileManagerWindow(IUnityContainer container, IEventAggregator eventAggregator, IWorkHandler workHandler)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _workHandler = workHandler;
            Title = String.Format("GODspeed v{0}", App.ApplicationVersion);

            InitializeComponent();
            CommandBindings.Add(new CommandBinding(FileManagerCommands.OpenDriveDropdownCommand, ExecuteOpenDriveDropdownCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.SettingsCommand, ExecuteSettingsCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.StatisticsCommand, ExecuteStatisticsCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.AboutCommand, ExecuteAboutCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.VisitWebsiteCommand, ExecuteVisitWebsiteCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.UserStatisticsParticipationCommand, ExecuteUserStatisticsParticipationCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.ExitCommand, ExecuteExitCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.SplitterCommand, ExecuteSplitterCommand));

            LayoutRoot.PreviewKeyDown += LayoutRootOnPreviewKeyDown;
            Closing += OnClosing;
        }

        protected override void Initialize()
        {
            //NOTE: intentionally blank to prevent IUserSettingsProvider resolvation
        }

        public void Initialize(FileManagerViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.Initialize();
            base.Initialize();
        }

        private void OnClosing(object sender, CancelEventArgs args)
        {
            ViewModel.Dispose();
        }

        //TODO: Temporary solution. KeyBinding doesn't work with Key.Delete (requires investigation)
        private void LayoutRootOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            var deleteCommand = ViewModel.DeleteCommand;
            if (!deleteCommand.CanExecute(null)) return;
            e.Handled = true;
            deleteCommand.Execute(null);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source == null) return;
            source.AddHook(HwndHandler);
            UsbNotification.RegisterUsbDeviceNotification(source.Handle);
        }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == UsbNotification.WmDevicechange)
            {
                var e = (UsbDeviceChange) wparam;
                if (Enum.IsDefined(typeof(UsbDeviceChange), e)) 
                    _eventAggregator.GetEvent<UsbDeviceChangedEvent>().Publish(new UsbDeviceChangedEventArgs(e));
            }
            return IntPtr.Zero;
        }

        private void ExecuteOpenDriveDropdownCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var control = e.Parameter as ContentControl;
            if (control == null) return;
            var pane = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(control, 0), 0) as FileListPane;
            if (pane == null) return;
            var combobox = pane.FindName("DriveDropdown") as ComboBox;
            if (combobox == null) return;

            combobox.IsDropDownOpen = true;
            var item = combobox.ItemContainerGenerator.ContainerFromItem(combobox.SelectedItem) as ComboBoxItem;
            if (item != null) item.Focus();
        }

        private void ExecuteSettingsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var settings = UnityInstance.Container.Resolve<SettingsWindow>();
            settings.ShowDialog();
        }

        private void ExecuteStatisticsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var statistics = UnityInstance.Container.Resolve<StatisticsWindow>();
            statistics.ShowDialog();
        }

        private void ExecuteAboutCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var about = _container.Resolve<AboutDialog>();
            about.ShowDialog();
        }

        private void ExecuteVisitWebsiteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Web.Browse(e.Parameter.ToString());
        }

        private void ExecuteUserStatisticsParticipationCommand(object sender, ExecutedRoutedEventArgs e)
        {
            new UserStatisticsParticipationDialog(_workHandler, null, null).ShowDialog();
        }

        private void ExecuteExitCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExecuteSplitterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var eventInformation = e.Parameter as EventInformation<MouseEventArgs>;
            var l = Convert.ToDouble(eventInformation != null ? eventInformation.CommandArgument : e.Parameter);
            PanesGrid.ColumnDefinitions[0].Width = new GridLength(l, GridUnitType.Star);
            PanesGrid.ColumnDefinitions[2].Width = new GridLength(100 - l, GridUnitType.Star);
        }

        private void OnUserMessagesOpened(object sender, RoutedEventArgs e)
        {
            _userMessageReadTimers = new Queue<Timer>();
            EnqueueNewTimer();
            ((MenuItem)sender).ItemContainerGenerator.ItemsChanged += OnUserMessagesItemsChanged;
        }

        private void EnqueueNewTimer()
        {
            var items = ViewModel.UserMessages.Where(m => !m.IsRead).ToArray();
            if (items.Length == 0) return;
            var timer = new Timer(CheckUserMessages, items, 3000, -1);
            _userMessageReadTimers.Enqueue(timer);
        }

        private void OnUserMessagesClosed(object sender, RoutedEventArgs e)
        {
            ((MenuItem)sender).ItemContainerGenerator.ItemsChanged -= OnUserMessagesItemsChanged;
            lock (_userMessageReadTimers)
            {
                while (_userMessageReadTimers.Count > 0)
                {
                    var timer = _userMessageReadTimers.Dequeue();
                    timer.Dispose();
                }
            }
        }

        private void OnUserMessagesItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            EnqueueNewTimer();
        }

        private void CheckUserMessages(object state)
        {
            UIThread.BeginRun(() =>
                                  {
                                      ViewModel.SetUserMessagesToRead((IUserMessageViewModel[]) state);
                                      if (_userMessageReadTimers != null && _userMessageReadTimers.Count > 0) 
                                          _userMessageReadTimers.Dequeue();
                                  });
        }

    }
}