using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Core.Security;

namespace Neurotoxin.Godspeed.Core.Io.Xex
{
    public class XexFile : BinaryModelBase
    {
        private readonly MemoryStream _baseFileStream = new MemoryStream();
        private readonly byte[] _retailKey2 =
        {
            0x20, 0xB1, 0x85, 0xA5, 0x9D, 0x28, 0xFD, 0xC3,
            0x40, 0x58, 0x3F, 0xBB, 0x08, 0x96, 0xBF, 0x91
        };

        [BinaryData(4, "ascii")]
        public virtual string Magic { get; set; }

        [BinaryData]
        public virtual XexFlags Flags { get; set; }

        [BinaryData]
        public virtual uint DataOffset { get; set; }

        [BinaryData]
        public virtual uint Reserved { get; set; }

        [BinaryData]
        public virtual int FileHeaderOffset { get; set; }

        [BinaryData]
        public virtual uint OptionalHeaderCount { get; set; }

        private XexOptionalHeader[] _optionalHeaders;
        public XexOptionalHeader[] OptionalHeaders
        {
            get
            {
                if (_optionalHeaders == null)
                {
                    var c = (int) OptionalHeaderCount;
                    _optionalHeaders = new XexOptionalHeader[c];
                    if (c > 0)
                    {
                        var data = Binary.ReadBytes(24, c * XexOptionalHeader.Size);
                        var offset = 0;
                        for (var i = 0; i < c; i++)
                        {
                            _optionalHeaders[i] = ModelFactory.GetModel<XexOptionalHeader>(data, offset);
                            offset += XexOptionalHeader.Size;
                        }
                    }
                }
                return _optionalHeaders;
            }
        }

        private XexFileHeader _header;
        public XexFileHeader Header
        {
            get { return _header ?? (_header = ModelFactory.GetModel<XexFileHeader>(Binary, FileHeaderOffset)); }
        }

        private XexCompressionInfo _compressionInfo;
        public XexCompressionInfo CompressionInfo
        {
            get {
                return _compressionInfo ??
                       (_compressionInfo = ModelFactory.GetModel<XexCompressionInfo>(Binary, (int) XexOptionalHeaderId.DecompressionInformation));
            }
        }

        public bool IsValid
        {
            get { return Magic == "XEX2"; }
        }

        public XexFile(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        public bool DecryptBaseFile()
        {
            return DecryptBaseFile(_baseFileStream);
        }

        private bool DecryptBaseFile(Stream outBaseFile)
        {
            var tempStream = new MemoryStream();
            var ci = Get<XexCompressionInfo>();

            // Incase its compressed
            XexCompressedBaseFile compBlock;

            #region Decrypt if encrypted

            if (ci.EncryptionType == XexEncryptionType.Encrypted)
            {
                #region Calculate our key

                var aes = Rijndael.Create();
                aes.Padding = PaddingMode.None;
                var ms = new MemoryStream(Header.AesKey);
                var decryptKey = new byte[0x10];
                using (var decrypt = new CryptoStream(ms, aes.CreateDecryptor(_retailKey2, new byte[0x10]), CryptoStreamMode.Read))
                {
                    decrypt.Read(decryptKey, 0, 0x10);
                }

                #endregion

                // Now decrypt our data
                aes = Rijndael.Create();
                aes.Padding = PaddingMode.None;
                var dataStream = Binary.GetStream();
                dataStream.Seek(DataOffset, SeekOrigin.Begin);
                using (var decrypt = new CryptoStream(dataStream, aes.CreateDecryptor(decryptKey, new byte[0x10]), CryptoStreamMode.Read))
                {
                    #region Decrypt non compressed

                    if (ci.CompressionType == XexCompressionType.NotCompressed)
                    {
                        // Loop through each section and decrypt
                        foreach (var block in ci.RawBlocks)
                        {
                            var buffer = new byte[block.DataSize];
                            decrypt.Read(buffer, 0, buffer.Length);
                            outBaseFile.Write(buffer, 0, buffer.Length);
                            outBaseFile.Write(new byte[block.ZeroSize], 0, block.ZeroSize);
                        }

                        // Set our length to image size
                        outBaseFile.Seek(0, SeekOrigin.Begin);
                        outBaseFile.SetLength(Header.ImageSize);

                        // Return true
                        return true;
                    }

                    #endregion

                    #region Decrypt compressed

                    compBlock = ci.CompressedBaseFile;

                    var index = 0;
                    while (compBlock.DataSize != 0)
                    {
                        var testData = new byte[compBlock.DataSize];
                        dataStream.Read(testData, 0, compBlock.DataSize);
                        dataStream.Seek(compBlock.DataSize * -1, SeekOrigin.Current);

                        // Decrypt our data size
                        var buffer = new byte[compBlock.DataSize];
                        decrypt.Read(buffer, 0, buffer.Length);

                        // Verify our hash
                        var hash = SHA1.Create().ComputeHash(buffer);
                        for (var x = 0; x < 0x14; x++)
                            if (hash[x] != compBlock.Hash[x]) throw new Exception("Bad hash");

                        // Write each block
                        tempStream.Write(buffer, 0, compBlock.DataSize);

                        compBlock = ModelFactory.GetModel<XexCompressedBaseFile>(buffer);
                        index = index + 1;
                    }

                    tempStream.Seek(0, SeekOrigin.Begin);

                    #endregion
                }
                dataStream.Close();
            }
            else
            {
                #region Write out non compressed

                if (ci.CompressionType == XexCompressionType.NotCompressed)
                {
                    // Loop through each section and decrypt
                    foreach (var block in ci.RawBlocks)
                    {
                        var buffer = Binary.ReadBytes((int)DataOffset, block.DataSize);
                        outBaseFile.Write(buffer, 0, buffer.Length);
                        outBaseFile.Write(new byte[block.ZeroSize], 0, block.ZeroSize);
                    }

                    // Set our length to image size
                    outBaseFile.Seek(0, SeekOrigin.Begin);
                    outBaseFile.SetLength(Header.ImageSize);

                    // Return true
                    return true;
                }

                #endregion

                // Since we didnt write to the temp stream 
                // lets just take it from the file
            }

            #endregion

            // Time to work with compresed
            var sizeLeft = (int)Header.ImageSize;
            compBlock = ci.CompressedBaseFile;

            #region Handle Delta Compresed

            if (ci.CompressionType == XexCompressionType.DeltaCompressed)
            {
                while (compBlock.DataSize != 0)
                {
                    var buffer = new byte[compBlock.DataSize];
                    tempStream.Read(buffer, 0, buffer.Length);
                    outBaseFile.Write(buffer, 0, compBlock.DataSize);

                    compBlock = ModelFactory.GetModel<XexCompressedBaseFile>(buffer);
                }

                // Seek back to start
                outBaseFile.Seek(0, SeekOrigin.Begin);

                // return
                return true;
            }

            #endregion

            #region Decompress if compressed

            #region Create Decompression Ctx

            int maxSize = 0x8000, ctx = -1, unknown = 0;
            XCompress.LzxDecompress lzx;
            lzx.CpuType = 1;
            lzx.WindowSize = ci.CompressedBaseFile.CompressionWindow;
            IntPtr allocate = Marshal.AllocHGlobal(0x23200);
            if (XCompress.LDICreateDecompression(ref maxSize, ref lzx,
                0, 0, allocate, ref unknown, ref ctx) != 0)
                throw new Exception("Failed to create decompression");

            #endregion

            while (compBlock.DataSize != 0)
            {
                var buffer = new byte[compBlock.DataSize];
                tempStream.Read(buffer, 0, buffer.Length);

                compBlock = ModelFactory.GetModel<XexCompressedBaseFile>(buffer);
                var offset = compBlock.OffsetTableSize;

                #region Decompress

                while (true)
                {
                    int compressedLen = BitConverter.ToUInt16(buffer, offset);
                    if (compressedLen == 0) break;
                    offset += 2;
                    var compressedData = new byte[compressedLen];
                    Buffer.BlockCopy(buffer, offset, compressedData, 0, compressedLen);
                    offset += compressedLen;
                    var decompressedLength = (sizeLeft < 0x8000) ? sizeLeft : 0x8000;
                    var decompressedData = new byte[decompressedLength];
                    if (XCompress.LDIDecompress(ctx, compressedData,
                        compressedLen, decompressedData, ref decompressedLength) != 0)
                        throw new Exception("Failed to decompress");
                    outBaseFile.Write(decompressedData, 0, decompressedLength);
                    sizeLeft -= decompressedLength;
                }

                #endregion
            }

            #region Destroy ctx

            if (XCompress.LDIDestroyDecompression(ctx) != 0)
                throw new Exception("Failed to destroy decompression");
            Marshal.FreeHGlobal(allocate);

            #endregion

            #endregion

            // Move back to the begining
            outBaseFile.Seek(0, SeekOrigin.Begin);
            outBaseFile.SetLength(Header.ImageSize);

            // All done
            return true;
        }


        public List<T> GetList<T>() where T : BinaryModelBase
        {
            var id = typeof(T).GetAttribute<XexHeaderAttribute>().Id;
            var header = OptionalHeaders.Single(h => h.Id == id);
            var offset = (int)header.Data;
            var length = Binary.ReadInt(offset, EndianType.BigEndian);
            var i = offset + 4;
            var list = new List<T>();
            var offsetTable = ModelFactory.GetOffsetTable(typeof (T));
            var last = offset + 4 + length;
            while (i + offsetTable.Size < last)
            {
                list.Add(ModelFactory.GetModel<T>(Binary, i));
                i += offsetTable.Size;
            }
            return list;
        }

        public T Get<T>() where T : BinaryModelBase
        {
            var id = typeof(T).GetAttribute<XexHeaderAttribute>().Id;
            var header = OptionalHeaders.Single(h => h.Id == id);
            var offset = (int)header.Data;
            return ModelFactory.GetModel<T>(Binary, offset);
        }

        public byte[] GetResource(string resourceName)
        {
            var ri = GetList<XexResourceInfo>().SingleOrDefault(r => r.Name == resourceName);
            if (ri == null) throw new ObjectNotFoundException("Resource not found by name: " + resourceName);
            var offset = ri.Offset - Header.LoadAddress;
            _baseFileStream.Seek(offset, SeekOrigin.Begin);
            var bytes = new byte[ri.Size];
            _baseFileStream.Read(bytes, 0, (int)ri.Size);
            return bytes;
        }

        //public byte[] Decrypt(byte[] bytes)
        //{
        //    var ci = Get<XexCompressionInfo>();
        //    using (var aes = Aes.Create())
        //    {
        //        var d = aes.CreateDecryptor();
                
        //    }
        //}
    }
}