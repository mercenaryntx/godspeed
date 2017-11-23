using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Gpd.Entries
{
    public class SyncEntry : BinaryModelBase
    {
        public const int Size = 16;

        [BinaryData]
        public virtual ulong EntryId { get; set; }
        [BinaryData]
        public virtual ulong SyncId { get; set; }

        public SyncEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}