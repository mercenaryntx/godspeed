using System;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xex
{
    [XexHeader(XexOptionalHeaderId.StaticLibraries)]
    public class XexLibrary : BinaryModelBase
    {

        [BinaryData(8, "ascii", StringReadOptions.AutoTrim)]
        public virtual string Name { get; set; }

        [BinaryData(8)]
        public virtual Version Version { get; set; }

        public XexLibrary(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}