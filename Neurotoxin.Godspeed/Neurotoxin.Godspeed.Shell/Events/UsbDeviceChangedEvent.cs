using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Core.Constants;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class UsbDeviceChangedEvent : CompositePresentationEvent<UsbDeviceChangedEventArgs> { }

    public class UsbDeviceChangedEventArgs
    {
        public UsbDeviceChange Change { get; private set; }

        public UsbDeviceChangedEventArgs(UsbDeviceChange change)
        {
            Change = change;
        }
    }
}