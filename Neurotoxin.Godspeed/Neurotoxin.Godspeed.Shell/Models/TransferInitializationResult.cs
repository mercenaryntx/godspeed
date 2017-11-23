using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Exceptions;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class TransferInitializationResult
    {
        public RemoteCopyMode RemoteCopyMode { get; set; }
        public TelnetException TelnetException { get; set; }
        public long? TargetFreeSpace { get; set; }

        public TransferInitializationResult()
        {
        }

        public TransferInitializationResult(RemoteCopyMode remoteCopyMode)
        {
            RemoteCopyMode = remoteCopyMode;
        }
    }
}