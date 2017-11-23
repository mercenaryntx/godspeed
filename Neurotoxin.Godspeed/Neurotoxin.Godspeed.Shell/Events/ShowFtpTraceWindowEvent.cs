using System;
using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Helpers;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class ShowFtpTraceWindowEvent : CompositePresentationEvent<ShowFtpTraceWindowEventArgs> { }

    public class ShowFtpTraceWindowEventArgs
    {
        public FtpTraceListener TraceListener { get; private set; }
        public string ConnectionName { get; private set; }

        public ShowFtpTraceWindowEventArgs(FtpTraceListener traceListener, string connectionName)
        {
            TraceListener = traceListener;
            ConnectionName = connectionName;
        }
    }
}