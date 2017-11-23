using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Iso
{
    public class VolumeDescriptor : BinaryModelBase
    {
        public const int SectorSize = 0x800;

        [BinaryData(20, "ascii")]
        public virtual string Magic { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint RootDirSector { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint RootDirSize { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual ulong ImageCreationTime { get; set; }

        public bool IsValid
        {
            get { return Magic == "MICROSOFT*XBOX*MEDIA"; }
        }

        public VolumeDescriptor(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}