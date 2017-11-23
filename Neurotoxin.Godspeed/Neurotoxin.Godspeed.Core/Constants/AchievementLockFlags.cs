using System;

namespace Neurotoxin.Godspeed.Core.Constants
{
    [Flags]
    public enum AchievementLockFlags
    {
        UnlockedOnline = 0x10000,   //Indicates the achievement was achieved online.
        Unlocked = 0x20000,          //Indicates the achievement was achieved.
        Visible = 0x8
    }
}