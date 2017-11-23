using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IFileManagerViewModel
    {
        IPaneViewModel LeftPane { get; set; }
        IPaneViewModel RightPane { get; set; }
        IPaneViewModel ActivePane { get; }
        IPaneViewModel OtherPane { get; }
        IFileListPaneViewModel SourcePane { get; }
        IFileListPaneViewModel TargetPane { get; }

        int UnreadMessageCount { get; }
        ObservableCollection<IUserMessageViewModel> UserMessages { get; }

        DelegateCommand<EventInformation<KeyEventArgs>> SwitchPaneCommand { get; }
        DelegateCommand EditCommand { get; }
        DelegateCommand<IEnumerable<FileSystemItem>> CopyCommand { get; }
        DelegateCommand<IEnumerable<FileSystemItem>> MoveCommand { get; }
        DelegateCommand NewFolderCommand { get; }
        DelegateCommand<IEnumerable<FileSystemItem>> DeleteCommand { get; }
        DelegateCommand<UserMessageCommandParameter> OpenUserMessageCommand { get; }
        DelegateCommand<UserMessageViewModel> RemoveUserMessageCommand { get; }

        void Initialize();
        void SetUserMessagesToRead(IUserMessageViewModel[] items);
        void RaiseCanExecuteChanges();
    }
}