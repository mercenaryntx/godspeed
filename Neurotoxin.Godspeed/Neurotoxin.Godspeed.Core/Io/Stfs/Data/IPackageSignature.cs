using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Data
{
    public interface IPackageSignature : IBinaryModel
    {
        byte[] Signature { get; set; }
    }
}