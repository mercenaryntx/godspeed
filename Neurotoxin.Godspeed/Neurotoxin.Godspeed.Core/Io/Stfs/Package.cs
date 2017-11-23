using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Io.Stfs.Events;
using Neurotoxin.Godspeed.Core.Models;
using System.Linq;
using ContentType = Neurotoxin.Godspeed.Core.Constants.ContentType;

namespace Neurotoxin.Godspeed.Core.Io.Stfs
{
    public abstract class Package<T> : BinaryModelBase
    {
        #region MetaData

        [BinaryData]
        public virtual Magic Magic { get; set; }

        [BinaryData(0x228)]
        public virtual Certificate Certificate { get; set; }

        [BinaryData(16)]
        public virtual LicenseEntry[] LicenseData { get; set; }

        [BinaryData(20)]
        public virtual byte[] HeaderHash { get; set; }

        [BinaryData]
        public virtual int HeaderSize { get; set; }

        [BinaryData]
        public virtual ContentType ContentType { get; set; }

        [BinaryData]
        public virtual uint MetaDataVersion { get; set; }

        [BinaryData]
        public virtual ulong ContentSize { get; set; }

        [BinaryData(4, StringReadOptions.ID)]
        public virtual string MediaId { get; set; }

        [BinaryData(4, EndianType.LittleEndian)]
        public virtual Version Version { get; set; }

        [BinaryData(4, EndianType.LittleEndian)]
        public virtual Version BaseVersion { get; set; }

        [BinaryData(4, StringReadOptions.ID)]
        public virtual string TitleId { get; set; }

        [BinaryData(1)]
        public virtual int Platform { get; set; }

        [BinaryData(1)]
        public virtual byte ExecutableType { get; set; }

        [BinaryData(1)]
        public virtual byte DiscNumber { get; set; }

        [BinaryData(1)]
        public virtual byte DiscInSet { get; set; }

        [BinaryData]
        public virtual uint SaveGameId { get; set; }

        [BinaryData(5, StringReadOptions.ID)]
        public virtual string ConsoleId { get; set; }

        [BinaryData(8, StringReadOptions.ID)]
        public virtual string ProfileId { get; set; }

        [BinaryData(0x24)]
        public virtual T VolumeDescriptor { get; set; }

        [BinaryData]
        public virtual int DataFileCount { get; set; }

        [BinaryData]
        public virtual long DataFileCombinedSize { get; set; }

        [BinaryData]
        public virtual VolumeDescriptorType DescriptorType { get; set; }

        [BinaryData]
        public virtual uint Reserved { get; set; }

        private IMediaInfo _mediaInfo;
        [BinaryData(0x4C)]
        public virtual IMediaInfo MediaInfo
        {
            get
            {
                if (_mediaInfo == null)
                {
                    switch (ContentType)
                    {
                        case ContentType.AvatarItem:
                            _mediaInfo = ModelFactory.GetModel<AvatarItemMediaInfo>(Binary, 0x3D9);
                            break;
                        case ContentType.Video:
                            _mediaInfo = ModelFactory.GetModel<VideoMediaInfo>(Binary, 0x3D9);
                            break;
                        default:
                            throw new NotSupportedException("STFS: Not supported content type: " + ContentType);
                    }
                }
                return _mediaInfo;
            }
        }

        [BinaryData(0x14)]
        public virtual byte[] DeviceId { get; set; }

        [BinaryData(0x900, StringReadOptions.AutoTrim)]
        public virtual string DisplayName { get; set; }

        [BinaryData(0x900, StringReadOptions.AutoTrim)]
        public virtual string DisplayDescription { get; set; }

        [BinaryData(0x80, StringReadOptions.AutoTrim)]
        public virtual string PublisherName { get; set; }

        [BinaryData(0x80, StringReadOptions.AutoTrim)]
        public virtual string TitleName { get; set; }

        [BinaryData(1)]
        public virtual TransferFlags TransferFlags { get; set; }

        [BinaryData]
        public virtual int ThumbnailImageSize { get; set; }

        [BinaryData]
        public virtual int TitleThumbnailImageSize { get; set; }

        [BinaryData(0x4000)]
        public virtual byte[] ThumbnailImage { get; set; }

        [BinaryData(0x4000)]
        public virtual byte[] TitleThumbnailImage { get; set; }

        [BinaryData]
        public virtual InstallerType InstallerType { get; set; }

        #endregion

        #region Other properties

        protected bool IsModified { get; set; }
        public bool IsValid
        {
            get { return Enum.IsDefined(typeof(Magic), Magic); }
        }

        #endregion

        #region Events

        public event EventHandler<DurationEventArgs> ActionDuration;
        public event EventHandler<ContentCountEventArgs> ContentCountDetermined;
        public event EventHandler<ContentParsedEventArgs> ContentParsed;

        protected void NotifyActionDuration(string description, Action action)
        {
            var before = DateTime.Now;
            action.Invoke();
            var handler = ActionDuration;
            if (handler != null) handler.Invoke(this, new DurationEventArgs(description, DateTime.Now - before));
        }

        protected void NotifyContentCountDetermined(int count)
        {
            var handler = ContentCountDetermined;
            if (handler != null) handler.Invoke(this, new ContentCountEventArgs(count));
        }

        protected void NotifyContentParsed(object content)
        {
            var handler = ContentParsed;
            if (handler != null) handler.Invoke(this, new ContentParsedEventArgs(content));
        }

        #endregion

        #region Initialization

        public Package(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
            NotifyActionDuration("STFS package parse", Parse);
        }

        protected abstract void Parse();

        #endregion

        #region Security

        private static readonly byte[] DefaultD = new byte[]
                                                      {
                                                          0x6D, 0x4C, 0xCF, 0x3D, 0xE8, 0x65, 0x51, 0xFF, 0x2D, 0xAC,
                                                          0xC1, 0x90, 0xE7, 0x47, 0xEB, 0xC6, 0x74, 0x58, 0xD0, 0x2D,
                                                          0x19, 0x08, 0xAC, 0x79, 0xCE, 0xD0, 0x1D, 0xA3, 0x1C, 0xC3,
                                                          0x2E, 0x39, 0x8E, 0xC7, 0xEF, 0x66, 0xFA, 0xE4, 0x2F, 0x10,
                                                          0x42, 0xA8, 0x4E, 0xE7, 0xA1, 0xFD, 0xF4, 0xF0, 0xCB, 0x64,
                                                          0x67, 0xA6, 0x10, 0x4D, 0x6D, 0x3A, 0x56, 0x9D, 0x1F, 0xEC,
                                                          0x51, 0xFC, 0xC2, 0x26, 0x45, 0xC2, 0xDE, 0xF9, 0x9B, 0x4C,
                                                          0x4C, 0x93, 0x4D, 0xA8, 0x2B, 0x48, 0xAC, 0xED, 0xD7, 0xFC,
                                                          0xEA, 0xE9, 0x72, 0xFB, 0xB2, 0x39, 0x88, 0xC1, 0x07, 0x34,
                                                          0x6F, 0x2A, 0x07, 0x7E, 0x97, 0x81, 0xF5, 0x02, 0x21, 0xFA,
                                                          0xCD, 0xDD, 0x30, 0xDD, 0xE5, 0x41, 0xB3, 0x4A, 0x22, 0x73,
                                                          0x80, 0x89, 0x2B, 0x9E, 0x90, 0xAF, 0xC4, 0x0A, 0x8A, 0x50,
                                                          0x15, 0x0F, 0xBD, 0x6E, 0xD4, 0x95, 0x37, 0x79
                                                      };

        public void Resign(string kvPath = null)
        {
            var kv = !string.IsNullOrEmpty(kvPath)
                         ? (Stream) new FileStream(kvPath, FileMode.Open, FileAccess.Read)
                         : GetDefaultKeyvault();
            Resign(kv);
            kv.Close();
        }

        protected virtual void Resign(Stream kv)
        {
            ResignPackage(kv, 0x344, 0x118, 0x22C);
        }

        protected void ResignPackage(Stream kv, int headerStart, int size, int toSignLoc)
        {
            var rsaParameters = GetRSAParameters(kv);

            // read the certificate
            kv.Position = 0x9B8 + (kv.Length == 0x4000 ? 0x10 : 0);
            Certificate.PublicKeyCertificateSize = kv.ReadShort();
            kv.Read(Certificate.OwnerConsoleId, 0, 5);
            Certificate.OwnerConsolePartNumber = kv.ReadWString(0x11);
            Certificate.OwnerConsoleType = (ConsoleType)(kv.ReadUInt() & 3);
            Certificate.DateGeneration = kv.ReadWString(8);
            Certificate.PublicExponent = kv.ReadUInt();
            kv.Read(Certificate.PublicModulus, 0, 128);
            kv.Read(Certificate.CertificateSignature, 0, 256);

            ConsoleId = Certificate.OwnerConsoleId.ToHex();

            HeaderHash = HashBlock(headerStart, ((HeaderSize + 0xFFF) & 0xF000) - headerStart);

            var rsaEncryptor = new RSACryptoServiceProvider();
            var rsaSigFormat = new RSAPKCS1SignatureFormatter(rsaEncryptor);
            rsaEncryptor.ImportParameters(rsaParameters);
            rsaSigFormat.SetHashAlgorithm("SHA1");
            var signature = rsaSigFormat.CreateSignature(HashBlock(toSignLoc, size));
            Array.Reverse(signature);

            Certificate.Signature = signature;
        }

        public abstract void Rehash();

        public byte[] HashBlock(int pos, int length = 0x1000)
        {
            var sha1 = new SHA1CryptoServiceProvider();
            return sha1.ComputeHash(Binary.ReadBytes(pos, length));
        }

        private static RSAParameters GetRSAParameters(Stream kv)
        {
            var p = new RSAParameters
                        {
                            Exponent = new byte[4],
                            Modulus = new byte[0x80],
                            P = new byte[0x40],
                            Q = new byte[0x40],
                            DP = new byte[0x40],
                            DQ = new byte[0x40],
                            InverseQ = new byte[0x40]
                        };

            kv.Position = 0x28C + (kv.Length == 0x4000 ? 0x10 : 0);
            p.Exponent = kv.ReadBytes(0x4);
            kv.Position += 8;
            p.Modulus = kv.ReadBytes(0x80);
            p.P = kv.ReadBytes(0x40);
            p.Q = kv.ReadBytes(0x40);
            p.DP = kv.ReadBytes(0x40);
            p.DQ = kv.ReadBytes(0x40);
            p.InverseQ = kv.ReadBytes(0x40);

            p.Modulus.SwapBytes(8);
            p.P.SwapBytes(8);
            p.Q.SwapBytes(8);
            p.DP.SwapBytes(8);
            p.DQ.SwapBytes(8);
            p.InverseQ.SwapBytes(8);
            p.D = DefaultD;

            return p;
        }

        private static UnmanagedMemoryStream GetDefaultKeyvault()
        {
            var assembly = Assembly.GetAssembly(typeof (Package<>));
            var rStream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".g.resources");
            var resourceReader = new System.Resources.ResourceReader(rStream);
            var items = resourceReader.OfType<System.Collections.DictionaryEntry>();
            return (UnmanagedMemoryStream)items.First(x => x.Key.Equals("resources/kv_dec.bin")).Value;
        }

        #endregion

        public void Save(string path)
        {
            File.WriteAllBytes(path, Binary.ReadAll());
        }
    }
}