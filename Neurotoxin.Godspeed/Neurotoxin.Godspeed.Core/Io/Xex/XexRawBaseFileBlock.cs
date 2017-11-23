using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xex
{
    public class XexRawBaseFileBlock : BinaryModelBase
    {
        [BinaryData]
        public virtual int DataSize { get; set; }

        [BinaryData]
        public virtual int ZeroSize { get; set; }

        public XexRawBaseFileBlock(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}