using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Gpd
{
    public class XdbfFreeSpaceEntry : BinaryModelBase
    {
        [BinaryData]
        public virtual int AddressSpecifier { get; set; }

        [BinaryData]
        public virtual int Length { get; set; }

        public XdbfFreeSpaceEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}