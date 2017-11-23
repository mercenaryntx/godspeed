using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xbe
{
    public class XbeSection : BinaryModelBase
    {
        [BinaryData(EndianType.LittleEndian)]
        public virtual XbeSectionFlags Flags { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint VirtualAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint VirtualSize { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual int RawAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual int RawSize { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint SectionNameAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint SectionNameRefCount { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint HeadSharedPageRefCountAddress { get; private set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint TailSharedPageRefCountAddress { get; private set; }

        [BinaryData(20)]
        public virtual byte[] SectionDigest { get; set; }

        public string Name
        {
            get
            {
                var offset = (int)(SectionNameAddress - BaseAddress);
                var length = 0;
                while (Binary[offset + length] != 0) length++;

                var buffer = Binary.ReadBytes(offset, length);
                return Encoding.ASCII.GetString(buffer);
            }
        }

        public byte[] Data
        {
            get { return Binary.ReadBytes(RawAddress, RawSize); }
        }

        public int BaseAddress { get; set; }

        public XbeSection(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}