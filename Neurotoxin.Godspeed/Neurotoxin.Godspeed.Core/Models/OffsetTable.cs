using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Io.Stfs;

namespace Neurotoxin.Godspeed.Core.Models
{
    public class OffsetTable
    {
        //TODO: nullterminateds should reset _size;
        private int? _size;
        public int Size 
        {
            get
            {
                if (IsDynamic || !_size.HasValue)
                {
                    _size = Properties.Count == 0 ? 0 : Properties.Where(o => o.Value != null && o.Value.Length > 0).Sum(o => o.Value.Length);
                }
                return _size.Value;
            }
        }
        private Dictionary<string, BinaryLocation> Properties { get; set; }
        public bool IsDynamic { get; set; }

        public List<string> Keys
        {
            get { return Properties.Keys.ToList(); }
        }

        public OffsetTable()
        {
            Properties = new Dictionary<string, BinaryLocation>();
        }

        public OffsetTable Clone(int shift)
        {
            var newTable = new OffsetTable {IsDynamic = IsDynamic};
            if (!IsDynamic) newTable._size = Size;
            newTable.Properties = new Dictionary<string, BinaryLocation>();
            foreach (var kvp in Properties)
            {
                newTable.Properties.Add(kvp.Key, kvp.Value == null ? null : new BinaryLocation(kvp.Value.Offset + shift, kvp.Value.Length));
            }
            return newTable;
        }

        public void ShiftOffsets(int diff)
        {
            var keys = Properties.Keys;
            foreach (var key in keys.Where(key => Properties[key] != null))
            {
                Properties[key].Offset += diff;
            }
        }

        public void ShiftOffsetsFrom(string propertyName, int diff)
        {
            var keys = Properties.Keys;
            var found = false;
            foreach (var key in keys)
            {
                if (found && Properties[key] != null) Properties[key].Offset += diff;
                if (key == propertyName)
                    found = true;
            }
        }

        public void MapOffsets(BinMap binMap, Type type)
        {
            foreach (var kvp in Properties.Where(kvp => kvp.Value != null))
            {
                binMap.Add(kvp.Value.Offset, kvp.Value.Length, kvp.Key, type.Name);
            }
        }

        public string Log()
        {
            var sb = new StringBuilder();
            foreach (var kvp in Properties.Where(kvp => kvp.Value != null))
            {
                sb.AppendLine(string.Format("0x{0,3:X3} 0x{1,3:X3} {2}", kvp.Value.Offset, kvp.Value.Length, kvp.Key));
            }
            return sb.ToString();
        }

        public void Add(string key, int offset, int length)
        {
            Add(key, new BinaryLocation(offset, length));
        }

        public void Add(string key, BinaryLocation value)
        {
            Properties.Add(key, value);
        }

        public BinaryLocation this[string key]
        {
            get
            {
                if (!Properties.ContainsKey(key))
                    throw new KeyNotFoundException(string.Format("Offset information of property {0} cannot be found", key));
                return Properties[key];
            }
            set { Properties[key] = value; }
        }

        public bool Contains(string key)
        {
            return Properties.ContainsKey(key);
        }
    }
}