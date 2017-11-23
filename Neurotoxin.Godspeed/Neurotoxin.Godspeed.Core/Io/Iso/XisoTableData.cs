using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Iso
{
    public class XisoTableData : BinaryModelBase
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding(1252);

        [BinaryData(EndianType.LittleEndian)]
        public virtual ushort Left { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual ushort Right { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint Sector { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint Size { get; set; }

        [BinaryData(1)]
        public virtual XisoFlags Flags { get; set; }

        [BinaryData(1)]
        public virtual byte NameLength { get; set; }

        public string Name
        {
            get
            {
                var buffer = Binary.ReadBytes(14, NameLength);
                return Encoding.GetString(buffer);
            }
        }

        public bool IsDirectory
        {
            get { return Flags.HasFlag(XisoFlags.Directory); }
        }

        public override int BinarySize
        {
            get
            {
                var padding = 4 - ((14 + NameLength) % 4);
                if (padding == 4) padding = 0;
                return 14 + NameLength + padding;
            }
        }

        public XisoTableData(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}