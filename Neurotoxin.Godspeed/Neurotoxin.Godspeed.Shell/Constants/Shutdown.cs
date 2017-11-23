using System;

namespace Neurotoxin.Godspeed.Shell.Constants
{
    [Flags]
    public enum Shutdown
    {
        Disabled = 0,
        PC = 1,
        Xbox = 2,
        Both = 3
    }
}