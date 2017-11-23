using System;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xex
{
    [XexHeader(XexOptionalHeaderId.ExecutionId)]
    public class XexExecutionId : BinaryModelBase
    {
        [BinaryData(4, StringReadOptions.ID)]
        public virtual string MediaId { get; set; }

        [BinaryData(4)]
        public virtual Version Version { get; set; }

        [BinaryData(4)]
        public virtual Version BaseVersion { get; set; }

        [BinaryData(4, StringReadOptions.ID)]
        public virtual string TitleId { get; set; }

        [BinaryData]
        public virtual byte Platform { get; set; }

        [BinaryData]
        public virtual byte ExecutableType { get; set; }

        [BinaryData]
        public virtual byte DiscNumber { get; set; }

        [BinaryData]
        public virtual byte DiscCount { get; set; }

        public XexExecutionId(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}