using System.Collections.Generic;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Data
{
    public class FileEntry : BinaryModelBase
    {
        [BinaryData(0x28, "ascii", StringReadOptions.AutoTrim)]
        public virtual string Name { get; set; }

        [BinaryData(1)]
        public virtual FileEntryFlags Flags { get; set; }

        [BinaryData(3, EndianType.LittleEndian)]
        public virtual int BlocksForFile { get; set; }

        [BinaryData(3, EndianType.LittleEndian)]
        public virtual int BlocksForFileCopy { get; set; }

        [BinaryData(3, EndianType.LittleEndian)]
        public virtual int StartingBlockNum { get; set; }

        [BinaryData]
        public virtual ushort PathIndicator { get; set; }

        [BinaryData]
        public virtual int FileSize { get; set; }

        [BinaryData]
        public virtual int CreatedTimeStamp { get; set; }

        [BinaryData]
        public virtual int AccessTimeStamp { get; set; }

        public int EntryIndex { get; set; }
        public int FileEntryAddress { get; set; }

        public List<FileEntry> Files { get; set; }
        public List<FileEntry> Folders { get; set; }

        public bool BlocksAreConsecutive
        {
            get { return Flags.HasFlag(FileEntryFlags.BlocksAreConsecutive); }
            set
            {
                if (value)
                    Flags |= FileEntryFlags.BlocksAreConsecutive;
                else
                    Flags &= ~FileEntryFlags.BlocksAreConsecutive;
            }
        }

        public bool IsDirectory
        {
            get { return Flags.HasFlag(FileEntryFlags.IsDirectory) || Name == "Root"; }
        }

        public FileEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
            Files = new List<FileEntry>();
            Folders = new List<FileEntry>();
        }
    }
}