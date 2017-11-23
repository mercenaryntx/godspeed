using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Data
{
    public class HashEntry : BinaryModelBase
    {
        public const int Size = 0x18;

        [BinaryData(0x14)]
        public virtual byte[] BlockHash { get; set; }

        [BinaryData(0x1)]
        public virtual BlockStatus Status { get; set; }

        [BinaryData(0x3)]
        public virtual int NextBlock { get; set; }

        public int? Block { get; set; }
        public int RealBlock { get; set; }

        public HashEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}