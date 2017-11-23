using System;
using Neurotoxin.Godspeed.Core.Constants;

namespace Neurotoxin.Godspeed.Core.Io.Gpd.Entries
{
    public class SettingList : EntryList<SettingEntry>
    {
        public SettingList(GpdFile parent) : base(parent) {}

        public T Get<T>(SettingId id)
        {
            var setting = Get((ulong) id);
            if (setting != null) return setting.GetValue<T>();
            return (T) DefaultValue(typeof (T));
        }

        public void Set<T>(SettingId id, T value)
        {
            var setting = Get((ulong) id);
            if (setting == null)
            {
                setting = _parent.AddNewEntry<SettingEntry>(EntryType.Setting, new byte[24], (ulong)id);
                setting.Type = SettingEntry.GetSettingTypeFromType(typeof (T));
            }
            setting.SetValue(value);
        }

        private static object DefaultValue(Type type) 
        {
            if (type == typeof(string) || type == typeof(byte[])) return null;
            if (type == typeof(DateTime)) return DateTime.MinValue;
            return 0;
        }
    }
}