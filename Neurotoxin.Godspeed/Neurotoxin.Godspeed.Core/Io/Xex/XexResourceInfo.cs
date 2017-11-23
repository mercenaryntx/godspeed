using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xex
{
    [XexHeader(XexOptionalHeaderId.ResourceInfo)]
    public class XexResourceInfo : BinaryModelBase
    {
        [BinaryData(8, "ascii", StringReadOptions.AutoTrim)]
        public virtual string Name { get; set; }

        [BinaryData]
        public virtual uint Offset { get; set; }

        [BinaryData]
        public virtual uint Size { get; set; }

        public XexResourceInfo(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}