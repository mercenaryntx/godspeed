using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Data
{
    public class SvodVolumeDescriptor : BinaryModelBase, IVolumeDescriptor
    {
        [BinaryData(0x1)]
        public virtual int Size { get; set; }

        [BinaryData(0x1)]
        public virtual int BlockCacheElementCount { get; set; }

        [BinaryData(0x1)]
        public virtual int WorkerThreadProcessor { get; set; }

        [BinaryData(0x1)]
        public virtual int WorkerThreadPriority { get; set; }

        [BinaryData(0x14)]
        public virtual byte[] Hash { get; set; }

        [BinaryData(0x1)]
        public virtual int DeviceFeatures { get; set; }

        [BinaryData(0x3)]
        public virtual int DataBlockCount { get; set; }

        [BinaryData(0x3)]
        public virtual int DataBlockOffset { get; set; }

        [BinaryData(0x5)]
        public virtual int Padding { get; set; }

        public SvodVolumeDescriptor(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}