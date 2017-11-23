using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IFileListPaneViewModel : IPaneViewModel
    {
        FileSystemItemViewModel Drive { get; set; }
        long? FreeSpace { get; }

        ObservableCollection<FileSystemItemViewModel> Items { get; }
        IEnumerable<FileSystemItemViewModel> SelectedItems { get; }
        FileSystemItemViewModel CurrentFolder { get; }
        FileSystemItemViewModel CurrentRow { get; set; }
        ResumeCapability ResumeCapability { get; }
        bool HasValidSelection { get; }
        bool IsReadOnly { get; }
        bool IsInEditMode { get; set; }
        bool IsInPathEditMode { get; set; }
        bool IsFSD { get; }
        bool IsVerificationEnabled { get; }
        ColumnMode DisplayColumnMode { get; }
        ColumnMode EditColumnMode { get; set; }
        FileListPaneViewMode ViewMode { get; set; }
        ObservableCollection<IMenuItemViewModel> Favorites { get; }

        Queue<QueueItem> PopulateQueue(FileOperation action, IEnumerable<FileSystemItem> selection);
        TransferResult CreateFolder(string path);
        TransferResult Delete(FileSystemItem item);

        void GetItemViewModel(string itemPath);
        string GetTargetPath(string sourcePath);

        FileExistenceInfo FileExists(string path);
        Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition);
        bool CopyStream(FileSystemItem item, Stream stream, long remoteStartPosition = 0, long? byteLimit = null);

        void Refresh(bool refreshCache);
        void Refresh(bool refreshCache, Action callback);
        void Abort();

        void Recognize(FileSystemItemViewModel item);
        void Rename(ColumnMode column);
        void Rename(ColumnMode column, string newValue);

        void SwitchTitleNameColumns(DataGridColumn column);


    }
}