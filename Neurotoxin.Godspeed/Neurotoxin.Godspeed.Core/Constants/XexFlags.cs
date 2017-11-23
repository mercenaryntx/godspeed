using System;

namespace Neurotoxin.Godspeed.Core.Constants
{
    [Flags]
    public enum XexFlags
    {
        Unknown = 0,
        TitleModule = 1,
        ExportsToTitle = 2,
        SystemDebugger = 4,
        DllModule = 8,
        ModulePatch = 0x10,
        PatchFull = 0x20,
        PatchDelta = 0x40,
        UserCode = 0x80
    }
}