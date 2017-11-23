using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Io.Gpd.Entries;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Gpd
{
    public class GameFile : GpdFile
    {
        public string TitleId { get; set; }
        public string Title { get; set; }

        public int Gamerscore
        {
            get { return Settings.Get<int>(SettingId.GamercardTitleCredEarned); }
            set { Settings.Set(SettingId.GamercardTitleCredEarned, value); }
        }

        public int UnlockedAchievementCount
        {
            get { return Settings.Get<int>(SettingId.GamercardTitleAchievementsEarned); }
            set { Settings.Set(SettingId.GamercardTitleAchievementsEarned, value); }
        }

        public int AchievementCount
        {
            get { return Achievements.Count; }
        }
        public int TotalGamerscore
        {
            get { return Achievements.Sum(a => a.Gamerscore); }
        }
        public byte[] Thumbnail { get; set; }

        protected GameFile(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            var tid = (int)SettingId.TitleInformation;
            if (HasEntry(EntryType.String, tid)) Title = Strings.Get(tid).Text;
            if (HasEntry(EntryType.Image, tid)) Thumbnail = Images.Get(tid).ImageData;

            //if (Title == "Shadow Complex")
            //{
            //    foreach (var e in Entries)
            //    {
            //        var id = e.Id;
            //        var extra = string.Empty;
            //        if (e.IsSyncData)
            //        {
            //            if (e.Type != EntryType.AvatarAward) id -= 0x200000000;
            //            extra = "SyncData";
            //        }
            //        if (e.IsSyncList)
            //        {
            //            if (e.Type != EntryType.AvatarAward) id -= 0x100000000;
            //            extra = "SyncList";
            //        }
            //        Debug.WriteLine("[{0,3}][{1}][{2}]", id, e.Type, extra);
            //    }
            //}
        }

        protected override IEnumerable<XdbfEntry> CollectParseEntries()
        {
            return Entries.Where(e => e.Id != (int) SettingId.TitleInformation);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            var unlockeds = Achievements.Where(a => a.IsUnlocked).ToList();
            UnlockedAchievementCount = unlockeds.Count();
            Gamerscore = unlockeds.Sum(a => a.Gamerscore);
        }

        public void UnlockAchievement(int id, byte[] image)
        {
            var achievement = Achievements.FirstOrDefault(a => a.AchievementId == id);
            if (achievement == null)
                throw new ArgumentException("Invalid achievement id " + id);

            achievement.Flags |= AchievementLockFlags.Unlocked;
            achievement.UnlockTime = DateTime.Now;
            AddNewEntry<ImageEntry>(EntryType.Image, image, achievement.ImageId);
            Recalculate();
        }

        public bool MergeWith(GameFile other)
        {
            if (TitleId != other.TitleId)
                throw new NotSupportedException("You don't want to do that!");

            var achievementMerged = MergeEntries(other.Achievements, Achievements, EntryType.Achievement, (a, b) =>
            {
                if (a.IsUnlocked || !b.IsUnlocked) return false;
                a.UnlockTime = b.UnlockTime;
                if (b.Flags.HasFlag(AchievementLockFlags.Unlocked)) a.Flags |= AchievementLockFlags.Unlocked;
                if (b.Flags.HasFlag(AchievementLockFlags.UnlockedOnline)) a.Flags |= AchievementLockFlags.UnlockedOnline;
                return true;
            });
            if (achievementMerged) Recalculate();

            var imagesMerged = MergeEntries(other.Images, Images, EntryType.Image, null);

            var awardsMerged = MergeEntries(other.AvatarAwards, AvatarAwards, EntryType.AvatarAward, (a, b) =>
            {
                if (a.IsUnlocked || !b.IsUnlocked) return false;
                a.UnlockTime = b.UnlockTime;
                if (b.Flags.HasFlag(AchievementLockFlags.Unlocked)) a.Flags |= AchievementLockFlags.Unlocked;
                if (b.Flags.HasFlag(AchievementLockFlags.UnlockedOnline)) a.Flags |= AchievementLockFlags.UnlockedOnline;
                return true;
            });

            return achievementMerged || imagesMerged || awardsMerged;
        }

        private bool MergeEntries<T>(EntryList<T> source, EntryList<T> target, EntryType type, Func<T,T, bool> action) where T : EntryBase
        {
            var changed = false;
            foreach (var b in source)
            {
                var a = target.FirstOrDefault(e => e.Entry.Id == b.Entry.Id);
                if (a == null)
                {
                    AddNewEntry<T>(type, b.AllBytes, b.Entry.Id);
                    changed = true;
                    continue;
                }
                if (action != null) changed = action.Invoke(a,b) || changed;
            }
            return changed;
        }
    }
}