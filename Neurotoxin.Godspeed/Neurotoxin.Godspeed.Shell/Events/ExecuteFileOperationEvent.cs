using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class ExecuteFileOperationEvent : CompositePresentationEvent<ExecuteFileOperationEventArgs> { }

    public class ExecuteFileOperationEventArgs
    {
        public FileOperation Action { get; private set; }
        public IFileListPaneViewModel SourcePane { get; private set; }
        public IFileListPaneViewModel TargetPane { get; private set; }
        public IEnumerable<FileSystemItem> Items { get; private set; }

        public ExecuteFileOperationEventArgs(FileOperation action, IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane, IEnumerable<FileSystemItem> items)
        {
            Action = action;
            SourcePane = sourcePane;
            TargetPane = targetPane;
            Items = items;
        }
    }
}