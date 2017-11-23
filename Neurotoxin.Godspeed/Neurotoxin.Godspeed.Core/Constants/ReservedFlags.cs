using System;

namespace Neurotoxin.Godspeed.Core.Constants
{
    [Flags]
    public enum ReservedFlags
    {
        Unknown = 1,
        PasscodeEnabled = 0x10000000,
        LiveEnabled = 0x20000000,
        Recovering = 0x40000000
    }
}