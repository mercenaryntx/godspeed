using System;
using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Helpers;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class CloseFtpTraceWindowEvent : CompositePresentationEvent<CloseFtpTraceWindowEventArgs> { }

    public class CloseFtpTraceWindowEventArgs
    {
        public FtpTraceListener TraceListener { get; private set; }
        public bool IsClosing { get; private set; }

        public CloseFtpTraceWindowEventArgs(FtpTraceListener traceListener, bool isClosing)
        {
            TraceListener = traceListener;
            IsClosing = isClosing;
        }
    }
}