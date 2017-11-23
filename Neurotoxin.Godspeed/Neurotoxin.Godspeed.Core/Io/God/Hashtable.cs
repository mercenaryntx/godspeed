using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.God
{
    public class HashTable : BinaryModelBase
    {
        [BinaryData(0xCC)]
        public virtual HashEntry[] Entries { get; set; }

        public HashTable(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}