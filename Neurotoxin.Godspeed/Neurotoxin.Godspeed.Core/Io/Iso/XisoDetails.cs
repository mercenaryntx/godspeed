using System;

namespace Neurotoxin.Godspeed.Core.Io.Iso
{
    public class XisoDetails
    {
        public string Name { get; set; }
        public string TitleId { get; set; }
        public string MediaId { get; set; }
        public Version Version { get; set; }
        public Version BaseVersion { get; set; }
        public byte ExecutionType { get; set; }
        public byte Platform { get; set; }
        public byte DiscNumber { get; set; }
        public byte DiscCount { get; set; }
        public byte[] Thumbnail { get; set; }
    }
}