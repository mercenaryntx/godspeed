using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs
{
    public abstract class SvodPackage : Package<SvodVolumeDescriptor>
    {
        protected SvodPackage(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        protected override void Parse()
        {
        }

        public override void Rehash()
        {
            throw new NotImplementedException();
        }


    }
}