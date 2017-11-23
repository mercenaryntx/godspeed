using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
/*
 * 
 * Thanks to Anthony for most of the XEX2 code.
 * 
 * */
namespace XeXtractor
{
    public class XEX2
    {
        private readonly byte[] devkitKey = new byte[] {
                0xA8, 0xB0, 0x05, 0x12, 0xED, 0xE3, 0x63, 0x8D, 
                0xC6, 0x58, 0xB3, 0x10, 0x1F, 0x9F, 0x50, 0xD1 };
        private readonly byte[] devkitKey2 = new byte[0x10];
        private readonly byte[] retailKey = new byte[]{
                0xA2, 0x6C, 0x10, 0xF7, 0x1F, 0xD9, 0x35, 0xE9,
                0x8B, 0x99, 0x92, 0x2C, 0xE9, 0x32, 0x15, 0x72 };
        private readonly byte[] retailKey2 = new byte[] {
                0x20, 0xB1, 0x85, 0xA5, 0x9D, 0x28, 0xFD, 0xC3,
                0x40, 0x58, 0x3F, 0xBB, 0x08, 0x96, 0xBF, 0x91 };

        public enum ImageKeys
        {
            ResourceInfo = 0x000002FF,
            BaseFileFormat = 0x000003FF,
            DeltaPatchDescriptor = 0x000005FF,
            BaseReference = 0x00000405,

            OriginalBaseAddress = 0x00010001,
            EntryPoint = 0x00010100,
            ImageBaseAddress = 0x00010201,
            ImportLibraries = 0x000103FF,
            ImageChecksumTimestamp = 0x00018002,
            EnabledForCallcap = 0x00018102,
            EnabledForFastcap = 0x00018200,
            OriginalPEImageName = 0x000183FF,

            StaticLibraries = 0x000200FF,
            TLSInfo = 0x00020104,
            DefaultStackSize = 0x00020200,
            DefaultFilesystemCacheSize = 0x00020301,
            DefaultHeapSize = 0x00020401,
            PageHeapSizeAndflags = 0x00028002,

            SystemFlags = 0x00030000,

            ExecutionID = 0x00040006,
            TitleWorkspaceSize = 0x00040201,
            GameRatingsSpecified = 0x00040310,
            LANKey = 0x00040404,
            IncludesXbox360Logo = 0x000405FF,
            MultidiscMediaIDs = 0x000406FF,
            AlternateTitleIDs = 0x000407FF,
            AdditionalTitleMemory = 0x00040801,

            BoundingPath = 0x000080FF,
            DeviceId = 0x00008105,

            IncludesExportsByName = 0x00E10402
        }
        private static readonly Dictionary<ImageKeys, Type> HeaderInfo;

        public EndianIo Io { get; private set; }

        #region XexHeaderLayout

        public XexHeader Header;
        public Dictionary<ImageKeys, OptionalHeader> OptionalHeaders;
        public XexSecurityInfo SecurityInfo;
        public XexSectionTable SectionTable;

        #endregion

        #region OptionalHeaderProperties

        public bool OptionalHeaderExists(ImageKeys key)
        {
            return OptionalHeaders.ContainsKey(key);
        }
        public void RemoveOptionalHeader(ImageKeys key)
        {
            // Check if this header exists
            if (!OptionalHeaderExists(key)) return;

            // Now lets remove this header
            OptionalHeaders.Remove(key);
        }
        public void CreateOptionalHeader(ImageKeys key)
        {
            // Check if this header exists
            if (!OptionalHeaderExists(key)) return;

            // Get our optional header for this type and add it
            OptionalHeader optHeader = GetOptionalHeader(key, 0);
            OptionalHeaders.Add(key, optHeader);
        }

        public OriginalPEName PEName
        {
            get
            {
                if (!OptionalHeaderExists(ImageKeys.OriginalPEImageName))
                    return null;
                return (OriginalPEName)OptionalHeaders[ImageKeys.OriginalPEImageName];
            }
        }

        #endregion

        static XEX2()
        {
            // Create new dictionary with values
            HeaderInfo = new Dictionary<ImageKeys, Type>();
            HeaderInfo.Add(ImageKeys.ResourceInfo, typeof(XexResources));
            HeaderInfo.Add(ImageKeys.BaseFileFormat, typeof(BaseFileFormat));
            HeaderInfo.Add(ImageKeys.DeltaPatchDescriptor, typeof(DeltaPatchDescriptor));
            HeaderInfo.Add(ImageKeys.BaseReference, null);

            HeaderInfo.Add(ImageKeys.OriginalBaseAddress, typeof(BaseFileAddress));
            HeaderInfo.Add(ImageKeys.EntryPoint, typeof(BaseFileEntryPoint));
            HeaderInfo.Add(ImageKeys.ImageBaseAddress, typeof(BaseFileAddress));
            HeaderInfo.Add(ImageKeys.ImportLibraries, typeof(ImportLibraries));
            HeaderInfo.Add(ImageKeys.ImageChecksumTimestamp, typeof(BaseFileChecksumAndTimeStamp));
            HeaderInfo.Add(ImageKeys.EnabledForCallcap, null);
            HeaderInfo.Add(ImageKeys.EnabledForFastcap, null);
            HeaderInfo.Add(ImageKeys.OriginalPEImageName, typeof(OriginalPEName));

            HeaderInfo.Add(ImageKeys.StaticLibraries, typeof(StaticLibraries));
            HeaderInfo.Add(ImageKeys.TLSInfo, typeof(TLSInfo));
            HeaderInfo.Add(ImageKeys.DefaultStackSize, typeof(BaseFileDefaultStackSize));
            HeaderInfo.Add(ImageKeys.DefaultFilesystemCacheSize, null);
            HeaderInfo.Add(ImageKeys.DefaultHeapSize, null);
            HeaderInfo.Add(ImageKeys.PageHeapSizeAndflags, null);

            HeaderInfo.Add(ImageKeys.SystemFlags, typeof(SystemFlags));

            HeaderInfo.Add(ImageKeys.ExecutionID, typeof(ExecutionId));
            HeaderInfo.Add(ImageKeys.TitleWorkspaceSize, typeof(WorkspaceSize));
            HeaderInfo.Add(ImageKeys.GameRatingsSpecified, typeof(GameRatings));
            HeaderInfo.Add(ImageKeys.LANKey, typeof(LANKey));
            HeaderInfo.Add(ImageKeys.IncludesXbox360Logo, typeof(Xbox360Logo));
            HeaderInfo.Add(ImageKeys.MultidiscMediaIDs, null);
            HeaderInfo.Add(ImageKeys.AlternateTitleIDs, null);
            HeaderInfo.Add(ImageKeys.AdditionalTitleMemory, null);

            HeaderInfo.Add(ImageKeys.BoundingPath, typeof(BoundingPath));

            HeaderInfo.Add(ImageKeys.IncludesExportsByName, null);
        }
        public XEX2(byte[] data)
        {
            Io = new EndianIo(data, EndianType.BigEndian);
        }
        public XEX2(EndianIo io) { Io = io; }
        public XEX2(string filePath)
        {
            Io = new EndianIo(filePath, EndianType.BigEndian);
        }

        public void Open() { Io.Open(FileMode.Open); }
        public void Close() { Io.Close(); }

        public void Read()
        {
            // Seek to the begining
            Io.SeekTo(0);

            // Read our header
            Header.Read(Io.In);

            // Setup our optional header info
            OptionalHeaders = new Dictionary<ImageKeys, OptionalHeader>();
            for (int x = 0; x < Header.OptionalHeaderEntries; x++)
            {
                ImageKeys key = (ImageKeys)Io.In.ReadInt32();
                OptionalHeader optHeader = GetOptionalHeader(key, Io.In.ReadInt32());
                OptionalHeaders.Add(key, optHeader);
            }

            // Read our security info
            Io.SeekTo(Header.SecurityInfoOffset);
            SecurityInfo.Read(Io.In);

            // Read our section table
            SectionTable.Read(Io.In);

            // Go through and read each optional header now
            foreach (var pair in OptionalHeaders)
                pair.Value.Read(Io.In);
            
            DecryptRsa();
        }
        public void Write(Stream outStream)
        {
            EndianWriter ew = new EndianWriter(outStream, EndianType.BigEndian);

            // Calculate some stuff
            int tempInfoOffset = Header.SizeOf + (OptionalHeaders.Count * 0x08);
            if (Header.SecurityInfoOffset < tempInfoOffset)
                Header.SecurityInfoOffset = tempInfoOffset;

            // Write out our security info
            ew.SeekTo(Header.SecurityInfoOffset);
            SecurityInfo.Write(ew);

            // Write our our sections
            SectionTable.Write(ew);

            // Write out our optional headers
            foreach (var optHeader in OptionalHeaders)
                if (optHeader.Key != ImageKeys.ImportLibraries)
                    optHeader.Value.Write(ew);

            // If we have imports lets do that now
            if (OptionalHeaders.ContainsKey(ImageKeys.ImportLibraries))
            {
                int importSize = OptionalHeaders[ImageKeys.ImportLibraries].SizeOf();
                int padding = ((int)outStream.Position + importSize) % 0x1000;
                if (padding != 0) padding = 0x1000 - padding;
                ew.Write(new byte[padding]);
                OptionalHeaders[ImageKeys.ImportLibraries].Write(ew);
            }

            // Set our image offset
            Header.DataOffset = (int)outStream.Position;

            // Seek back to the optional headers
            ew.SeekTo(0);
            Header.Write(ew);
            foreach (var optHeader in OptionalHeaders)
            {
                ew.Write((int)optHeader.Value.ImageKey);
                ew.Write(optHeader.Value.Data);
            }

            // Read out our header
            byte[] buffer = new byte[Header.DataOffset];
            outStream.Seek(0, SeekOrigin.Begin);
            outStream.Read(buffer, 0, Header.DataOffset);

            // Compute hash of all the data
            SHA1 sha = SHA1.Create();
            int dataStart = Header.SecurityInfoOffset + 0x17C;
            sha.TransformBlock(buffer, dataStart,
                Header.DataOffset - dataStart, null, 0);
            byte[] digest = sha.ComputeHash(buffer, 0,
                Header.SecurityInfoOffset + 0x08);

            // Write out our hash to the security info block
            ew.SeekTo(Header.SecurityInfoOffset + 0x164);
            ew.Write(digest);

            // Go back and read the header
            /*
            outStream.Seek(Header.SecurityInfoOffset + 0x108, SeekOrigin.Begin);
            buffer = new byte[0x74];
            outStream.Read(buffer, 0, 0x74);
            digest = XeCryptRotSumSha(buffer, 0x74);
            */

            //Create a RSA verifier
            //RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //rsa.ImportParameters(GetRSAParams());
            //byte[] test = rsa.Decrypt(SecurityInfo.RsaSignature, false);
            //RSAPKCS1SignatureDeformatter signatureDeformatter = new RSAPKCS1SignatureDeformatter(RSA);
            //signatureDeformatter.SetHashAlgorithm("SHA1");

            //Verify our singature
            //byte[] signature = new byte[0x100];
            //cpyBlock(buffer, 0x36C, signature, 0, 0x100);
            //Array.Reverse(signature);
            //bool valid = signatureDeformatter.VerifySignature(digest, signature);
        }

        public void DecryptRsa()
        {

        }
        public void ExtractAllRessource()
        {
            // Make sure we have resources
         /*   if (!OptionalHeaders.ContainsKey(ImageKeys.ResourceInfo))
                return false;
            */
            // Get our resources
            XexResources resources = (XexResources)
                OptionalHeaders[ImageKeys.ResourceInfo];

            // Now find our resource index
            for (int x = 0; x < resources.Resources.Length; x++)
            {
              
                ExtractResource(x,resources.Resources[x].Name);
            }

        }
        public bool ExtractSpa()
        {
            // Make sure we have execution id
            if (!OptionalHeaders.ContainsKey(ImageKeys.ExecutionID))
                return false;

            // Get our resources
            ExecutionId executionId = (ExecutionId)
                OptionalHeaders[ImageKeys.ExecutionID];

            return ExtractResource(executionId.TitleId.ToString("X8"));
        }
        public bool ExtractSpa(Stream spaStream)
        {
            // Make sure we have execution id
            if (!OptionalHeaders.ContainsKey(ImageKeys.ExecutionID))
                return false;

            // Get our resources
            ExecutionId executionId = (ExecutionId)
                OptionalHeaders[ImageKeys.ExecutionID];

            return ExtractResource(executionId.TitleId.ToString("X8"), spaStream);
        }

        public bool ExtractResource(string resourceName)
        {
            // Make sure we have resources
            if (!OptionalHeaders.ContainsKey(ImageKeys.ResourceInfo))
                return false;

            // Get our resources
            XexResources resources = (XexResources)
                OptionalHeaders[ImageKeys.ResourceInfo];

            // Now find our resource index
            for (int x = 0; x < resources.Resources.Length; x++)
                if (resources.Resources[x].Name == resourceName)
                    return ExtractResource(x, resourceName);

            return false;
        }
        public bool ExtractResource(string resourceName, Stream resourceStream)
        {
            // Make sure we have resources
            if (!OptionalHeaders.ContainsKey(ImageKeys.ResourceInfo))
                return false;

            // Get our resources
            XexResources resources = (XexResources)
                OptionalHeaders[ImageKeys.ResourceInfo];

            // Now find our resource index
            for (int x = 0; x < resources.Resources.Length; x++)
                if (resources.Resources[x].Name == resourceName)
                    return ExtractResource(x, resourceStream);

            return false;
        }
        public bool ExtractResource(int resourceIndex, string name)
        {
            // Create a file stream
            MemoryStream ms = new MemoryStream();
           /* FileStream fs = new FileStream(filePath,
                FileMode.Create, FileAccess.Write);
            */
            // Extract our resource
            if (!ExtractResource(resourceIndex, ms))
                return false;
            FileEntry fentr = new FileEntry();
            fentr.Data = ms.ToArray();
            fentr.fileName = name + "." +FileHandler.GetFileType(fentr.Data);
            fentr.type = "Resources";
            InnerFileStructure.getInstance().AddFileEntry(fentr);
            // Close our file
            ms.Close();

            // All done
            return true;
        }
        MemoryStream ms = new MemoryStream();
        public void DecryptBaseFile()
        {
            DecryptBaseFile(ms);
        }
        public byte[] getBaseFile()
        {
            return ms.ToArray();
        }
        public bool ExtractResource(int resourceIndex, Stream resourceStream)
        {
            // Make sure we have resources
            if (!OptionalHeaders.ContainsKey(ImageKeys.ResourceInfo))
                return false;

            // Get our resources
            XexResources resources = (XexResources)
                OptionalHeaders[ImageKeys.ResourceInfo];

            // Make sure its a valid resource index
            if (resourceIndex > resources.Resources.Length) return false;

            // Now lets get the address to the resource
            XexResources.Resource resource = resources.Resources[resourceIndex];
            int resourceAddress = resource.Address - SecurityInfo.LoadAddress;

         
           

            // Seek to the resource in the base file
            ms.Seek(resourceAddress, SeekOrigin.Begin);

            // Now lets copy it to our resource stream
            byte[] buffer = new byte[resource.Size];
            ms.Read(buffer, 0, resource.Size);
            resourceStream.Write(buffer, 0, resource.Size);

            // Seek back to the begining
            resourceStream.Seek(0, SeekOrigin.Begin);

            // All done
            return true;
        }

        public void GetXexHeader(Stream outStream)
        {
            // Seek to our header
            Io.SeekTo(0);
            outStream.Write(Io.In.ReadBytes(Header.DataOffset), 0, Header.DataOffset);
            outStream.Seek(0, SeekOrigin.Begin);
        }
        public bool DecrpytBaseFile(string filePath)
        {
            // Create a file stream
            FileStream fs = new FileStream(filePath,
                FileMode.Create, FileAccess.Write);

            // Decrypt our base file
            if (!DecryptBaseFile(fs))
                return false;

            // Close our file
            fs.Close();

            // All done
            return true;
        }
        public bool DecryptBaseFile(Stream outBaseFile)
        {
            // Create a temp stream to read and write from
            Stream tempStream = new MemoryStream();

            // Get our base file format
            BaseFileFormat format = (BaseFileFormat)
                OptionalHeaders[ImageKeys.BaseFileFormat];

            // Incase its compressed
            CompressedBaseFile.CompBaseFileBlock compBlock;

            // Seek to our data
            Io.SeekTo(Header.DataOffset);

            #region Decrypt if encrypted

            if (format.EncryptionType == BaseFileFormat.EncryptionTypes.Encrypted)
            {
                #region Calculate our key

                Rijndael aes = Rijndael.Create();
                aes.Padding = PaddingMode.None;
                MemoryStream ms = new MemoryStream(SecurityInfo.AesKey);
                byte[] decryptKey = new byte[0x10];
                using (CryptoStream decrypt = new CryptoStream(ms,
                    aes.CreateDecryptor(retailKey2, new byte[0x10]), CryptoStreamMode.Read))
                    decrypt.Read(decryptKey, 0, 0x10);

                #endregion

                // Now decrypt our data
                aes = Rijndael.Create();
                aes.Padding = PaddingMode.None;
                using (CryptoStream decrypt = new CryptoStream(Io.Stream,
                    aes.CreateDecryptor(decryptKey, new byte[0x10]), CryptoStreamMode.Read))
                {
                    #region Decrypt non compressed

                    if (format.CompressionType == BaseFileFormat.CompressionTypes.NotCompressed)
                    {
                        // Loop through each section and decrypt
                        RawBaseFile rawBaseFile = format.RawFormat;
                        for (int x = 0; x < rawBaseFile.Blocks.Length; x++)
                        {
                            byte[] buffer = new byte[rawBaseFile.Blocks[x].DataSize];
                            decrypt.Read(buffer, 0, buffer.Length);
                            outBaseFile.Write(buffer, 0, buffer.Length);
                            outBaseFile.Write(new byte[rawBaseFile.Blocks[x].ZeroSize], 0,
                                     rawBaseFile.Blocks[x].ZeroSize);
                        }

                        // Set our length to image size
                        outBaseFile.Seek(0, SeekOrigin.Begin);
                        outBaseFile.SetLength(SecurityInfo.ImageSize);

                        // Return true
                        return true;
                    }

                    #endregion

                    #region Decrypt compressed
          
                    compBlock = format.CompressedFormat.Block;
                    int index = 0;
                    while (compBlock.DataSize != 0)
                    {
                        byte[] testData = new byte[compBlock.DataSize];
                        Io.Stream.Read(testData, 0, compBlock.DataSize);
                        Io.Stream.Seek(compBlock.DataSize*-1, SeekOrigin.Current);
                       
                        // Decrypt our data size
                        byte[] buffer = new byte[compBlock.DataSize];
                        decrypt.Read(buffer, 0, buffer.Length);
                     
                        // Verify our hash
                        byte[] hash = SHA1.Create().ComputeHash(buffer);
                        for (int x = 0; x < 0x14; x++)
                            if (hash[x] != compBlock.Hash[x])
                                throw new Exception("Bad hash");

                        // Write each block
                        tempStream.Write(buffer, 0, compBlock.DataSize);

                        // Get our endian reader to read the data
                        EndianReader er = new EndianReader(new MemoryStream(buffer), EndianType.BigEndian);
                        compBlock.Read(er);
                        index = index + 1;
                    }
                  

                    tempStream.Seek(0, SeekOrigin.Begin);

                    #endregion
                }
            }
            else
            {
                #region Write out non compressed

                if (format.CompressionType ==
                    BaseFileFormat.CompressionTypes.NotCompressed)
                {
                    // Loop through each section and decrypt
                    RawBaseFile rawBaseFile = format.RawFormat;
                    for (int x = 0; x < rawBaseFile.Blocks.Length; x++)
                    {
                        byte[] buffer = Io.In.ReadBytes(rawBaseFile.Blocks[x].DataSize);
                        outBaseFile.Write(buffer, 0, buffer.Length);
                        outBaseFile.Write(new byte[rawBaseFile.Blocks[x].ZeroSize], 0,
                                 rawBaseFile.Blocks[x].ZeroSize);
                    }

                    // Set our length to image size
                    outBaseFile.Seek(0, SeekOrigin.Begin);
                    outBaseFile.SetLength(SecurityInfo.ImageSize);

                    // Return true
                    return true;
                }

                #endregion

                // Since we didnt write to the temp stream 
                // lets just take it from the file
            }

            #endregion

            // Time to work with compresed
            int sizeLeft = SecurityInfo.ImageSize;
            compBlock = format.CompressedFormat.Block;

            #region Handle Delta Compresed

            if (format.CompressionType == BaseFileFormat.CompressionTypes.DeltaCompressed)
            {
                while (compBlock.DataSize != 0)
                {
                    byte[] buffer = new byte[compBlock.DataSize];
                    tempStream.Read(buffer, 0, buffer.Length);
                    outBaseFile.Write(buffer, 0, compBlock.DataSize);

                    // Get our endian reader to read the data
                    EndianReader er = new EndianReader(new MemoryStream(buffer),
                                                       EndianType.BigEndian);
                    compBlock.Read(er);
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
            lzx.WindowSize = format.CompressedFormat.CompressionWindow;
            IntPtr allocate = Marshal.AllocHGlobal(0x23200);
            if (XCompress.LDICreateDecompression(ref maxSize, ref lzx,
                0, 0, allocate, ref unknown, ref ctx) != 0)
                throw new Exception("Failed to create decompression");

            #endregion

            while (compBlock.DataSize != 0)
            {
                byte[] buffer = new byte[compBlock.DataSize];
                tempStream.Read(buffer, 0, buffer.Length);

                // Get our endian reader to read the data
                EndianReader er = new EndianReader(new MemoryStream(buffer),
                                                   EndianType.BigEndian);
                compBlock.Read(er);

                #region Decompress

                while (true)
                {
                    int compressedLen = er.ReadUInt16();
                    if (compressedLen == 0) break;
                    byte[] compressedData = er.ReadBytes(compressedLen);
                    int decompressedLength = (sizeLeft < 0x8000) ? sizeLeft : 0x8000;
                    byte[] decompressedData = new byte[decompressedLength];
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
            outBaseFile.SetLength(SecurityInfo.ImageSize);

            // All done
            return true;
        }
        public bool EncryptBaseFile(Stream outBaseFile, Stream inBaseFile)
        {


            return false;
        }

        public bool ApplyPatch(Stream outStream, XEX2 patch)
        {
            DeltaPatchDescriptor dpDescriptor = (DeltaPatchDescriptor)
                patch.OptionalHeaders[ImageKeys.DeltaPatchDescriptor];

            // Get our patch data
            MemoryStream patchStream = new MemoryStream();
            if (!patch.DecryptBaseFile(patchStream))
                return false;

            // Get our basefile header
            MemoryStream baseFileHeader = new MemoryStream();
            GetXexHeader(baseFileHeader);

            // Get our basefile data
            MemoryStream baseFile = new MemoryStream();
            if (!DecryptBaseFile(baseFile))
                return false;

            // Now lets begin to apply
            #region Create Decompression Ctx

            int maxSize = 0x8000, ctx = -1, unknown = 0;
            XCompress.LzxDecompress lzx;
            lzx.CpuType = 1; lzx.WindowSize = maxSize;
            IntPtr allocate = Marshal.AllocHGlobal(0x2B200); // IntPtr allocate2 = new IntPtr(allocate.ToInt32() + 0x20000);
            if (XCompress.LDICreateDecompression(ref maxSize, ref lzx,
                0, 0, allocate, ref unknown, ref ctx) != 0)
                throw new Exception("Failed to create decompression");

            #endregion

            // Decompress our header
            DeltaDecompress(ctx, baseFileHeader, dpDescriptor.DeltaHeaderPatchData);

            // Reset our context
            XCompress.LDIResetDecompression(ctx);

            BaseFileFormat format = (BaseFileFormat)
                patch.OptionalHeaders[ImageKeys.BaseFileFormat];
            CompressedBaseFile.CompBaseFileBlock compBlock =
                format.CompressedFormat.Block;

            // Loop and decompress the base file
            while (compBlock.DataSize > 0)
            {
                // Read this block
                EndianReader er = new EndianReader(patchStream,
                    EndianType.BigEndian);

                // Get our patch data length after the header
                int patchDataLen = compBlock.DataSize - 0x18;

                // Read the block header
                compBlock.Read(er);

                // Now delta decompress
                DeltaDecompress(ctx, baseFile, er.ReadBytes(patchDataLen));
            }

            #region Destroy ctx

            if (XCompress.LDIDestroyDecompression(ctx) != 0)
                throw new Exception("Failed to destroy decompression");
            Marshal.FreeHGlobal(allocate);

            #endregion

            // Write the patched baseFile to the out stream
            baseFile.Seek(0, SeekOrigin.Begin);
            outStream.Seek(0, SeekOrigin.Begin);
            byte[] baseData = new byte[baseFile.Length];
            baseFile.Read(baseData, 0, (int)baseFile.Length);
            outStream.Write(baseData, 0, (int)baseFile.Length);
            outStream.Seek(0, SeekOrigin.Begin);

            return true;
        }
        public void DeltaDecompress(int ldiContext, Stream baseFile, byte[] patchData)
        {
            EndianReader er = new EndianReader(
                new MemoryStream(patchData), EndianType.BigEndian);
            DeltaPatch deltaPatch = new DeltaPatch();

            // Now lets read our patch data
            int deltaBlockSize = patchData.Length;
            while (deltaBlockSize > 0x0C)
            {
                // Read our patch
                deltaPatch.Read(er);
                if (deltaPatch.UncompressedLen == 0) break;

                // Figure out what to do
                if (deltaPatch.CompressedLen == 0)
                {
                    baseFile.Seek(deltaPatch.YPos, SeekOrigin.Begin);
                    baseFile.Write(new byte[deltaPatch.UncompressedLen], 0,
                                   deltaPatch.UncompressedLen);
                }
                else if (deltaPatch.CompressedLen == 1)
                {
                    byte[] buffer = new byte[deltaPatch.UncompressedLen];
                    baseFile.Seek(deltaPatch.XPos, SeekOrigin.Begin);
                    baseFile.Read(buffer, 0, deltaPatch.UncompressedLen);
                    baseFile.Seek(deltaPatch.YPos, SeekOrigin.Begin);
                    baseFile.Write(buffer, 0, deltaPatch.UncompressedLen);
                }
                else
                {
                    // Set our window data
                    int windowSize = deltaPatch.UncompressedLen;
                    byte[] windowData = new byte[windowSize];
                    baseFile.Seek(deltaPatch.XPos, SeekOrigin.Begin);
                    baseFile.Read(windowData, 0, windowSize);
                    if (XCompress.LDISetWindowData(ldiContext, windowData, windowSize) != 0)
                        throw new Exception("Failed to set window data");

                    // decompress
                    if (deltaPatch.Decompress(ldiContext) != 0)
                        throw new Exception("Failed to decompress");

                    // Copy the data
                    baseFile.Seek(deltaPatch.YPos, SeekOrigin.Begin);
                    baseFile.Write(deltaPatch.DecompressedData,
                                   0, deltaPatch.UncompressedLen);

                    if (XCompress.LDIResetDecompression(ldiContext) != 0)
                        throw new Exception("Failed reset decompression");
                }

                deltaBlockSize -= deltaPatch.Size;
            }
        }

        public string OutputInfo()
        {
            StringBuilder sb = new StringBuilder();

            // Xex Info
            sb.AppendLine("Xex Info");
            //sb.AppendLine("Retail");
            BaseFileFormat format = (BaseFileFormat)OptionalHeaders[ImageKeys.BaseFileFormat];
            sb.AppendFormat("  {0}\r\n", format.CompressionType);
            sb.AppendFormat("  {0}\r\n", format.EncryptionType);

            int[] flags = (int[])Enum.GetValues(typeof(ModuleFlag));
            foreach (int flag in flags)
                if (((int)Header.ModuleFlags & flag) == flag)
                    sb.AppendFormat("  {0}\r\n",
                        Enum.GetName(typeof(ModuleFlag), flag));

            //sb.AppendLine("Xex Info");
            foreach (var kvp in OptionalHeaders)
                sb.AppendFormat("{0}\r\n", kvp.Value);

            sb.AppendFormat("Header Length: {0:X}\r\n", SecurityInfo.HeaderLength);
            sb.AppendFormat("Image Size: {0:X}\r\n", SecurityInfo.ImageSize);
            //sb.AppendFormat("RSA Signature: {0}\r\n", Header.SecurityInfo.RsaSignature);
            sb.AppendFormat("Length2: {0:X}\r\n", SecurityInfo.Length2);
            sb.AppendFormat("Image Flags: {0}\r\n", SecurityInfo.ImageFlags);
            sb.AppendFormat("Load Address: {0:X}\r\n", SecurityInfo.LoadAddress);
            sb.AppendFormat("Section Table Digest: {0}\r\n", BytesToString(SecurityInfo.SectionDigest));
            sb.AppendFormat("Import Table Count: {0:X}\r\n", SecurityInfo.ImportTableCount);
            sb.AppendFormat("Import Table Digest: {0}\r\n", BytesToString(SecurityInfo.ImportTableDigest));
            sb.AppendFormat("Media Id: {0}\r\n", BytesToString(SecurityInfo.Xgd2MediaId));
            sb.AppendFormat("AES Seed: {0}\r\n", BytesToString(SecurityInfo.AesKey));
            sb.AppendFormat("Export Table: {0:X}\r\n", SecurityInfo.ExportTable);
            sb.AppendFormat("Header Hash: {0}\r\n", BytesToString(SecurityInfo.HeaderDigest));
            sb.AppendFormat("Game Region: {0}\r\n", SecurityInfo.GameRegions);
            sb.AppendFormat("Media Types: {0}\r\n", SecurityInfo.AllowedMediaTypes);

            // Print resource info if there is any
            if (OptionalHeaders.ContainsKey(ImageKeys.ResourceInfo))
            {
                sb.AppendLine();
                sb.AppendLine("XEX Resources");
                XexResources resources = (XexResources)
                                         OptionalHeaders[ImageKeys.ResourceInfo];
                foreach (var res in resources.Resources)
                    sb.AppendLine(res.ToString());
            }

            return sb.ToString();
        }

        public static string BytesToString(byte[] data)
        {
            string outputString = "";
            for (int x = 0; x < data.Length; x++)
                outputString += data[x].ToString("X2");
            return outputString;
        }

        public OptionalHeader GetOptionalHeader(ImageKeys key, int data)
        {
            // Make sure our type isnt null
            if (!HeaderInfo.ContainsKey(key) || HeaderInfo[key] == null)
                return new OptionalHeader { ImageKey = key, Data = data, };

            // Create a instance of this type
            OptionalHeader optHeader = (OptionalHeader)
                Activator.CreateInstance(HeaderInfo[key], new object[] { });
            optHeader.ImageKey = key;
            optHeader.Data = data;

            // Return type
            return optHeader;
        }

        private static byte[] XeCryptRotSumSha(byte[] data, int size)
        {
            // Get our sum of the data
            byte[] output = new byte[0x20];
            XeCryptRotSum(data, 0, size / 8, output);

            // Create sha1
            SHA1 sha = SHA1.Create();

            // Update with our sum 2 times
            sha.TransformBlock(output, 0, 0x20, null, 0);
            sha.TransformBlock(output, 0, 0x20, null, 0);

            // Update with our data
            sha.TransformBlock(data, 0, size, null, 0);

            // "not" our sum
            for (int x = 0; x < 0x20; x++)
                output[x] = (byte)~output[x];

            // Update with our sum again twice
            sha.TransformBlock(output, 0, 0x20, null, 0);
            sha.TransformFinalBlock(output, 0, 0x20);

            // Return our hash
            return sha.Hash;
        }
        private static void XeCryptRotSum(byte[] data, int index, int size, byte[] output)
        {
            // Make sure size isnt 0
            if (size == 0) return;

            // Swap each int64 and load our current sum
            for (int x = 0; x < 4; x++) Array.Reverse(output, x * 8, 8);
            for (int x = 0; x < size; x++) Array.Reverse(data, x * 8, 8);
            ulong r7 = BitConverter.ToUInt64(output, 0x00),
                 r9 = BitConverter.ToUInt64(output, 0x08),
                 r6 = BitConverter.ToUInt64(output, 0x10),
                 r10 = BitConverter.ToUInt64(output, 0x18);

            // Run our algo
            for (int r5 = size; r5 > 0; r5--, index += 8)
            {
                ulong r11 = BitConverter.ToUInt64(data, index);
                ulong r8 = r11 + r9;
                r9 = (r8 < r11) ? 1UL : 0UL;

                r10 = r10 - r11;
                r7 = r9 + r7;
                r9 = (r8 << 29) | (r8 >> 35);
                r11 = (r10 > r11) ? 1UL : 0UL;

                r6 = r6 - r11;
                r10 = (r10 << 31) | (r10 >> 33);
            }

            // Copy back our new sum and swap
            Array.Copy(BitConverter.GetBytes(r7), 0, output, 0x00, 8);
            Array.Copy(BitConverter.GetBytes(r9), 0, output, 0x08, 8);
            Array.Copy(BitConverter.GetBytes(r6), 0, output, 0x10, 8);
            Array.Copy(BitConverter.GetBytes(r10), 0, output, 0x18, 8);
            for (int x = 0; x < 4; x++) Array.Reverse(output, x * 8, 8);
            for (int x = 0; x < size; x++) Array.Reverse(data, x * 8, 8);
        }

        #region Flags and Enums

        [Flags]
        public enum ModuleFlag
        {
            TitleModule = 0x00000001,
            ExportsToTitle = 0x00000002,
            SystemDebugger = 0x00000004,
            DllModule = 0x00000008,
            ModulePatch = 0x00000010,
            PatchFull = 0x00000020,
            PatchDelta = 0x00000040,
            UserMode = 0x00000080
        }

        [Flags]
        public enum AllowedMediaType
        {
            HardDisk = 0x00000001,
            DvdX2 = 0x00000002,
            DvdCd = 0x00000004,
            Dvd5 = 0x00000008,
            Dvd9 = 0x00000010,
            SystemFlash = 0x00000020,
            MemoryUnit = 0x00000080,
            MassStorageDevice = 0x00000100,
            SmbFilesystem = 0x00000200,
            DirectFromRam = 0x00000400,
            SecureVirtualOpticalDevice = 0x00001000,
            WirelessNStorageDevice = 0x00002000,
            InsecurePackage = 0x01000000,
            SaveGamePackage = 0x02000000,
            LocallySignedPackage = 0x04000000,
            LiveSignedPackage = 0x08000000,
            XboxPlatformPackage = 0x10000000
        }

        [Flags]
        public enum ImageFlag
        {
            RevocationCheckRequired = 0x01,

            ManufacturingUtility = 0x02,
            ManufacturingSupportTool = 0x04,
            ManufacturingAwareModule = ManufacturingUtility | ManufacturingSupportTool,

            Xgd2MediaOnly = 0x08,
            CardeaKey = 0x100,
            XeikaKey = 0x200,
            TitleUserMode = 0x400,
            SystemUserMode = 0x800,
            Orange0 = 0x1000,
            Orange1 = 0x2000,
            Orange2 = 0x4000,
            IptvSignupApplication = 0x10000,
            IptvTitleApplication = 0x20000,
            KeyVaultPrivilegesRequired = 0x04000000,
            OnlineActivationRequired = 0x08000000,
            PageSize4Kb = 0x10000000, // Either 4KB or 64KB
            NoGameRegion = 0x20000000,
            RevocationCheckOptional = 0x40000000
        }

        [Flags]
        public enum GameRegion : uint
        {
            NorthAmerica = 0xFF,
            Japan = 0x100,
            China = 0x200,
            RestOfAsia = 0xFC00,
            AustraliaNewZealand = 0x10000,
            RestOfEurope = 0xFE0000,
            RestOfWord = 0xFF000000,
            AllRegions = 0xFFFFFFFF
        }

        public enum SectionInfo
        {
            Code = 0x00000001, // encrypted, code memory
            Data = 0x00000002, // unencrypted read/write data memory
            ReadOnly = 0x00000003 // used for basefile headers and resources
        }

        #endregion

        #region Structures

        public struct XexVersion
        {
            public int Major;
            public int Minor;
            public int Build;
            public int Qfe;

            public XexVersion(int version)
            {
                Major = version >> 28;
                Minor = (version >> 24) & 0x0F;
                Build = (version >> 8) & 0xFFFF;
                Qfe = version & 0xFF;
            }

            public static implicit operator XexVersion(int version)
            {
                return new XexVersion(version);
            }
            public static implicit operator int(XexVersion version)
            {
                return ((version.Major & 0x0F) << 28) |
                    ((version.Minor & 0x0F) << 24) |
                    ((version.Build & 0xFFFF) << 8) |
                    (version.Qfe & 0xFF);
            }

            public override string ToString()
            {
                return string.Format("{0}.{1}.{2}.{3}",
                    Major, Minor, Build, Qfe);
            }
        }

        public struct XexHeader
        {
            public int Magic;
            public ModuleFlag ModuleFlags;
            public int DataOffset;
            public int Reserved;
            public int SecurityInfoOffset;
            public int OptionalHeaderEntries;

            public int SizeOf { get { return 0x18; } }

            public void Read(EndianReader er)
            {
                if ((Magic = er.ReadInt32()) != 0x58455832)
                    throw new Exception("Invalid XEX Header magic");
                ModuleFlags = (ModuleFlag)er.ReadInt32();
                DataOffset = er.ReadInt32();
                Reserved = er.ReadInt32();
                SecurityInfoOffset = er.ReadInt32();
                OptionalHeaderEntries = er.ReadInt32();
            }
            public void Write(EndianWriter ew)
            {
                ew.Write(Magic);
                ew.Write((int)ModuleFlags);
                ew.Write(DataOffset);
                ew.Write(Reserved);
                ew.Write(SecurityInfoOffset);
                ew.Write(OptionalHeaderEntries);
            }
        }

        public struct XexSection
        {
            public SectionInfo Info;
            public int Size;
            public byte[] Digest;

            public static int SizeOf { get { return 0x18; } }

            public void Read(EndianReader er)
            {
                int temp = er.ReadInt32();
                Info = (SectionInfo)(temp & 0x0F);
                Size = temp >> 4;
                Digest = er.ReadBytes(0x14);
            }
            public void Write(EndianWriter ew)
            {
                int temp = (Size << 4) | (int)Info;
                ew.Write(temp);
                ew.Write(Digest);
            }

            public override string ToString()
            {
                return string.Format("0x{0:X} : {1}", Size, Info);
            }
        }
        public struct XexSectionTable
        {
            public int SectionCount;
            public XexSection[] Sections;

            public int SizeOf { get { return 0x04 + (SectionCount * XexSection.SizeOf); } }

            public void Read(EndianReader er)
            {
                // Read our sections
                SectionCount = er.ReadInt32();
                Sections = new XexSection[SectionCount];
                for (int x = 0; x < SectionCount; x++)
                    Sections[x].Read(er);
            }
            public void Write(EndianWriter ew)
            {
                ew.Write(SectionCount);
                for (int x = 0; x < SectionCount; x++)
                    Sections[x].Write(ew);
            }
        }

        public struct XexSecurityInfo
        {
            public int HeaderLength;
            public int ImageSize;
            public byte[] RsaSignature;
            public int Length2;
            public ImageFlag ImageFlags;
            public int LoadAddress;
            public byte[] SectionDigest;
            public int ImportTableCount;
            public byte[] ImportTableDigest;
            public byte[] Xgd2MediaId;
            public byte[] AesKey;
            public int ExportTable;
            public byte[] HeaderDigest;
            public GameRegion GameRegions;
            public AllowedMediaType AllowedMediaTypes;

            public int SizeOf { get { return 0x180; } }

            public void Read(EndianReader er)
            {
                HeaderLength = er.ReadInt32();
                ImageSize = er.ReadInt32();
                RsaSignature = er.ReadBytes(0x100);
                Length2 = er.ReadInt32();
                ImageFlags = (ImageFlag)er.ReadInt32();
                LoadAddress = er.ReadInt32();
                SectionDigest = er.ReadBytes(0x14);
                ImportTableCount = er.ReadInt32();
                ImportTableDigest = er.ReadBytes(0x14);
                Xgd2MediaId = er.ReadBytes(0x10);
                AesKey = er.ReadBytes(0x10);
                ExportTable = er.ReadInt32();
                HeaderDigest = er.ReadBytes(0x14);
                int region = er.ReadInt32();
                long position = er.BaseStream.Position;
                GameRegions = (GameRegion)region;
                AllowedMediaTypes = (AllowedMediaType)er.ReadInt32();
            }
            public void Write(EndianWriter ew)
            {
                ew.Write(HeaderLength);
                ew.Write(ImageSize);
                ew.Write(RsaSignature);
                ew.Write(Length2);
                ew.Write((int)ImageFlags);
                ew.Write(LoadAddress);
                ew.Write(SectionDigest);
                ew.Write(ImportTableCount);
                ew.Write(ImportTableDigest);
                ew.Write(Xgd2MediaId);
                ew.Write(AesKey);
                ew.Write(ExportTable);
                ew.Write(HeaderDigest);
                ew.Write((int)GameRegions);
                ew.Write((int)AllowedMediaTypes);
            }
        }

        #endregion

        #region Optional Headers

        public class OptionalHeader
        {
            public ImageKeys ImageKey;
            public int Data;

            public virtual int SizeOf() { return 0x08; }

            public virtual void Read(EndianReader er) { }
            public virtual void Write(EndianWriter ew) { }

            public override string ToString()
            {
                return string.Format("{0} - 0x{1:X8}", ImageKey, Data);
            }
        }
        public class OriginalPEName : OptionalHeader
        {
            public string Name;

            public override int SizeOf()
            {
                if (Name.Length % 4 == 0)
                    return 0x04 + Name.Length;

                return 0x04 + Name.Length + (4 - (Name.Length % 4));
            }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                int size = er.ReadInt32();
                Name = er.ReadAsciiString(size - 4);
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(SizeOf());
                ew.WriteAsciiString(Name, Name.Length);
                ew.Write(new byte[SizeOf() - (4 + Name.Length)]);
            }
        }
        public class BaseFileAddress : OptionalHeader
        {
            public int BaseAddress;

            public override int SizeOf() { return 0x04; }

            public override void Read(EndianReader er)
            {
                BaseAddress = Data;
            }
            public override void Write(EndianWriter ew)
            {
                Data = BaseAddress;
            }
        }
        public class BaseFileEntryPoint : OptionalHeader
        {
            public int EntryPoint;

            public override int SizeOf() { return 0x04; }

            public override void Read(EndianReader er)
            {
                EntryPoint = Data;
            }
            public override void Write(EndianWriter ew)
            {
                Data = EntryPoint;
            }
        }
        public class BaseFileChecksumAndTimeStamp : OptionalHeader
        {
            public int Checksum;
            public DateTime Timestamp;

            public override int SizeOf() { return 0x08; }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                Checksum = er.ReadInt32();
                Timestamp = new DateTime(1970, 1, 1).AddSeconds(er.ReadInt32());
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(Checksum);
                TimeSpan timeSpan = Timestamp - new DateTime(1970, 1, 1);
                ew.Write((int)timeSpan.TotalSeconds);
            }
        }
        public class BaseFileDefaultStackSize : OptionalHeader
        {
            public int DefaultStackSize;

            public override int SizeOf() { return 0x04; }

            public override void Read(EndianReader er)
            {
                DefaultStackSize = Data;
            }
            public override void Write(EndianWriter ew)
            {
                Data = DefaultStackSize;
            }
        }
        public class BaseFileFormat : OptionalHeader
        {
            public enum EncryptionTypes
            {
                NotEncrypted = 0,
                Encrypted = 1
            }
            public enum CompressionTypes
            {
                NotCompressed = 1,
                Compressed = 2,
                DeltaCompressed = 3
            }

            public int InfoSize;
            public EncryptionTypes EncryptionType;
            public CompressionTypes CompressionType;

            public RawBaseFile RawFormat;
            public CompressedBaseFile CompressedFormat;

            public override int SizeOf()
            {
                if (CompressionType == CompressionTypes.NotCompressed)
                    return 0x08 + RawFormat.SizeOf;

                return 0x08 + CompressedFormat.SizeOf;
            }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                InfoSize = er.ReadInt32();
                EncryptionType = (EncryptionTypes)er.ReadInt16();
                CompressionType = (CompressionTypes)er.ReadInt16();

                // Set our format data and read it
                if (CompressionType == CompressionTypes.NotCompressed)
                {
                    RawFormat = new RawBaseFile();
                    RawFormat.Read(er, InfoSize - 0x08);
                }
                else
                {
                    CompressedFormat = new CompressedBaseFile();
                    CompressedFormat.Read(er);
                }
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                InfoSize = SizeOf();
                ew.Write(InfoSize);
                ew.Write((short)EncryptionType);
                ew.Write((short)CompressionType);
                if (CompressionType == CompressionTypes.NotCompressed)
                    RawFormat.Write(ew);
                else
                    CompressedFormat.Write(ew);
            }
        }
        public class RawBaseFile
        {
            public struct RawBaseFileBlock
            {
                public int DataSize;
                public int ZeroSize;

                public void Read(EndianReader er)
                {
                    DataSize = er.ReadInt32();
                    ZeroSize = er.ReadInt32();
                }
                public void Write(EndianWriter ew)
                {
                    ew.Write(DataSize);
                    ew.Write(ZeroSize);
                }
            }

            public RawBaseFileBlock[] Blocks;

            public int SizeOf { get { return Blocks.Length * 0x08; } }

            public void Read(EndianReader er, int size)
            {
                Blocks = new RawBaseFileBlock[size / 8];
                for (int x = 0; x < Blocks.Length; x++)
                    Blocks[x].Read(er);
            }
            public void Write(EndianWriter ew)
            {
                for (int x = 0; x < Blocks.Length; x++)
                    Blocks[x].Write(ew);
            }
        }
        public class CompressedBaseFile
        {
            public struct CompBaseFileBlock
            {
                public int DataSize;
                public byte[] Hash;

                public void Read(EndianReader er)
                {
                    DataSize = er.ReadInt32();
                    Hash = er.ReadBytes(0x14);
                }
                public void Read(byte[] data)
                {
                    DataSize = data[0] << 24 | data[1] << 16 |
                        data[2] << 8 | data[3];
                    for (int x = 4; x < 0x14 + 4; x++)
                        Hash[x - 4] = data[x];
                }
                public void Write(EndianWriter ew)
                {
                    ew.Write(DataSize);
                    ew.Write(Hash);
                }
            }

            public int CompressionWindow = 0x8000;
            public CompBaseFileBlock Block;

            public int SizeOf { get { return 0x04 + 0x18; } }

            public void Read(EndianReader er)
            {
                CompressionWindow = er.ReadInt32();
                Block.Read(er);
            }
            public void Write(EndianWriter ew)
            {
                ew.Write(CompressionWindow);
                Block.Write(ew);
            }
        }
        public class ImportLibraries : OptionalHeader
        {
            public struct ImportLib
            {
                public int Size;
                public byte[] NextImportDigest;
                public int ID;
                public XexVersion Version;
                public XexVersion VersionMin;
                public short NameIndex;
                public ushort Count;
                public int[] ImportTable;

                public int SizeOf() { return 0x28 + (0x04 * Count); }

                public void Read(EndianReader er)
                {
                    Size = er.ReadInt32();
                    NextImportDigest = er.ReadBytes(0x14);
                    ID = er.ReadInt32();
                    Version = er.ReadInt32();
                    VersionMin = er.ReadInt32();
                    NameIndex = er.ReadInt16();
                    Count = er.ReadUInt16();
                    ImportTable = new int[Count];
                    for (int x = 0; x < Count; x++)
                        ImportTable[x] = er.ReadInt32();
                }
                public void Write(EndianWriter ew)
                {
                    Size = SizeOf();
                    ew.Write(Size);
                    ew.Write(NextImportDigest);
                    ew.Write(ID);
                    ew.Write(Version);
                    ew.Write(VersionMin);
                    ew.Write(NameIndex);
                    ew.Write(Count);
                    for (int x = 0; x < Count; x++)
                        ew.Write(ImportTable[x]);
                }
            }

            public int SectionSize;
            public int HeaderSize;
            public int LibraryCount;
            public string[] LibNames;
            public ImportLib[] Libs;

            public override int SizeOf()
            {
                int size = 0x0C + SizeOfStrings();
                for (int x = 0; x < LibraryCount; x++)
                    size += Libs[x].SizeOf();

                return size;
            }
            public int SizeOfStrings()
            {
                int size = 0;
                for (int x = 0; x < LibraryCount; x++)
                {
                    size += LibNames[x].Length + 1;
                    if ((size % 4) != 0) size += 4 - (size % 4);
                }
                return size;
            }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                SectionSize = er.ReadInt32();
                HeaderSize = er.ReadInt32();
                LibraryCount = er.ReadInt32();
                LibNames = new string[LibraryCount];
                byte[] libNames = er.ReadBytes(HeaderSize);
                EndianReader ler = new EndianReader(new MemoryStream(libNames), EndianType.BigEndian);
                for (int x = 0; x < LibraryCount; x++)
                {
                    LibNames[x] = ler.ReadNullTerminatedString();
                    int pad = (int)ler.BaseStream.Position % 4;
                    if (pad != 0)
                        ler.BaseStream.Seek(4 - pad, SeekOrigin.Current);
                }

                Libs = new ImportLib[LibraryCount];
                for (int x = 0; x < LibraryCount; x++)
                    Libs[x].Read(er);
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                SectionSize = SizeOf();
                HeaderSize = SizeOfStrings();

                ew.Write(SectionSize);
                ew.Write(HeaderSize);
                ew.Write(LibraryCount);
                for (int x = 0; x < LibraryCount; x++)
                {
                    int size = LibNames[x].Length + 1;
                    ew.WriteNullTermString(LibNames[x]);
                    if ((size % 4) != 0) ew.Write(new byte[4 - (size % 4)]);
                }

                //ew.Write(new byte[HeaderSize]);
                for (int x = 0; x < LibraryCount; x++)
                    Libs[x].Write(ew);
            }
        }
        public class StaticLibraries : OptionalHeader
        {
            public struct Library
            {
                public enum ApprovalTypes
                {
                    Unapproved = 0,
                    PossibleApproved,
                    Approved,
                    Expired
                }

                public string Name;
                public short VersionMajor;
                public short VersionMinor;
                public short VersionBuild;
                public ApprovalTypes ApprovalType;
                public byte VersionQfe;

                public void Read(EndianReader er)
                {
                    Name = er.ReadAsciiString(8);
                    VersionMajor = er.ReadInt16();
                    VersionMinor = er.ReadInt16();
                    VersionBuild = er.ReadInt16();
                    ApprovalType = (ApprovalTypes)(er.ReadByte() >> 5);
                    VersionQfe = er.ReadByte();
                }
                public void Write(EndianWriter ew)
                {
                    ew.WriteAsciiString(Name, 8);
                    ew.Write(VersionMajor);
                    ew.Write(VersionMinor);
                    ew.Write(VersionBuild);
                    ew.Write((byte)((int)ApprovalType << 5));
                    ew.Write(VersionQfe);
                }

                public override string ToString()
                {
                    return string.Format("{0} {1}.{2}.{3}.{4} [{5}]",
                        Name, VersionMajor, VersionMinor, VersionBuild,
                        VersionQfe, ApprovalType);
                }
            }

            private int sectionSize;
            public Library[] Libraries;

            public override int SizeOf() { return 0x04 + (Libraries.Length * 0x10); }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                sectionSize = er.ReadInt32();
                Libraries = new Library[(sectionSize - 4) / 0x10];
                for (int x = 0; x < Libraries.Length; x++)
                    Libraries[x].Read(er);
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(SizeOf());
                for (int x = 0; x < Libraries.Length; x++)
                    Libraries[x].Write(ew);
            }
        }
        public class ExecutionId : OptionalHeader
        {
            public int MediaId;
            public XexVersion Version;
            public XexVersion BaseVersion;
            public int TitleId;
            public byte Platform;
            public byte ExecutableType;
            public byte DiscNumber;
            public byte NumberOfDiscs;
            public int SavegameId;

            public override int SizeOf() { return 0x18; }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                MediaId = er.ReadInt32();
                Version = er.ReadInt32();
                BaseVersion = er.ReadInt32();
                TitleId = er.ReadInt32();
                Platform = er.ReadByte();
                ExecutableType = er.ReadByte();
                DiscNumber = er.ReadByte();
                NumberOfDiscs = er.ReadByte();
                SavegameId = er.ReadInt32();
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(MediaId);
                ew.Write(Version);
                ew.Write(BaseVersion);
                ew.Write(TitleId);
                ew.Write(Platform);
                ew.Write(ExecutableType);
                ew.Write(DiscNumber);
                ew.Write(NumberOfDiscs);
                ew.Write(SavegameId);
            }
        }
        public class TLSInfo : OptionalHeader
        {
            public int NumberOfSlots;
            public int DataSize;
            public int RawDataAddress;
            public int RawDataSize;

            public override int SizeOf() { return 0x10; }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                NumberOfSlots = er.ReadInt32();
                DataSize = er.ReadInt32();
                RawDataAddress = er.ReadInt32();
                RawDataSize = er.ReadInt32();
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(NumberOfSlots);
                ew.Write(DataSize);
                ew.Write(RawDataAddress);
                ew.Write(RawDataSize);
            }
        }
        public class LANKey : OptionalHeader
        {
            public byte[] Key;

            public override int SizeOf() { return 0x10; }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                Key = er.ReadBytes(0x10);
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(Key);
            }
        }
        public class XexResources : OptionalHeader
        {
            public Resource[] Resources;
            public struct Resource
            {
                public string Name { get; set; }
                public int Address { get; set; }
                public int Size { get; set; }

                public void Read(EndianReader er)
                {
                    Name = er.ReadAsciiString(8);
                    Address = er.ReadInt32();
                    Size = er.ReadInt32();
                }
                public void Write(EndianWriter ew)
                {
                    ew.WriteAsciiString(Name, 8);
                    ew.Write(Address);
                    ew.Write(Size);
                }

                public override string ToString()
                {
                    return string.Format("{0} : 0x{1:X} - 0x{2:X}",
                        Name, Address, Size);
                }
            }

            public override int SizeOf() { return 0x04 + (Resources.Length * 0x10); }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                int length = er.ReadInt32();
                Resources = new Resource[(length - 4) / 0x10];
                for (int x = 0; x < Resources.Length; x++)
                    Resources[x].Read(er);
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(SizeOf());
                for (int x = 0; x < Resources.Length; x++)
                    Resources[x].Write(ew);
            }
        }
        public class SystemFlags : OptionalHeader
        {
            [Flags]
            public enum Privilege : uint
            {
                NoForceReboot = 0x00000001,
                ForegroundTasks = 0x00000002,
                NoOddMapping = 0x00000004,
                HandleMceInput = 0x00000008,
                RestrictHudFeatures = 0x00000010,
                HandleGamepadDisconnect = 0x00000020,
                InsecureSockets = 0x00000040,
                Xbox1XspInterop = 0x00000080,
                SetDashContext = 0x00000100,
                TitleUsesGameVoiceChannel = 0x00000200,
                TitlePal50Incompatible = 0x00000400,
                TitleInsecureUtilitydrive = 0x00000800,
                TitleXamHooks = 0x00001000,
                TitlePii = 0x00002000,
                CrossplatformSystemLink = 0x00004000,
                MultidiscSwap = 0x00008000,
                MultidiscInsecureMedia = 0x00010000,
                Ap25Media = 0x00020000,
                NoConfirmExit = 0x00040000,
                AllowBackgroundDownload = 0x00080000,
                CreatePersistableRamdrive = 0x00100000,
                InheritPersistedRamdrive = 0x00200000,
                AllowHudVibration = 0x00400000,
                TitleBothUtilityPartitions = 0x00800000,
                HandleIPTVInput = 0x01000000,
                PreferBigbuttonInput = 0x02000000,
                Reserved26 = 0x04000000,
                MultidiscCrossTitle = 0x08000000,
                TitleInstallIncompatible = 0x10000000,
                AllowAvatarGetMetadataByXUID = 0x20000000,
                AllowControllerSwapping = 0x40000000,
                DashExtensibilityModule = 0x80000000
                /* These next ones dont even fit into a DWORD?
                AllowNetworkReadCancel          = 0x100000000,
                XexUninterruptableReads         = 0x200000000,
                RequireExperienceFull           = 0x400000000,
                GamevoiceRequiredUI             = 0x800000000,
                */
            }

            public Privilege Privileges;

            public override int SizeOf() { return 0x04; }

            public override void Read(EndianReader er)
            {
                Privileges = (Privilege)Data;
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)Privileges;
            }
        }
        public class Xbox360Logo : OptionalHeader
        {
            public int SectionSize;
            public int ImageLength;
            public byte[] ImageData;

            public override int SizeOf() { return 0x08 + ImageData.Length; }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                SectionSize = er.ReadInt32();
                ImageLength = er.ReadInt32();
                ImageData = er.ReadBytes(ImageLength);
              //  System.IO.File.WriteAllBytes("c:\\logo.bin", ImageData);
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(SectionSize);
                ew.Write(ImageLength);
                ew.Write(ImageData);
            }
        }
        public class GameRatings : OptionalHeader
        {
            #region Rating Enums

            // North America
            public enum ESRB
            {
                Everyone = 0x00,
                Everyone10AndOlder = 0x02,
                Teen = 0x04,
                Mature = 0x06,
                RatingPending = 0x08,
                Unrated = 0xFF
            }

            // Pan European
            public enum PEGI
            {
                Ages3AndUp = 0x00,
                Ages4AndUp = 0x01,
                Ages5AndUp = 0x02,
                Ages6AndUp = 0x03,
                Ages7AndUp = 0x04,
                Ages8AndUp = 0x05,
                Ages9AndUp = 0x06,
                Ages10AndUp = 0x07,
                Ages11AndUp = 0x08,
                Ages12AndUp = 0x09,
                Ages13AndUp = 0x0A,
                Ages14AndUp = 0x0B,
                Ages15AndUp = 0x0C,
                Ages16AndUp = 0x0D,
                Ages18AndUp = 0x0E,
                Unrated = 0xFF
            }

            // Japan
            public enum CERO
            {
                AllAges = 0x00,
                Ages12AndUp = 0x02,
                Ages15AndUp = 0x04,
                Ages17AndUp = 0x06,
                Ages18AndUp = 0x08,
            }

            // Germany
            public enum USK
            {
                AllAges = 0x00,
                Ages6AndUp = 0x02,
                Ages12AndUp = 0x04,
                Ages16AndUp = 0x06,
                Ages18AndUp = 0x08,
                Unrated = 0xFF
            }

            // Australia & New Zealand
            public enum OFLC
            {
                AllAges = 0x00,
                Ages8AndUp = 0x02,
                Mature = 0x04,
                MatureAccompanied = 0x06,
                Unrated = 0xFF
            }

            // Korea
            public enum KMRB
            {
                Unrated = 0xFF
            }

            // Brazil
            public enum DJCTQ
            {
                Unrated = 0xFF
            }

            public enum FPB
            {
                AllAges = 0x00,
                ParentalGuidance = 0x06,
                Ages10AndUp = 0x07,
                Ages13AndUp = 0x0A,
                Ages16AndUp = 0x0D,
                Ages18AndUp = 0x0E,
                Unrated = 0xFF
            }

            #endregion

            public byte[] Ratings;

            public override int SizeOf() { return 0x40; }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                Ratings = er.ReadBytes(0x40);
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(Ratings);
            }
        }
        public class BoundingPath : OptionalHeader
        {
            public string Path;

            public override int SizeOf()
            {
                if (Path.Length % 4 == 0)
                    return 0x04 + Path.Length;

                return 0x04 + Path.Length + (4 - (Path.Length % 4));
            }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                int size = er.ReadInt32();
                Path = er.ReadAsciiString(size - 4);
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
                ew.Write(SizeOf());
                ew.WriteAsciiString(Path, Path.Length);
                ew.Write(new byte[SizeOf() - (4 + Path.Length)]);
            }
        }
        public struct DeltaPatch
        {
            public int XPos;
            public int YPos;
            public short UncompressedLen;
            public short CompressedLen;
            public byte[] CompressedData;
            public byte[] DecompressedData;

            public int Size { get { return 0x0C + CompressedLen; } }

            public void Read(EndianReader er)
            {
                XPos = er.ReadInt32();
                YPos = er.ReadInt32();
                UncompressedLen = er.ReadInt16();
                CompressedLen = er.ReadInt16();
                CompressedData = er.ReadBytes(CompressedLen);
            }

            public int Decompress()
            {
                // Create decompression ctx
                int maxSize = 0x8000, ctx = -1, unknown = 0x9800;
                XCompress.LzxDecompress lzx;
                lzx.CpuType = 1;
                lzx.WindowSize = maxSize;
                IntPtr allocate = Marshal.AllocHGlobal(0x23200);
                bool success = XCompress.LDICreateDecompression(ref maxSize, ref lzx,
                       0, 0, allocate, ref unknown, ref ctx) == 0;

                // Call our decompress
                if (success)
                    success = Decompress(ctx) == 0;

                // Destroy ctx
                if (XCompress.LDIDestroyDecompression(ctx) != 0) success = false;
                Marshal.FreeHGlobal(allocate);

                // Success
                return success ? 0 : 1;
            }

            public int Decompress(int ctx)
            {
                int compressedLen = CompressedLen;
                byte[] compressedData = CompressedData;
                int decompressedLength = UncompressedLen;
                DecompressedData = new byte[decompressedLength];
                return XCompress.LDIDecompress(ctx,
                    compressedData, compressedLen,
                    DecompressedData, ref decompressedLength);
            }
        }
        public class DeltaPatchDescriptor : OptionalHeader
        {
            public int DescriptorSize;
            public XexVersion TargetVersion;
            public XexVersion SourceVersion;
            public byte[] SourceDigest;
            public byte[] EncryptionSeed;
            public int SizeOfTargetHeaders;
            public int DeltaHeadersSourceOffset;
            public int DeltaHeadersSourceSize;
            public int DeltaHeadersTargetOffset;
            public int DeltaImageSourceOffset;
            public int DeltaImageSourceSize;
            public int DeltaImageTargetOffset;

            public byte[] DeltaHeaderPatchData;

            public override int SizeOf() { return 0x4C + DeltaHeaderPatchData.Length; }

            public override void Read(EndianReader er)
            {
                er.SeekTo(Data);
                DescriptorSize = er.ReadInt32();
                TargetVersion = er.ReadInt32();
                SourceVersion = er.ReadInt32();
                SourceDigest = er.ReadBytes(0x14);
                EncryptionSeed = er.ReadBytes(0x10);
                SizeOfTargetHeaders = er.ReadInt32();
                DeltaHeadersSourceOffset = er.ReadInt32();
                DeltaHeadersSourceSize = er.ReadInt32();
                DeltaHeadersTargetOffset = er.ReadInt32();
                DeltaImageSourceOffset = er.ReadInt32();
                DeltaImageSourceSize = er.ReadInt32();
                DeltaImageTargetOffset = er.ReadInt32();
                DeltaHeaderPatchData = er.ReadBytes(Data - 0x4C);
            }
            public override void Write(EndianWriter ew)
            {
                Data = (int)ew.BaseStream.Position;
            }
        }
        public class WorkspaceSize : OptionalHeader
        {
            public int TitleWorkspaceSize;

            public override int SizeOf() { return 0x04; }

            public override void Read(EndianReader er)
            {
                TitleWorkspaceSize = Data;
            }
            public override void Write(EndianWriter ew)
            {
                Data = TitleWorkspaceSize;
            }
        }

        #endregion
    }
}
