using System;
using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class ShowCorrespondingErrorEvent : CompositePresentationEvent<ShowCorrespondingErrorEventArgs> { }

    public class ShowCorrespondingErrorEventArgs
    {
        public Exception Exception { get; private set; }
        public bool FeedbackNeeded { get; private set; }

        public ShowCorrespondingErrorEventArgs(Exception exception, bool feedbackNeeded)
        {
            Exception = exception;
            FeedbackNeeded = feedbackNeeded;
        }
    }
}