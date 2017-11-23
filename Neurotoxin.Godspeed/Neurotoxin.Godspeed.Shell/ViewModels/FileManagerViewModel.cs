using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Neurotoxin.Godspeed.Core;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{

    public class FileManagerViewModel : CommonViewModelBase, IFileManagerViewModel
    {
        private readonly ITransferManagerViewModel _transferManager;
        private readonly IUserSettingsProvider _userSettings;

        #region Main window properties

        private readonly Stack<IPaneViewModel> _leftPaneStack = new Stack<IPaneViewModel>();

        private const string LEFTPANE = "LeftPane";
        private IPaneViewModel _leftPane;
        public IPaneViewModel LeftPane
        {
            get { return _leftPane; }
            set { _leftPane = value; NotifyPropertyChanged(LEFTPANE); }
        }

        private readonly Stack<IPaneViewModel> _rightPaneStack = new Stack<IPaneViewModel>();

        private const string RIGHTPANE = "RightPane";
        private IPaneViewModel _rightPane;
        public IPaneViewModel RightPane
        {
            get { return _rightPane; }
            set
            {
                _rightPane = value; 
                NotifyPropertyChanged(RIGHTPANE);
                NotifyPropertyChanged(FTPPANE);
            }
        }

        private const string ACTIVEPANE = "ActivePane";
        public IPaneViewModel ActivePane
        {
            get 
            { 
                if (LeftPane != null && LeftPane.IsActive) return LeftPane;
                if (RightPane != null && RightPane.IsActive) return RightPane;
                return null;
            }
        }

        private const string OTHERPANE = "OtherPane";
        public IPaneViewModel OtherPane
        {
            get { return LeftPane.IsActive ? RightPane : LeftPane; }
        }

        private const string SOURCEPANE = "SourcePane";
        public IFileListPaneViewModel SourcePane
        {
            get
            {
                var left = LeftPane as IFileListPaneViewModel;
                var right = RightPane as IFileListPaneViewModel;
                if (left != null && left.IsActive) return left;
                if (right != null && right.IsActive) return right;
                return null;
            }
        }

        private const string TARGETPANE = "TargetPane";
        public IFileListPaneViewModel TargetPane
        {
            get
            {
                var left = LeftPane as IFileListPaneViewModel;
                var right = RightPane as IFileListPaneViewModel;
                if (left != null && !left.IsActive) return left;
                if (right != null && !right.IsActive) return right;
                return null;
            }
        }

        private const string FTPPANE = "FtpPane";
        public FtpContentViewModel FtpPane
        {
            get
            {
                return RightPane as FtpContentViewModel;
            }
        }

        private const string UNREADMESSAGECOUNT = "UnreadMessageCount";
        public int UnreadMessageCount
        {
            get { return UserMessages.Count(m => !m.IsRead); }
        }

        public ObservableCollection<IUserMessageViewModel> UserMessages { get; private set; }

        #endregion

        #region SwitchPaneCommand

        public DelegateCommand<EventInformation<KeyEventArgs>> SwitchPaneCommand { get; private set; }

        private bool CanExecuteSwitchPaneCommand(EventInformation<KeyEventArgs> eventInformation)
        {
            return eventInformation.EventArgs.Key == Key.Tab;
        }

        private void ExecuteSwitchPaneCommand(EventInformation<KeyEventArgs> eventInformation)
        {
            OtherPane.SetActive();
            eventInformation.EventArgs.Handled = true;
        }

        #endregion

        #region EditCommand

        public DelegateCommand EditCommand { get; private set; }

        private bool CanExecuteEditCommand()
        {
            var pane = ActivePane as ConnectionsViewModel;
            return pane != null && pane.SelectedItem is FtpConnectionItemViewModel && !pane.IsBusy;
        }
        
        private void ExecuteEditCommand()
        {
            ((ConnectionsViewModel)ActivePane).Edit();
        }

        #endregion

        #region CopyCommand

        public DelegateCommand<IEnumerable<FileSystemItem>> CopyCommand { get; private set; }

        private bool CanExecuteCopyCommand(IEnumerable<FileSystemItem> queue)
        {
            return CanExecuteCopyCommand(SourcePane, TargetPane);
        }

        private bool CanExecuteCopyCommand(IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane)
        {
            return targetPane != null && sourcePane != null && sourcePane.HasValidSelection && !sourcePane.IsBusy &&
                   !targetPane.IsBusy && !targetPane.IsReadOnly && !sourcePane.IsInEditMode;
        }

        private void ExecuteCopyCommand(IEnumerable<FileSystemItem> queue)
        {
            if (!ConfirmCommand(FileOperation.Copy, SourcePane.SelectedItems.Count() > 1)) return;
            OnExecuteFileOperation(FileOperation.Copy, SourcePane, TargetPane, queue);
        }

        #endregion

        #region MoveCommand

        public DelegateCommand<IEnumerable<FileSystemItem>> MoveCommand { get; private set; }

        private bool CanExecuteMoveCommand(IEnumerable<FileSystemItem> queue)
        {
            return CanExecuteMoveCommand(SourcePane, TargetPane);
        }

        private bool CanExecuteMoveCommand(IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane)
        {
            return targetPane != null && sourcePane != null && sourcePane.HasValidSelection && !sourcePane.IsBusy &&
                   !targetPane.IsBusy && !targetPane.IsReadOnly && !sourcePane.IsReadOnly && !sourcePane.IsInEditMode;
        }

        private void ExecuteMoveCommand(IEnumerable<FileSystemItem> queue)
        {
            if (!ConfirmCommand(FileOperation.Move, SourcePane.SelectedItems.Count() > 1)) return;
            OnExecuteFileOperation(FileOperation.Move, SourcePane, TargetPane, queue);
        }

        #endregion

        #region NewFolderCommand

        public DelegateCommand NewFolderCommand { get; private set; }

        private bool CanExecuteNewFolderCommand()
        {
            return SourcePane != null && !SourcePane.IsBusy && !SourcePane.IsReadOnly && !SourcePane.IsInEditMode;
        }

        private void ExecuteNewFolderCommand()
        {
            var items = SourcePane.Items.Select(item => item.Name).ToList();
            var wkDirs = DirectoryStructure.WellKnownDirectoriesOf(SourcePane.CurrentFolder.Path);
            var suggestion = wkDirs.Where(d => !items.Contains(d)).Select(d => new InputDialogOptionViewModel
                {
                    Value = d,
                    DisplayName = TitleRecognizer.GetTitle(d)
                }).ToList();

            var name = WindowManager.ShowTextInputDialog(Resx.AddNewFolder, Resx.FolderName + Strings.Colon, string.Empty, suggestion);
            if (name == null) return;
            name = name.Trim();
            if (name == string.Empty)
            {
                WindowManager.ShowMessage(Resx.AddNewFolder, Resx.CannotCreateFolderWithNoName);
                return;
            }
            //var invalidChars = Path.GetInvalidFileNameChars();
            //if (invalidChars.Any(name.Contains))
            //{
            //    WindowManager.ShowMessage(Resx.AddNewFolder, Resx.CannotCreateFolderWithInvalidCharacters);
            //    return;
            //}
            var path = string.Format("{0}{1}", SourcePane.CurrentFolder.Path, name);
            WorkHandler.Run(() => SourcePane.CreateFolder(path), result => NewFolderSuccess(result, name), NewFolderError);
        }

        private void NewFolderSuccess(TransferResult result, string name)
        {
            if (result != TransferResult.Ok)
            {
                WindowManager.ShowMessage(Resx.AddNewFolder, string.Format(Resx.FolderAlreadyExists, name));
                return;
            }
            SourcePane.Refresh(false, () =>
            {
                var newFolder = SourcePane.Items.SingleOrDefault(item => item.Name == name);
                if (newFolder != null) SourcePane.CurrentRow = newFolder;
            });
        }

        private void NewFolderError(Exception ex)
        {
            EventAggregator.GetEvent<ShowCorrespondingErrorEvent>().Publish(new ShowCorrespondingErrorEventArgs(ex, false));
        }

        #endregion

        #region DeleteCommand

        public DelegateCommand<IEnumerable<FileSystemItem>> DeleteCommand { get; private set; }

        private bool CanExecuteDeleteCommand(IEnumerable<FileSystemItem> queue)
        {
            return CanExecuteDeleteCommand(SourcePane);
        }

        private bool CanExecuteDeleteCommand(IPaneViewModel activePane)
        {
            var connections = activePane as ConnectionsViewModel;
            if (connections != null)
            {
                var validItem = connections.SelectedItem as FtpConnectionItemViewModel;
                return validItem != null && !connections.IsBusy;
            }
            var sourcePane = activePane as IFileListPaneViewModel;
            return sourcePane != null && 
                   sourcePane.HasValidSelection && 
                   !sourcePane.IsBusy && 
                   !sourcePane.IsReadOnly &&
                   !sourcePane.IsInEditMode && 
                   !sourcePane.IsInPathEditMode;
        }

        private void ExecuteDeleteCommand(IEnumerable<FileSystemItem> queue)
        {
            var connections = ActivePane as ConnectionsViewModel;
            if (!ConfirmCommand(FileOperation.Delete, connections == null && SourcePane.SelectedItems.Count() > 1)) return;
            if (connections != null)
            {
                connections.Delete();
            }
            else
            {
                OnExecuteFileOperation(FileOperation.Delete, SourcePane, null, queue);
            }
        }

        #endregion

        #region OpenUserMessageCommand

        public DelegateCommand<UserMessageCommandParameter> OpenUserMessageCommand { get; private set; }

        private void ExecuteOpenUserMessageCommand(UserMessageCommandParameter p)
        {
            var message = p.ViewModel;
            message.IsRead = true;
            message.IsChecked = true;
            NotifyPropertyChanged(UNREADMESSAGECOUNT);

            switch (p.Command)
            {
                case MessageCommand.OpenUrl:
                    if (Web.Browse((string)p.Parameter))
                    {
                        if (message.Flags.HasFlag(MessageFlags.IgnoreAfterOpen)) _userSettings.IgnoreMessage(message.Message);
                    }
                    break;
                case MessageCommand.OpenDialog:
                    var dialogType = (Type) p.Parameter;
                    var stat = Container.Resolve<IStatisticsViewModel>();
                    var d = FastActivator<IWorkHandler, IWindowManager, IStatisticsViewModel>.CreateInstance(dialogType, WorkHandler, WindowManager, stat) as Window;
                    if (d.ShowDialog() == true) _userSettings.IgnoreMessage(message.Message);
                    break;
            }
        }

        #endregion

        #region RemoveUserMessageCommand

        public DelegateCommand<UserMessageViewModel> RemoveUserMessageCommand { get; private set; }

        private void ExecuteRemoveUserMessageCommand(UserMessageViewModel message)
        {
            if (message.Flags.HasFlag(MessageFlags.Ignorable) && WindowManager.Confirm(Resx.RemoveUserMessage, Resx.RemoveUserMessageConfirmation))
            {
                _userSettings.IgnoreMessage(message.Message);
            }
            UserMessages.Remove(message);
            if (UserMessages.Count == 0) UserMessages.Add(new NoMessagesViewModel());
        }

        #endregion

        public FileManagerViewModel(ITransferManagerViewModel transferManager, IUserSettingsProvider userSettings)
        {
            _transferManager = transferManager;
            _userSettings = userSettings;
            UserMessages = new ObservableCollection<IUserMessageViewModel> { new NoMessagesViewModel() };
            UserMessages.CollectionChanged += (sender, args) => NotifyPropertyChanged(UNREADMESSAGECOUNT);

            SwitchPaneCommand = new DelegateCommand<EventInformation<KeyEventArgs>>(ExecuteSwitchPaneCommand, CanExecuteSwitchPaneCommand);
            EditCommand = new DelegateCommand(ExecuteEditCommand, CanExecuteEditCommand);
            CopyCommand = new DelegateCommand<IEnumerable<FileSystemItem>>(ExecuteCopyCommand, CanExecuteCopyCommand);
            MoveCommand = new DelegateCommand<IEnumerable<FileSystemItem>>(ExecuteMoveCommand, CanExecuteMoveCommand);
            NewFolderCommand = new DelegateCommand(ExecuteNewFolderCommand, CanExecuteNewFolderCommand);
            DeleteCommand = new DelegateCommand<IEnumerable<FileSystemItem>>(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            OpenUserMessageCommand = new DelegateCommand<UserMessageCommandParameter>(ExecuteOpenUserMessageCommand);
            RemoveUserMessageCommand = new DelegateCommand<UserMessageViewModel>(ExecuteRemoveUserMessageCommand);

            EventAggregator.GetEvent<OpenNestedPaneEvent>().Subscribe(OnOpenNestedPane);
            EventAggregator.GetEvent<CloseNestedPaneEvent>().Subscribe(OnCloseNestedPane);
            EventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(OnActivePaneChanged);
            EventAggregator.GetEvent<RefreshPanesEvent>().Subscribe(OnRefreshPanes);
            EventAggregator.GetEvent<RaiseCanExecuteChangesEvent>().Subscribe(OnRaiseCanExecuteChanges);
            EventAggregator.GetEvent<NotifyUserMessageEvent>().Subscribe(OnNotifyUserMessage);
            EventAggregator.GetEvent<ExecuteFileOperationEvent>().Subscribe(OnExecuteFileOperation);
            EventAggregator.GetEvent<CanExecuteFileOperationEvent>().Subscribe(OnCanExecuteFileOperation);
        }

        public void Initialize()
        {
            LeftPane = (IPaneViewModel)Container.Resolve(GetStoredPaneType(_userSettings.LeftPaneType));
            var leftParam = _userSettings.LeftPaneFileListPaneSettings;
            LeftPane.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(leftParam), PaneLoaded);
            LeftPane.SetActive();

            RightPane = (IPaneViewModel)Container.Resolve(GetStoredPaneType(_userSettings.RightPaneType));
            var rightParam = _userSettings.RightPaneFileListPaneSettings;
            RightPane.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(rightParam), PaneLoaded);
        }

        private void PaneLoaded(PaneViewModelBase pane)
        {
            if (!LeftPane.IsLoaded || !RightPane.IsLoaded) return;
            EventAggregator.GetEvent<ShellInitializedEvent>().Publish(new ShellInitializedEventArgs());
        }

        private static Type GetStoredPaneType(string typeName)
        {
            return Assembly.GetExecutingAssembly().GetType(typeName);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            EditCommand.RaiseCanExecuteChanged();
            CopyCommand.RaiseCanExecuteChanged();
            MoveCommand.RaiseCanExecuteChanged();
            NewFolderCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }

        public void SetUserMessagesToRead(IUserMessageViewModel[] items)
        {
            items.ForEach(item => item.IsRead = true);
            NotifyPropertyChanged(UNREADMESSAGECOUNT);
        }

        private void OnOpenNestedPane(OpenNestedPaneEventArgs args)
        {
            if (LeftPane == args.Opener)
            {
                _leftPaneStack.Push(LeftPane);
                LeftPane = args.Openee;
                LeftPane.SetActive();
            }
            else if (RightPane == args.Opener)
            {
                _rightPaneStack.Push(RightPane);
                RightPane = args.Openee;
                RightPane.SetActive();
            }
        }

        private void OnCloseNestedPane(CloseNestedPaneEventArgs args)
        {
            if (LeftPane == args.Pane)
            {
                var settings = LeftPane.Settings;
                LeftPane.Dispose();
                LeftPane = _leftPaneStack.Pop();
                LeftPane.LoadDataAsync(LoadCommand.Restore, new LoadDataAsyncParameters(settings, args.Payload));
                LeftPane.SetActive();
            }
            else if (RightPane == args.Pane)
            {
                var settings = RightPane.Settings;
                RightPane.Dispose();
                RightPane = _rightPaneStack.Pop();
                RightPane.LoadDataAsync(LoadCommand.Restore, new LoadDataAsyncParameters(settings, args.Payload));
                RightPane.SetActive();
            }
        }

        private void OnActivePaneChanged(ActivePaneChangedEventArgs e)
        {
            NotifyPropertyChanged(ACTIVEPANE);
        }

        private void OnRefreshPanes(RefreshPanesEventArgs e)
        {
            LeftPane.Refresh(false);
            RightPane.Refresh(false);
        }

        private void OnRaiseCanExecuteChanges(RaiseCanExecuteChangesEventArgs e)
        {
            if (e.Sender is IPaneViewModel) RaiseCanExecuteChanges();
        }

        private void OnNotifyUserMessage(NotifyUserMessageEventArgs e)
        {
            UIThread.Run(() => NotifyUserMessage(e));
        }

        private void OnExecuteFileOperation(ExecuteFileOperationEventArgs e)
        {
            //ensure that e.SourcePane == SourcePane
            OnExecuteFileOperation(e.Action, e.SourcePane ?? SourcePane, e.TargetPane ?? TargetPane, e.Items);
        }

        private void OnExecuteFileOperation(FileOperation action, IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane, IEnumerable<FileSystemItem> items)
        {
            switch (action)
            {
                case FileOperation.Copy:
                    _transferManager.Copy(sourcePane, targetPane, items);
                    break;
                case FileOperation.Move:
                    _transferManager.Move(sourcePane, targetPane, items);
                    break;
                case FileOperation.Delete:
                    _transferManager.Delete(sourcePane, items);
                    break;
            }
        }

        private void OnCanExecuteFileOperation(CanExecuteFileOperationEventArgs e)
        {
            var source = e.Sender as IFileListPaneViewModel;
            IFileListPaneViewModel target = null;
            if (e.Sender == LeftPane) target = RightPane as IFileListPaneViewModel;
            if (e.Sender == RightPane) target = LeftPane as IFileListPaneViewModel;

            switch (e.Action)
            {
                case FileOperation.Copy:
                    if (!CanExecuteCopyCommand(source, target)) e.Cancelled = true;
                    break;
                case FileOperation.Move:
                    if (!CanExecuteMoveCommand(source, target)) e.Cancelled = true;
                    break;
                case FileOperation.Delete:
                    if (!CanExecuteDeleteCommand(source)) e.Cancelled = true;
                    break;
            }
        }

        private void NotifyUserMessage(NotifyUserMessageEventArgs e)
        {
            if (_userSettings.IsMessageIgnored(e.MessageKey)) return;
            var message = string.Format(Resx.ResourceManager.GetString(e.MessageKey), e.MessageArgs);
            var i = UserMessages.IndexOf(m => m.Message == message);
            if (i == -1)
            {
                if (UserMessages.First() is NoMessagesViewModel) UserMessages.RemoveAt(0);
                UserMessages.Insert(0, new UserMessageViewModel(message, e));
                if (_userSettings.DisableNotificationSound) return;
                var notificationSound = new SoundPlayer(new MemoryStream(ResourceManager.GetContentByteArray("/Resources/Sounds/notification.wav")));
                notificationSound.Play();
            }
            else if (i != 0)
            {
                UserMessages.Move(i, 0);
            }
        }

        private bool ConfirmCommand(FileOperation type, bool isPlural)
        {
            var title = Resx.ResourceManager.GetString(type.ToString());
            var message = Resx.ResourceManager.GetString(string.Format(isPlural ? "{0}ConfirmationPlural" : "{0}ConfirmationSingular", type));
            return WindowManager.Confirm(title, message);
        }

        public override void Dispose()
        {
            object data = null;
            FileListPaneSettings settings;
            IPaneViewModel left;
            do
            {
                settings = LeftPane.Settings;
                data = LeftPane.Close(data);
                left = LeftPane;
                LeftPane = _leftPaneStack.Count > 0 ? _leftPaneStack.Pop() : null;
                if (LeftPane != null) LeftPane.LoadDataAsync(LoadCommand.Restore, new LoadDataAsyncParameters(settings));
            } 
            while (LeftPane != null);  
            _userSettings.LeftPaneType = left.GetType().FullName;
            _userSettings.LeftPaneFileListPaneSettings = left.Settings;

            data = null;
            IPaneViewModel right;
            do
            {
                settings = RightPane.Settings;
                data = RightPane.Close(data);
                right = RightPane;
                RightPane = _rightPaneStack.Count > 0 ? _rightPaneStack.Pop() : null;
                if (RightPane != null) RightPane.LoadDataAsync(LoadCommand.Restore, new LoadDataAsyncParameters(settings));
            }
            while (RightPane != null);
            _userSettings.RightPaneType = right.GetType().FullName;
            _userSettings.RightPaneFileListPaneSettings = right.Settings;

            base.Dispose();
        }
    }
}