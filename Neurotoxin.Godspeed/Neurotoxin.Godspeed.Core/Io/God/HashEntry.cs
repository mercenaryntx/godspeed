using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.God
{
    public class HashEntry : BinaryModelBase
    {
        [BinaryData(0x14)]
        public virtual byte[] BlockHash { get; set; }

        public HashEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}