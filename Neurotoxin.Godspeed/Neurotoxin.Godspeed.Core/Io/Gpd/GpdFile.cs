using System;
using System.Collections.Generic;
using System.Diagnostics;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Io.Gpd.Entries;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;
using System.Linq;

namespace Neurotoxin.Godspeed.Core.Io.Gpd
{
    public abstract class GpdFile : BinaryModelBase
    {
        protected const int HeaderSize = 0x18;
        protected const int EntrySize = 0x12;
        protected const int FreeSpaceEntrySize = 0x8;

        private readonly int _entryTableSize;
        private readonly int _freeTableSize;
        private readonly int _tableSize;

        [BinaryData(4, "ascii")]
        public virtual string Magic { get; set; }

        [BinaryData]
        public virtual int Version { get; set; }

        [BinaryData]
        public virtual int EntryTableLength { get; private set; }

        [BinaryData]
        public virtual int EntryCount { get; set; }

        [BinaryData]
        public virtual int FreeSpaceTableLength { get; private set; }

        [BinaryData]
        public virtual int FreeSpaceTableEntryCount { get; set; }

        public EntryList<AchievementEntry> Achievements { get; private set; }
        public EntryList<ImageEntry> Images { get; private set; }
        public SettingList Settings { get; private set; }
        public EntryList<StringEntry> Strings { get; private set; }
        public EntryList<TitleEntry> TitlesPlayed { get; private set; }
        public EntryList<AvatarAwardEntry> AvatarAwards { get; private set; }
        public EntryList<ImageEntry> TheUnknowns { get; private set; } //HACK

        public List<XdbfFreeSpaceEntry> FreeSpace { get; set; }

        public List<XdbfEntry> Entries { get; private set; }

        public bool IsParsed { get; set; }

        public bool IsDirty { get; set; }

        protected GpdFile(OffsetTable offsetTable, BinaryContainer binary, int startOffset)
            : base(offsetTable, binary, startOffset)
        {
            _entryTableSize = EntryTableLength * EntrySize;
            _freeTableSize = FreeSpaceTableLength * FreeSpaceEntrySize;
            _tableSize = _entryTableSize + _freeTableSize + 0x18;

            Initialize();
        }

        protected virtual void Initialize()
        {
            IsParsed = false;
            Entries = new List<XdbfEntry>();
            Achievements = new EntryList<AchievementEntry>(this);
            Images = new EntryList<ImageEntry>(this);
            Settings = new SettingList(this);
            Strings = new EntryList<StringEntry>(this);
            TitlesPlayed = new EntryList<TitleEntry>(this);
            AvatarAwards = new EntryList<AvatarAwardEntry>(this);
            TheUnknowns = new EntryList<ImageEntry>(this); //HACK

            var pos = HeaderSize;

            for (var i = 0; i < EntryTableLength; i++)
            {
                BinMap.Add(pos, EntrySize, String.Empty, "XdbfEntry", null, null, i >= EntryCount);
                if (i < EntryCount)
                {
                    var entry = ModelFactory.GetModel<XdbfEntry>(Binary.ReadBytes(pos, EntrySize));
                    if (entry.Type != EntryType.Unknown) Entries.Add(entry);
                    BinMap.Add(pos, EntrySize, entry.Type.ToString(), "XdbfEntry", entry.Length, (entry.IsSyncList ? "SyncList" : entry.IsSyncData ? "SyncData" : String.Empty), i >= EntryCount);
                    //Debug.WriteLine("[{0}] {1}", entry.Id, entry.Type);
                }
                pos += EntrySize;
            }

            FreeSpace = new List<XdbfFreeSpaceEntry>();
            for (var i = 0; i < FreeSpaceTableLength; i++)
            {
                BinMap.Add(pos, FreeSpaceEntrySize, String.Empty, "XdbfFreeSpaceEntry", null, null,
                           i >= FreeSpaceTableEntryCount);
                if (i < FreeSpaceTableEntryCount)
                {
                    var entry = ModelFactory.GetModel<XdbfFreeSpaceEntry>(Binary.ReadBytes(pos, FreeSpaceEntrySize));
                    FreeSpace.Add(entry);
                    var contentAddress = GetRealAddress(entry.AddressSpecifier);
                    BinMap.Add(contentAddress, entry.Length, "<EMPTY>", "Unallocated content", null, null, true);
                }
                pos += FreeSpaceEntrySize;
            }
        }

        protected virtual IEnumerable<XdbfEntry> CollectParseEntries()
        {
            return Entries;
        }

        public void Parse()
        {
            if (IsParsed) return;

            foreach (var entry in CollectParseEntries())
            {
                var content = GetEntryContent(entry);
                AddEntry(entry, content);
            }
            IsParsed = true;
            BinMap.ClearCache();
        }

        public byte[] GetEntryContent(XdbfEntry entry)
        {
            var contentAddress = GetRealAddress(entry.AddressSpecifier);
            BinMap.Add(contentAddress, entry.Length,
                       entry.IsSyncData ? "SyncData" : entry.IsSyncList ? "SyncList" : String.Empty,
                       entry.Type.ToString());
            return Binary.ReadBytes(contentAddress, entry.Length);
        }

        private EntryBase AddEntry(XdbfEntry entry, byte[] content)
        {
            switch (entry.Type)
            {
                case EntryType.Achievement:
                    return Achievements.AddEntry(entry, content);
                case EntryType.Image:
                    return Images.AddEntry(entry, content);
                case EntryType.Setting:
                    return Settings.AddEntry(entry, content);
                case EntryType.Title:
                    return TitlesPlayed.AddEntry(entry, content);
                case EntryType.String:
                    return Strings.AddEntry(entry, content);
                case EntryType.AvatarAward:
                    return AvatarAwards.AddEntry(entry, content);
                case EntryType.MysteriousSeven:
                    return TheUnknowns.AddEntry(entry, content);
                default:
                    throw new NotSupportedException("Invalid entry type: " + entry.Type);
            }
        }

        public T AddNewEntry<T>(EntryType type, byte[] content, ulong? id = null) where T : EntryBase
        {
            if (id == null) id = GetNewId(type);
            var newEntry = ModelFactory.GetModel<XdbfEntry>();
            newEntry.Length = content.Length;
            newEntry.Type = type;
            newEntry.Id = id.Value;

            Entries.Add(newEntry);
            EntryCount++;

            IsDirty = true;

            return (T)AddEntry(newEntry, content);
        }

        public virtual void Recalculate()
        {
            IsDirty = true;
        }

        private int _rebuildAddress;
        private int _rebuildIndex;

        public void Rebuild()
        {
            if (!IsParsed) Parse();

            _rebuildAddress = 0;
            _rebuildIndex = 0;
            RewriteEntryList(Achievements, AchievementComparer.Instance);
            RewriteEntryList(Images);
            RewriteEntryList(Settings);
            RewriteEntryList(TitlesPlayed, TitleComparer.Instance);
            RewriteEntryList(Strings);
            RewriteEntryList(AvatarAwards);
            RewriteEntryList(TheUnknowns);
            EntryCount = _rebuildIndex;
            var eraser = new byte[(EntryTableLength - EntryCount) * EntrySize + FreeSpaceTableLength * FreeSpaceEntrySize];
            Binary.WriteBytes(HeaderSize + EntryCount * EntrySize, eraser, 0, eraser.Length);
            var freeSpaceAddress = GetRealAddress(_rebuildAddress);
            if (BinarySize == freeSpaceAddress) Binary.EnsureBinarySize(BinarySize + 0x1000);
            var freeSpaceLength = BinarySize - freeSpaceAddress;
            eraser = new byte[freeSpaceLength];
            Binary.WriteBytes(freeSpaceAddress, eraser, 0, eraser.Length);

            FreeSpaceTableEntryCount = 1;
            var freeSpaceEntry = FreeSpace[0];
            freeSpaceEntry.AddressSpecifier = freeSpaceLength;
            freeSpaceEntry.Length = -1 * freeSpaceLength - 1;
            Binary.WriteBytes(HeaderSize + EntryTableLength * EntrySize, freeSpaceEntry.Binary.ReadAll(), 0, FreeSpaceEntrySize);
            BinMap = new BinMap();

            Initialize();
            Parse();
        }

        private void RewriteEntryList<T>(EntryList<T> entryList, IComparer<T> comparer = null) where T : EntryBase
        {
            if (entryList == null) return;

            var length = entryList.Count * SyncEntry.Size;
            var syncListItems = entryList.ToList();
            if (comparer != null) syncListItems.Sort(comparer);
            var syncList = new BinaryContainer(length);

            
            foreach (var entry in entryList)
            {
                RewriteEntry(entry.Entry, entry.AllBytes);
            }
            var i = 0;
            foreach (var entry in syncListItems)
            {
                var item = ModelFactory.GetModel<SyncEntry>(syncList, i * 16);
                item.EntryId = entry.Entry.Id;
                item.SyncId = 0; //_syncId++; //?
                i++;
            }

            if (entryList.SyncList != null) RewriteEntry(entryList.SyncList.Entry, syncList.ReadAll());
            if (entryList.SyncData != null) RewriteEntry(entryList.SyncData.Entry, entryList.SyncData.AllBytes);
        }

        private void RewriteEntry(XdbfEntry entry, byte[] content)
        {
            var length = content.Length;
            entry.AddressSpecifier = _rebuildAddress;
            entry.Length = length;

            var offset = HeaderSize + (_rebuildIndex * EntrySize);
            Binary.WriteBytes(offset, entry.Binary.ReadAll(), 0, EntrySize);

            offset = GetRealAddress(_rebuildAddress);
            Binary.WriteBytes(offset, content, 0, length);

            _rebuildAddress += length;
            _rebuildIndex++;
        }

        protected bool HasEntry(EntryType type, int id)
        {
            return Entries.Any(e => e.Id == (ulong)id && e.Type == type);
        }

        protected ulong GetNewId(EntryType type)
        {
            var entries = Entries.Where(e => e.Type == type && e.Id < 256);
            if (entries.Any()) return entries.Max(e => e.Id) + 1;
            return 1;
        }

        internal int GetRealAddress(int specifier)
        {
            return specifier + _tableSize;
        }
    }
}