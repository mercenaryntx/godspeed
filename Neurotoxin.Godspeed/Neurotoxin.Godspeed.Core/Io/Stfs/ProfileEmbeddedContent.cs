using System.Collections.Generic;
using System.IO;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs
{
    [DeclaredOnly]
    public class ProfileEmbeddedContent : StfsPackage
    {
        public override int HeaderSize
        {
            get { return 0x1000; }
            set {}
        }

        [BinaryData(0x228)]
        public override Certificate Certificate { get; set; }

        [BinaryData(0x14)]
        public override byte[] HeaderHash { get; set; }

        [BinaryData(0x8)]
        protected long Unknown1 { get; set; }

        [BinaryData(0x24)]
        public override StfsVolumeDescriptor VolumeDescriptor { get; set; }

        [BinaryData(0x4)]
        protected int Unknown2 { get; set; }

        [BinaryData(0x8, StringReadOptions.ID)]
        public override string ProfileId { get; set; }

        [BinaryData(0x1)]
        protected int Unknown3 { get; set; }

        [BinaryData(0x5, StringReadOptions.ID)]
        public override string ConsoleId { get; set; }

        public ProfileEmbeddedContent(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        protected override void Resign(Stream kv)
        {
            ResignPackage(kv, 0x23C, 0xDC4, 0x23C);
        }

        public override void ExtractGames()
        {
            Games = new Dictionary<FileEntry, GameFile>();
            foreach (var gpd in FileStructure.Files)
            {
                GetGameFile(gpd);
            }
        }

    }
}