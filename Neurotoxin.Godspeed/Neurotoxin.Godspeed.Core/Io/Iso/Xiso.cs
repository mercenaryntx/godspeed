using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Helpers;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Gpd.Entries;
using Neurotoxin.Godspeed.Core.Io.Xbe;
using Neurotoxin.Godspeed.Core.Io.Xex;
using Neurotoxin.Godspeed.Core.Io.Xpr;
using Neurotoxin.Godspeed.Core.Models;
using IsoType = Neurotoxin.Godspeed.Core.Constants.IsoType;

namespace Neurotoxin.Godspeed.Core.Io.Iso
{
    public class Xiso : IDisposable
    {
        private static string Slash = "\\";
        public const string DEFAULTXEX = "default.xex";
        public const string DEFAULTXBE = "default.xbe";
        private BinaryReader _binaryReader;
        private string _path;
        private static readonly byte[] EmptySector;
        private static readonly IsoTypeDescriptor _gamePartition = new IsoTypeDescriptor(IsoType.GamePartitionOnly);

        public IsoTypeDescriptor Type { get; private set; }
        public ContentType ContentType { get; private set; }
        public XisoDetails Details { get; private set; }
        public Tree<XisoFileEntry> RootDir { get; private set; }
        public VolumeDescriptor VolumeDescriptor { get; private set; }

        public long Size
        {
            get { return _binaryReader.BaseStream.Length; }
        }

        public long VolumeSize
        {
            get { return Size - Type.RootOffset; }
        }

        public long RealSize
        {
            get
            {
                return RootDir.Where(file => !file.IsDirectory).Max(file => file.Offset + RequiredSectors((int)file.Size.Value) * VolumeDescriptor.SectorSize) - Type.RootOffset;
            }
        }
        
        public int VolumeSectors
        {
            get { return (int)(VolumeSize/VolumeDescriptor.SectorSize); }
        }

        static Xiso()
        {
            EmptySector = Enumerable.Repeat((byte)0xFF, VolumeDescriptor.SectorSize).ToArray();
        }

        public Xiso(string path)
        {
            Initialize(path);
        }

        private void Initialize(string path)
        {
            if (_binaryReader != null) Dispose();

            _path = path;
            _binaryReader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            var types = Enum.GetValues(typeof(IsoType));
            foreach (IsoType type in types)
            {
                Type = new IsoTypeDescriptor(type);
                var data = Read(32, 0x24);
                VolumeDescriptor = ModelFactory.GetModel<VolumeDescriptor>(data);
                if (!VolumeDescriptor.IsValid) continue;
                RootDir = new Tree<XisoFileEntry>(Slash, null);
                Details = new XisoDetails();
                Parse(VolumeDescriptor.RootDirSector, 0);
                return;
            }
            throw new InvalidDataException("Invalid XISO Image!");
        }

        public IEnumerable<XisoFileEntry> GetDirContents(string path)
        {
            return RootDir.GetChildren(path);
        }

        public XisoFileEntry GetEntry(string path)
        {
            if (path.EndsWith(Slash)) path = path.Substring(0, path.Length - 1);
            return RootDir.Find(path);
        }

        public byte[] GetFile(string path, bool swallowException = false)
        {
            var entry = GetEntry(path);
            if (entry == null || entry.IsDirectory)
            {
                if (swallowException) return null;
                throw new FileNotFoundException("File cannot be found on path: " + path);
            }
            return GetFile(entry);
        }

        public byte[] GetFile(XisoFileEntry entry)
        {
            _binaryReader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
            if (!entry.Size.HasValue) throw new InvalidDataException("File size cannot be null");
            return _binaryReader.ReadBytes((int)entry.Size.Value);
        } 

        private void Parse(uint tocOffset, int offset, string dirPrefix = null)
        {
            if (string.IsNullOrWhiteSpace(dirPrefix)) dirPrefix = Slash;
            var data = Read(tocOffset, 256, offset);
            var tableData = ModelFactory.GetModel<XisoTableData>(data);
            if (tableData.Sector == uint.MaxValue) return;

            if (tableData.Left != 0) Parse(tocOffset, tableData.Left * 4, dirPrefix);

            var file = new XisoFileEntry
            {
                TableData = tableData,
                Name = tableData.Name,
                Flags = tableData.Flags,
                Path = dirPrefix + tableData.Name
            };
            RootDir.Insert(file.Path, file);
            if (tableData.IsDirectory)
            {
                file.Path += Slash;
                Parse(tableData.Sector, 0, file.Path);
            }
            else
            {
                file.Size = tableData.Size;
                file.Offset = Type.SectorToOffset(tableData.Sector);
                ParseSpecial(file);
            }
            if (tableData.Right != 0) Parse(tocOffset, tableData.Right * 4, dirPrefix);
        }

        private void ParseSpecial(XisoFileEntry file)
        {
            switch (file.Name)
            {
                case DEFAULTXEX:
                    ContentType = ContentType.GameOnDemand;
                    var xex = GetFile(file);
                    var xexContent = ModelFactory.GetModel<XexFile>(xex);
                    if (xexContent.IsValid)
                    {
                        xexContent.DecryptBaseFile();
                        var executionId = xexContent.Get<XexExecutionId>();
                        var xdbf = xexContent.GetResource(executionId.TitleId);
                        var gpd = ModelFactory.GetModel<GpdFile>(xdbf);

                        var entry = gpd.Entries.SingleOrDefault(e => e.Id == 1 && e.Type == EntryType.Setting);
                        var content = gpd.GetEntryContent(entry);
                        Details.Name = Encoding.UTF8.GetString(content, 0x12, content[0x11]);

                        const ulong tid = (ulong) SettingId.TitleInformation;
                        entry = gpd.Entries.SingleOrDefault(e => e.Id == tid && e.Type == EntryType.Image);
                        if (entry != null)
                        {
                            content = gpd.GetEntryContent(entry);
                            var image = ModelFactory.GetModel<ImageEntry>(content);
                            Details.Thumbnail = image.ImageData;
                        }

                        Details.TitleId = executionId.TitleId;
                        Details.MediaId = executionId.MediaId;
                        Details.Version = executionId.Version;
                        Details.BaseVersion = executionId.BaseVersion;
                        Details.DiscNumber = executionId.DiscNumber;
                        Details.DiscCount = executionId.DiscCount;
                        Details.ExecutionType = executionId.ExecutableType;
                        Details.Platform = executionId.Platform;
                    }
                    break;
                case DEFAULTXBE:
                    ContentType = ContentType.XboxOriginalGame;
                    var xbe = GetFile(file);
                    var xbeContent = ModelFactory.GetModel<XbeFile>(xbe);
                    if (xbeContent.IsValid)
                    {
                        xbeContent.Initialize();
                        Details.TitleId = xbeContent.Certificate.TitleId;
                        Details.Name = xbeContent.Certificate.TitleName;
                        Details.Version = xbeContent.Certificate.Version;
                        Details.DiscNumber = (byte)xbeContent.Certificate.DiscNumber;
                        if (Details.DiscNumber == 0) Details.DiscNumber = 1;
                        Details.DiscCount = Details.DiscNumber;
                        var section = xbeContent.Sections.FirstOrDefault(s => s.Name == "$$XSIMAGE") ??
                                      xbeContent.Sections.FirstOrDefault(s => s.Name == "$$XTIMAGE");
                        if (section != null)
                        {
                            var xpr = ModelFactory.GetModel<XprPackage>(section.Data);
                            if (xpr.IsValid)
                            {
                                var img = xpr.DecompressImage();
                                var ms = new MemoryStream();
                                img.Save(ms, ImageFormat.Png);
                                Details.Thumbnail = ms.ToArray();
                            }
                        }
                    }
                    break;
            }
        }

        public void Rebuild(string targetPath, bool skipSystemUpdate)
        {
            if (skipSystemUpdate) RootDir.Remove("\\$SystemUpdate");

            var filename = Path.GetFileName(_path).Replace(".iso", "_rebuilt.iso");
            var path = Path.Combine(targetPath, filename);

            if (File.Exists(path)) File.Delete(path);
            using (var stream = File.OpenWrite(path))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    var magic = Encoding.ASCII.GetBytes(VolumeDescriptor.Magic);
                    uint rootDirSector = 0x24;
                    uint rootDirSize;
                    var lastDataSector = WriteDirectoryContents(writer, Slash, 0x24, out rootDirSize);
                    var length = _gamePartition.SectorToOffset(lastDataSector);
                    writer.BaseStream.SetLength(length);

                    writer.Seek(0, SeekOrigin.Begin);
                    writer.Write((uint)0x1a465358);
                    writer.Write((uint)0x400);
                    writer.Seek(0x8000, SeekOrigin.Begin);
                    writer.Write(ResourceHelper.GetResourceBytes("gdf_sector.bin"));
                    writer.Seek(0x10000, SeekOrigin.Begin);
                    writer.Write(magic);
                    writer.Write(rootDirSector);
                    writer.Write(rootDirSize * VolumeDescriptor.SectorSize);
                    writer.Write(VolumeDescriptor.ImageCreationTime);
                    writer.Write((byte)1);
                    writer.BaseStream.Seek(0x107EC, SeekOrigin.Begin);
                    writer.Write(magic);
                    writer.BaseStream.Seek(8, SeekOrigin.Begin);

                    var sectors = ((uint)(length / VolumeDescriptor.SectorSize));
                    length -= 0x400;
                    writer.Write(length);
                    writer.Seek(0x8050, SeekOrigin.Begin);
                    writer.Write(sectors);
                    writer.Write(sectors);
                    
                }
            }

            Initialize(path);
        }

        private uint WriteDirectoryContents(BinaryWriter writer, string path, uint sector, out uint requiredSectors)
        {
            var tableEntries = RootDir.GetChildren(path)
                                      .OrderBy(e => e.IsDirectory)
                                      .Select(e => ModelFactory.GetModel<XisoTableData>(e.TableData.ReadAllBytes()))
                                      .ToArray();
            var requiredSize = tableEntries.Sum(t => t.BinarySize);
            requiredSectors = RequiredSectors(requiredSize);
            var dataSector = sector + requiredSectors;
            var offset = 0;

            for (var i = 0; i < tableEntries.Length; i++)
            {
                var t = tableEntries[i];
                t.Left = 0;
                t.Right = i != tableEntries.Length - 1 ? (ushort)((offset + t.BinarySize) / 4) : (ushort)0;

                if (t.IsDirectory)
                {
                    t.Sector = dataSector;
                    uint innerTableSectors;
                    dataSector = WriteDirectoryContents(writer, path + t.Name + Slash, dataSector, out innerTableSectors);
                }
                else
                {
                    var data = Read(t.Sector, (int)t.Size);
                    t.Sector = dataSector;
                    WriteToSector(writer, dataSector, 0, data);
                    dataSector += RequiredSectors((int)t.Size);
                }
                WriteToSector(writer, sector, offset, t.ReadAllBytes());
                offset += t.BinarySize;
            }
            WriteToSector(writer, sector, offset, EmptySector, VolumeDescriptor.SectorSize * (int)requiredSectors - offset);
            return dataSector;
        }

        public void Seek(long offset, SeekOrigin seekOrigin = SeekOrigin.Begin)
        {
            if (seekOrigin == SeekOrigin.Begin) offset += Type.RootOffset;
            _binaryReader.BaseStream.Seek(offset, seekOrigin);
        }

        public void WriteToSector(BinaryWriter writer, uint sector, int offset, byte[] data, int? length = null)
        {
            try
            {
                if (!length.HasValue) length = data.Length;
                writer.BaseStream.Seek(_gamePartition.SectorToOffset(sector) + offset, SeekOrigin.Begin);
                writer.Write(data, 0, length.Value);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        public byte[] ReadBytes(int length)
        {
            return _binaryReader.ReadBytes(length);
        }

        private byte[] Read(uint sector, int length, int offset = 0)
        {
            _binaryReader.BaseStream.Seek(Type.SectorToOffset(sector) + offset, SeekOrigin.Begin);
            return _binaryReader.ReadBytes(length);
        }

        private uint RequiredSectors(int size)
        {
            return (uint)Math.Ceiling((double) size/VolumeDescriptor.SectorSize);
        }

        public void Dispose()
        {
            _binaryReader.Close();
            _binaryReader = null;
            GC.Collect();
        }
    }
}