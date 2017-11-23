using System;
using System.Collections.Generic;
using System.Linq;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Gpd.Entries
{
    public class EntryList<T> : SortedSet<T> where T :EntryBase
    {
        public SyncData SyncData { get; set; }
        public SyncList SyncList { get; set; }

        protected GpdFile _parent;
        protected EntryType _entryType;

        public EntryList(GpdFile parent)
        {
            _parent = parent;
            _entryType = EntryType.Unknown;
            var type = typeof(T);
            if (type == typeof(AchievementEntry)) _entryType = EntryType.Achievement;
            else if (type == typeof(AvatarAwardEntry)) _entryType = EntryType.AvatarAward;
            else if (type == typeof(ImageEntry)) _entryType = EntryType.Image;
            else if (type == typeof(SettingEntry)) _entryType = EntryType.Setting;
            else if (type == typeof(StringEntry)) _entryType = EntryType.String;
            else if (type == typeof(TitleEntry)) _entryType = EntryType.Title;
            else throw new NotSupportedException("Unknown type: " + type.Name);
        }

        public T AddEntry(XdbfEntry entry, byte[] binary)
        {
            if (entry.IsSyncList)
            {
                var container = new BinaryContainer(binary);
                var entryCount = entry.Length / 16;
                SyncList = new SyncList { Entry = entry, Binary = container };
                for (var i = 0; i < entryCount; i++)
                    SyncList.Add(ModelFactory.GetModel<SyncEntry>(container, i * 16));
                return null;
            }

            if (entry.IsSyncData)
            {
                SyncData = ModelFactory.GetModel<SyncData>(binary);
                SyncData.Entry = entry;
                return null;
            }

            var model = ModelFactory.GetModel<T>(binary);
            model.Entry = entry;
            Add(model);
            return model;
        }

        public T Get(int id)
        {
            return Get((ulong) id);
        }

        public T Get(ulong id)
        {
            var item = this.FirstOrDefault(e => e.Entry.Id == id);
            if (item == null)
            {
                var entry = _parent.Entries.FirstOrDefault(e => e.Id == id && e.Type == _entryType);
                if (entry == null) return null;

                var content = _parent.GetEntryContent(entry);
                item = AddEntry(entry, content);
            }
            return item;
        }

        public List<T> OrderBySyncList()
        {
            if (SyncList == null) return this.ToList();
            var result = new List<T>();
            foreach (var entry in SyncList)
            {
                result.Insert(0, this.First(item => item.Entry.Id == entry.EntryId));
            }
            return result;
        }
    }

}