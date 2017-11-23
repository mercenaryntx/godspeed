using System;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Gpd.Entries
{
    public class AchievementEntry : EntryBase
    {
        [BinaryData]
        public virtual uint Size { get; set; }

        [BinaryData]
        public virtual int AchievementId { get; set; }

        [BinaryData]
        public virtual uint ImageId { get; set; }

        [BinaryData]
        public virtual int Gamerscore { get; set; }

        [BinaryData]
        public virtual AchievementLockFlags Flags { get; set; }

        [BinaryData]
        public virtual DateTime UnlockTime { get; set; }

        [BinaryData(StringReadOptions.NullTerminated)]
        public virtual string Name { get; set; }

        [BinaryData(StringReadOptions.NullTerminated)]
        public virtual string UnlockedDescription { get; set; }

        [BinaryData(StringReadOptions.NullTerminated)]
        public virtual string LockedDescription { get; set; }

        public bool IsUnlocked
        {
            get { return Flags.HasFlag(AchievementLockFlags.Unlocked) || Flags.HasFlag(AchievementLockFlags.UnlockedOnline); }
        }

        public bool IsSecret
        {
            get { return !Flags.HasFlag(AchievementLockFlags.Visible); }
        }

        public AchievementEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}