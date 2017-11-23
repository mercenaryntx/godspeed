using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public delegate void LogChangedEventHandler(object sender, LogChangedEventArgs e);

    public class LogChangedEventArgs
    {
        public string Message { get; private set; }

        public LogChangedEventArgs(string message)
        {
            Message = message;
        }
    }
}
