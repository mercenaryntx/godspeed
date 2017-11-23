using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xex
{
    public class XexFileHeader : BinaryModelBase
    {
        [BinaryData]
        public virtual uint HeaderSize { get; set; }

        [BinaryData]
        public virtual uint ImageSize { get; set; }

        [BinaryData(0x100)]
        public virtual byte[] RsaSignature { get; set; }

        [BinaryData]
        public virtual uint Length2 { get; set; }

        [BinaryData]
        public virtual uint ImageFlags { get; set; }

        [BinaryData]
        public virtual uint LoadAddress { get; set; }

        [BinaryData(0x14)]
        public virtual byte[] SectionDigest { get; set; }

        [BinaryData]
        public virtual int ImportTableCount { get; set; }

        [BinaryData(0x14)]
        public virtual byte[] ImportTableDigest { get; set; }

        [BinaryData(0x10)]
        public virtual byte[] MediaId { get; set; }

        [BinaryData(0x10)]
        public virtual byte[] AesKey { get; set; }

        [BinaryData]
        public virtual uint ExportTable { get; set; }

        [BinaryData(0x14)]
        public virtual byte[] HeaderDigest { get; set; }

        [BinaryData]
        public virtual uint Region { get; set; }

        public XexFileHeader(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}