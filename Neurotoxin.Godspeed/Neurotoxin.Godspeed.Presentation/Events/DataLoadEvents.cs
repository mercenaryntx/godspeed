using System;
using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Core.Constants;

namespace Neurotoxin.Godspeed.Presentation.Events
{
    // Occurs when a data loading process has been started
    public class DataLoadingEvent : CompositePresentationEvent<DataLoadEventArgs>
    {
    }

    // Occurs when a data loading process has been finished
    public class DataLoadedEvent : CompositePresentationEvent<DataLoadEventArgs>
    {
    }

    public delegate void DataLoadEventHandler(object sender, DataLoadEventArgs e);

    public class DataLoadEventArgs : EventArgs
    {
        public object Sender { get; private set; }
        public string Section { get; private set; }
        public string Message { get; private set; }
        public DataLoadState State { get; private set; }

        public DataLoadEventArgs(object sender, DataLoadState state, string section, string message)
        {
            Sender = sender;
            State = state;
            Section = section;
            Message = message;
        }
    }

    public enum DataLoadState
    {
        Loading,
        Loaded
    }
}