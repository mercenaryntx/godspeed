using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Iso
{
    public class XisoFileEntry : INamed
    {
        public string Name { get; set; }
        public long Offset { get; set; }
        public string Path { get; set; }
        public uint? Size { get; set; }
        public XisoFlags Flags { get; set; }
        public XisoTableData TableData { get; set; }

        public bool IsDirectory
        {
            get { return Flags.HasFlag(XisoFlags.Directory); }
        }
    }
}