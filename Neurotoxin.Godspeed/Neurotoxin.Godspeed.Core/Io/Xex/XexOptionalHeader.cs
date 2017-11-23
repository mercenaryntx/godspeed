using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xex
{
    public class XexOptionalHeader : BinaryModelBase
    {
        public const int Size = 8;

        [BinaryData]
        public virtual XexOptionalHeaderId Id { get; set; }

        [BinaryData]
        public virtual uint Data { get; set; }

        public XexOptionalHeaderType Type
        {
            get
            {
                var id = (uint)Id;
                if ((id & 0xFF) == 0x01) return XexOptionalHeaderType.SimpleData;
                if ((id & 0xFF) == 0xFF) return XexOptionalHeaderType.DataSize;
                return XexOptionalHeaderType.EntrySize;
            }
        }

        public XexOptionalHeader(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}