using System;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Data
{
    public class ProgressCache : BinaryModelBase, IInstallerInformation
    {
        [BinaryData(4)]
        public virtual OnlineContentResumeState ResumeState { get; set; }

        [BinaryData]
        public virtual uint CurrentFileIndex { get; set; }

        [BinaryData]
        public virtual uint CurrentFileOffset { get; set; }

        [BinaryData]
        public virtual long ByteProcessed { get; set; }

        [BinaryData]
        public virtual DateTime LastModified { get; set; }

        [BinaryData(0x15D0)]
        public virtual byte[] CabResumeData { get; set; }

        public ProgressCache(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}