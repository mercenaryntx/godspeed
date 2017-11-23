using System;
using System.Collections.Generic;
using System.Windows.Shell;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public interface ITransferManagerViewModel : IViewModel
    {
        IFileListPaneViewModel SourcePane { get; }
        IFileListPaneViewModel TargetPane { get; }
        string ShutdownXbox { get; }
        FileOperation UserAction { get; set; }
        string TransferAction { get; set; }
        string SourceFile { get; set; }
        int CurrentFileProgress { get; set; }
        int TotalProgress { get; }
        double TotalProgressDouble { get; }
        int FilesTransferred { get; set; }
        int FileCount { get; set; }
        long BytesTransferred { get; set; }
        long TotalBytes { get; set; }
        int Speed { get; set; }
        TimeSpan ElapsedTime { get; set; }
        TimeSpan RemainingTime { get; set; }
        TaskbarItemProgressState ProgressState { get; set; }
        bool IsPaused { get; set; }
        bool IsVerificationSupported { get; }
        bool IsShutdownSupported { get; }
        bool IsResumeSupported { get; }
        bool IsVerificationEnabled { get; set; }
        bool IsShutdownPcEnabled { get; set; }
        bool IsShutdownXboxEnabled { get; set; }
 
        DelegateCommand PauseCommand { get; }
        DelegateCommand ContinueCommand { get; }

        void Copy(IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane, IEnumerable<FileSystemItem> queue);
        void Move(IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane, IEnumerable<FileSystemItem> queue);
        void Delete(IFileListPaneViewModel sourcePane, IEnumerable<FileSystemItem> queue);
        void InitializeTransfer(Queue<QueueItem> queue, FileOperation mode);
        void AbortTransfer();
    }
}