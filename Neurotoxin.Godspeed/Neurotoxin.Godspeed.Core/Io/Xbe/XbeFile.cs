using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xbe
{
    public class XbeFile : BinaryModelBase
    {
        [BinaryData(4, "ascii")]
        public virtual string Magic { get; set; }

        [BinaryData(0x100)]
        public virtual byte[] DigitalSignature { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint BaseAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint SizeOfHeaders { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint SizeOfImage { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint SizeOfImageHeader { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint TimeDate { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint CertificateAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual int NumberOfSections { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint SectionHeadersAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual XbeInitFlags InitFlags { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint EntryPoint { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint TlsAddress { get; set; }

        [BinaryData(24)]
        public virtual XbePe Pe { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint DebugPathnameAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint DebugFilenameAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint DebugUnicodePathnameAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint KernelImageThunkAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint NonKernelImportDirectoryAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint NumberOfLibraryVersions { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint LibraryVersionsAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint KernelLibraryVersionAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint XapiLibraryVersionAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint LogoBitmapAddress { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual uint LogoBitmapSize { get; set; }

        public XbeCertificate Certificate { get; private set; }

        public List<XbeSection> Sections { get; private set; }

        public bool IsValid
        {
            get { return Magic == "XBEH"; }
        }

        public XbeFile(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        public void Initialize()
        {
            Certificate = ModelFactory.GetModel<XbeCertificate>(Binary, (int)(CertificateAddress - BaseAddress));

            var sectionOffset = (int)(SectionHeadersAddress - BaseAddress);
            Sections = new List<XbeSection>();
            for (var i = 0; i < NumberOfSections; i++)
            {
                var section = ModelFactory.GetModel<XbeSection>(Binary, sectionOffset);
                section.BaseAddress = (int)BaseAddress;
                Sections.Add(section);
                sectionOffset += section.OffsetTableSize;
            }
        }
    }
}
