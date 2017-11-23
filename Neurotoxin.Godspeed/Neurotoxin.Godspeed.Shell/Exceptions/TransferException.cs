using System;
using System.Diagnostics;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Exceptions
{
    public class TransferException : Exception
    {
        public TransferErrorType Type { get; private set; }
        public string SourceFile { get; set; }
        public string TargetFile { get; set; }
        public long TargetFileSize { get; set; }
        public IFileListPaneViewModel Pane { get; set; }

        public TransferException(TransferErrorType type, string message, Exception innerException = null) 
            : base(message, innerException)
        {
            Type = type;
        }

    }
}
