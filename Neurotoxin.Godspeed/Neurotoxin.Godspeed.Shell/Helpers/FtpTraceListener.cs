using System;
using System.Collections.Generic;
using System.Diagnostics;
using Neurotoxin.Godspeed.Shell.Events;

namespace Neurotoxin.Godspeed.Shell.Helpers
{
    public class FtpTraceListener : TraceListener
    {
        public readonly Stack<string> Log = new Stack<string>();

        public event LogChangedEventHandler LogChanged;

        private void NotifyLogChanged(string message)
        {
            var handler = LogChanged;
            if (handler != null) handler.Invoke(this, new LogChangedEventArgs(message));
        }

        public override void Write(string message)
        {
            lock (Log)
            {
                if (Log.Count > 0 && !Log.Peek().EndsWith(Environment.NewLine)) message = Log.Pop() + message;
                Log.Push(message);
            }
            NotifyLogChanged(message);
        }

        public override void WriteLine(string message)
        {
            if (!message.EndsWith(Environment.NewLine)) message += Environment.NewLine;
            Write(message);
        }
    }
}