using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Neurotoxin.Godspeed.Core.Models
{
    public class BinMap
    {
        private readonly Dictionary<int, BinMapEntry> _log = new Dictionary<int, BinMapEntry>();
        private Tuple<int, int, bool>[] _highlightCache;

        public int[] Keys
        {
            get { return _log.Keys.ToArray(); }
        }

        public void Add(int offset, int? length, string propertyName, string className, int? blockNum = null, string description = null, bool? free = false)
        {
            if (!_log.ContainsKey(offset))
            {
                _log.Add(offset, new BinMapEntry
                {
                    Length = length,
                    PropertyName = propertyName,
                    ClassName = className,
                    BlockNum = blockNum,
                    Description = description,
                    Free = free.HasValue && free.Value
                });
                return;
            }

            if (length != null) _log[offset].Length = length;
            if (propertyName != null) _log[offset].PropertyName = propertyName;
            if (className != null) _log[offset].ClassName = className;
            if (description != null) _log[offset].Description = description;
            if (blockNum != null) _log[offset].BlockNum = blockNum;
            if (free != null) _log[offset].Free = free.Value;
            _highlightCache = null;
        }

        public int[] Highlight(int start, int end)
        {
            if (_highlightCache == null)
            {
                _highlightCache = _log.Where(kvp => kvp.Value.Length.HasValue)
                                      .Select(kvp => new Tuple<int, int, bool>(kvp.Key, kvp.Key + kvp.Value.Length.Value, kvp.Value.Free)).ToArray();
            }
            var x = _highlightCache.Where(t =>
                                   {
                                       var a = start > t.Item1 ? start : t.Item1;
                                       var b = end < t.Item2 ? end : t.Item2;
                                       return b >= a;
                                   })
                        .ToArray();
            var y = new int[end - start];
            for (var i = 0; i < end-start; i++)
            {
                var z = x.Where(t => t.Item1 <= start + i && t.Item2 > start + i).ToArray();
                var c = z.Count();
                if (c > 1) throw new Exception("BinMap OVERLAP!");
                y[i] = c == 0 ? 0 : z[0].Item3 ? 2 : 1;
            }
            return y;
        }

        public Tuple<int, BinMapEntry> Get(int offset)
        {
            var entry = _log.Where(kvp => kvp.Value.Length.HasValue)
                            .Where(kvp => kvp.Key <= offset && kvp.Key + kvp.Value.Length.Value > offset).ToArray();
            return entry.Any() ? new Tuple<int, BinMapEntry>(entry[0].Key, entry[0].Value) : null;
        }

        public string[] Output()
        {
            var keys = _log.Keys.ToArray();
            Array.Sort(keys);
            return (from key in keys
                    let entry = _log[key]
                    let block = entry.BlockNum.HasValue ? entry.BlockNum.Value.ToString(CultureInfo.InvariantCulture) : string.Empty
                    let blockTab = block.Length > 2 ? "\t" : "\t\t"
                    let propTab = entry.PropertyName != null && entry.PropertyName.Length > 7 ? entry.PropertyName.Length > 15 ? "\t" : "\t\t" : "\t\t\t"
                    let classTab = entry.ClassName != null && entry.ClassName.Length > 7 ? entry.ClassName.Length > 15 ? "\t" : "\t\t" : "\t\t\t"
                    select string.Format("[0x{0,8:X8}]-[0x{8,8:X8}] {1}{4}{2}{5}{3}{6}{7}", key, block, entry.PropertyName, entry.ClassName, blockTab, propTab, classTab, entry.Description, key + (entry.Length.HasValue ? entry.Length.Value : 0))).ToArray();
        }

        public void ClearCache()
        {
            _highlightCache = null;
        }
    }
}