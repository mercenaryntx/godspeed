using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xbe
{
    public class XbePe : BinaryModelBase
    {
        [BinaryData(EndianType.LittleEndian)]
        public virtual uint StackCommit { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint HeapReserve { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint HeapCommit { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint BaseAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint SizeOfImage { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint Checksum { get; set; }

        public XbePe(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}