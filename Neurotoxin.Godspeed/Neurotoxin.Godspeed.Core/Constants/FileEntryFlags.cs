using System;

namespace Neurotoxin.Godspeed.Core.Constants
{
    [Flags]
    public enum FileEntryFlags
    {
        IsDirectory = 0x80,
        BlocksAreConsecutive = 0x40,
        Default = 0
    }
}