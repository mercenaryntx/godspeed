using System;

namespace Neurotoxin.Godspeed.Core.Constants
{
    [Flags]
    public enum TransferFlags
    {
        None = 0,
        DeepLinkSupported = 4,
        DisableNetworkStorage = 8,
        KinectEnabled = 0x10,
        MoveOnlyTransfer = 0x20,
        DeviceTransfer = 0x40,
        ProfileTransfer = 0x80
    }
}