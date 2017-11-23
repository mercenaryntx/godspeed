using System;

namespace Neurotoxin.Godspeed.Shell.Constants
{
    [Flags]
    public enum ResumeCapability
    {
        None = 0,
        Append = 1,
        Restart = 2,
        Both = 3
    }
}
