using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xex
{
    public class XexCompressedBaseFile : BinaryModelBase
    {
        [BinaryData]
        public virtual int CompressionWindow { get; set; }

        [BinaryData]
        public virtual int DataSize { get; set; }

        [BinaryData(0x14)]
        public virtual byte[] Hash { get; set; }

        public XexCompressedBaseFile(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}