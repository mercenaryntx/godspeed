using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Gpd
{
    public abstract class XdbfEntry : BinaryModelBase
    {
        [BinaryData(2)]
        public virtual EntryType Type { get; set; }

        [BinaryData]
        public virtual ulong Id { get; set; }

        [BinaryData]
        public virtual int AddressSpecifier { get; set; }

        [BinaryData]
        public virtual int Length { get; set; }

        public bool IsSyncData
        {
            get { return Id == (ulong) (Type == EntryType.AvatarAward ? 2 : 0x200000000); }
        }

        public bool IsSyncList
        {
            get { return Id == (ulong)(Type == EntryType.AvatarAward ? 1 : 0x100000000); }
        }

        public XdbfEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}