using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Io.Gpd.Entries;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Gpd
{
    public class DashboardFile : GpdFile
    {
        public byte[] GamerPicture { get; set; }
        public byte[] AvatarImage { get; set; }

        public int OptionControllerVibration
        {
            get { return Settings.Get<int>(SettingId.OptionControllerVibration); }
            set { Settings.Set(SettingId.OptionControllerVibration, value); }
        }

        public int OptionVoiceMuted
        {
            get { return Settings.Get<int>(SettingId.OptionVoiceMuted); }
            set { Settings.Set(SettingId.OptionVoiceMuted, value); }
        }

        public int OptionVoiceThruSpeakers
        {
            get { return Settings.Get<int>(SettingId.OptionVoiceThruSpeakers); }
            set { Settings.Set(SettingId.OptionVoiceThruSpeakers, value); }
        }

        public int OptionVoiceVolume
        {
            get { return Settings.Get<int>(SettingId.OptionVoiceVolume); }
            set { Settings.Set(SettingId.OptionVoiceVolume, value); }
        }

        public int GamercardCred
        {
            get { return Settings.Get<int>(SettingId.GamercardCred); }
            set { Settings.Set(SettingId.GamercardCred, value); }
        }

        public int GamercardTitlesPlayed
        {
            get { return Settings.Get<int>(SettingId.GamercardTitlesPlayed); }
            set { Settings.Set(SettingId.GamercardTitlesPlayed, value); }
        }

        public int GamercardAchievementsEarned
        {
            get { return Settings.Get<int>(SettingId.GamercardAchievementsEarned); }
            set { Settings.Set(SettingId.GamercardAchievementsEarned, value); }
        }

        public string GamercardPictureKey
        {
            get { return Settings.Get<string>(SettingId.GamercardPictureKey); }
            set { Settings.Set(SettingId.GamercardPictureKey, value); }
        }

        public byte[] GamercardAvatarInfo1
        {
            get { return Settings.Get<byte[]>(SettingId.GamercardAvatarInfo1); }
            set { Settings.Set(SettingId.GamercardAvatarInfo1, value); }
        }

        public int MessengerAutoSignin
        {
            get { return Settings.Get<int>(SettingId.MessengerAutoSignin); }
            set { Settings.Set(SettingId.OptionVoiceMuted, value); }
        }

        protected DashboardFile(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
            //if (!HasEntry(EntryType.Setting, (int)SettingId.GamercardZone)) Settings.Set(SettingId.GamercardZone, 0);
            //if (!HasEntry(EntryType.Setting, (int)SettingId.GamercardRegion)) Settings.Set(SettingId.GamercardRegion, 2);
            //if (!HasEntry(EntryType.Setting, (int)SettingId.YearsOnLive)) Settings.Set(SettingId.YearsOnLive, 0);
            //if (!HasEntry(EntryType.Setting, (int)SettingId.GamercardMotto)) Settings.Set(SettingId.GamercardMotto, String.Empty);
            //if (!HasEntry(EntryType.Setting, (int)SettingId.GamercardUserLocation)) Settings.Set(SettingId.GamercardUserLocation, String.Empty);
            //if (!HasEntry(EntryType.Setting, (int)SettingId.GamercardUserName)) Settings.Set(SettingId.GamercardUserName, String.Empty);
            //if (!HasEntry(EntryType.Setting, (int)SettingId.GamercardUserBio)) Settings.Set(SettingId.GamercardUserBio, String.Empty);
            //if (!HasEntry(EntryType.Setting, (int)SettingId.GamercardRep)) Settings.Set(SettingId.GamercardRep, (float)0.0);
        }

        protected override void Initialize()
        {
            base.Initialize();
            const int titleInformation = (int)SettingId.TitleInformation;
            const int avatarImage = (int)SettingId.AvatarImage;
            if (HasEntry(EntryType.Image, titleInformation)) GamerPicture = Images.Get(titleInformation).ImageData;
            if (HasEntry(EntryType.Image, avatarImage)) AvatarImage = Images.Get(avatarImage).ImageData;
        }

        public override void Recalculate()
        {
            base.Recalculate();
            GamercardTitlesPlayed = TitlesPlayed.Count;
            GamercardAchievementsEarned = TitlesPlayed.Sum(g => g.AchievementsUnlocked);
            GamercardCred = TitlesPlayed.Sum(g => g.GamerscoreUnlocked);
        }

        //public void RebuildTitleSyncList()
        //{
        //    var entry = TitlesPlayed.SyncList.Entry;

        //    //create new data
        //    var length = TitlesPlayed.Count*SyncEntry.Size;
        //    var binary = new BinaryContainer(length);
        //    for (var i = 0; i < TitlesPlayed.Count; i++)
        //    {
        //        var item = ModelFactory.GetModel<SyncEntry>(binary, i*16);
        //        item.EntryId = TitlesPlayed[i].Entry.Id;
        //        item.SyncId = 0; //?
        //    }

        //    //release old space
        //    var freeSpaceIndex = FreeSpaceTableEntryCount++;
        //    var pos = freeSpaceIndex*FreeSpaceEntrySize;
        //    var freeSpaceEntry = ModelFactory.GetModel<XdbfFreeSpaceEntry>(binary, pos);
        //    freeSpaceEntry.AddressSpecifier = entry.AddressSpecifier;
        //    freeSpaceEntry.Length = entry.Length;
        //    FreeSpace.Add(freeSpaceEntry);

        //    //copy data to a new space
        //    var content = binary.ReadAll();
        //    var specifier = AllocateSpaceForContent(content);
        //    var startOffset = GetRealAddress(specifier);
        //    Binary.WriteBytes(startOffset, content, 0, content.Length);
        //    entry.AddressSpecifier = specifier;
        //    entry.Length = length;

        //    //parse copied data back
        //    var entryCount = entry.Length / 16;
        //    TitlesPlayed.SyncList = new SyncList { Entry = entry };
        //    for (var i = 0; i < entryCount; i++)
        //        TitlesPlayed.SyncList.Add(ModelFactory.GetModel<SyncEntry>(Binary, startOffset + i * 16));
            
        //}
    }
}