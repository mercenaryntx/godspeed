using System;

namespace Neurotoxin.Godspeed.Shell.Constants
{
    [Flags]
    public enum MessageFlags
    {
        None = 0x0,
        Ignorable = 0x1,
        IgnoreAfterOpen = 0x2,
    }
}