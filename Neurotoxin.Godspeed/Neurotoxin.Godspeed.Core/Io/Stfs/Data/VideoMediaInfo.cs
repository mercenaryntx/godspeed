using System;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Data
{
    public class VideoMediaInfo : BinaryModelBase, IMediaInfo
    {
        [BinaryData(0x10)]
        public byte[] SeriesId { get; set; }

        [BinaryData(0x10)]
        public byte[] SeasonId { get; set; }

        [BinaryData]
        public ushort SeasonNumber { get; set; }

        [BinaryData]
        public ushort EpisodeNumber { get; set; }

        public VideoMediaInfo(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}