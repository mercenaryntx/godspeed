using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Data
{
    public class PackageSignature : BinaryModelBase, IPackageSignature
    {
        [BinaryData(0x100)]
        public virtual byte[] Signature { get; set; }

        public PackageSignature(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}