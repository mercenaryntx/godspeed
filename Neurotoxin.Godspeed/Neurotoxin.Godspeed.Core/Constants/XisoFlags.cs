using System;

namespace Neurotoxin.Godspeed.Core.Constants
{
    [Flags]
    public enum XisoFlags
    {
        Unknown = 0,
        ReadOnly = 1,
        Hidden = 2,
        System = 4,
        Directory = 0x10,
        Archive = 0x20,
        Device = 0x40,
        Normal = 0x80
    }
}