using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Core.Security;

namespace Neurotoxin.Godspeed.Core.Io.Stfs
{
    public class Account : BinaryModelBase
    {
        [BinaryData]
        public virtual ReservedFlags ReservedFlags { get; set; }

        [BinaryData]
        public virtual uint LiveFlags { get; set; }

        [BinaryData(32, "utf-16BE", StringReadOptions.AutoTrim)]
        public virtual string GamerTag { get; set; }

        [BinaryData]
        public virtual ulong XUID { get; set; }

        [BinaryData]
        public virtual CachedUserFlags CachedUserFlags { get; set; }

        [BinaryData]
        public virtual XboxLiveServiceProvider ServiceProvider { get; set; }

        [BinaryData(4)]
        public virtual Button[] Passcode { get; set; }

        [BinaryData(20)]
        public virtual string OnlineDomain { get; set; }

        [BinaryData(24)]
        public virtual string KerberosRealm { get; set; }

        [BinaryData(16)]
        public virtual byte[] OnlineKey { get; set; }

        [BinaryData(114)]
        public virtual byte[] UserPassportMemberName { get; set; }

        [BinaryData(32)]
        public virtual byte[] UserPassportPassword { get; set; }

        [BinaryData(114)]
        public virtual byte[] OwnerPassportMemberName { get; set; }

        private static readonly byte[] RetailKey = new byte[] { 0xE1, 0xBC, 0x15, 0x9C, 0x73, 0xB1, 0xEA, 0xE9, 0xAB, 0x31, 0x70, 0xF3, 0xAD, 0x47, 0xEB, 0xF3 };
        private static readonly byte[] DevkitKey = new byte[] { 0xDA, 0xB6, 0x9A, 0xD9, 0x8E, 0x28, 0x76, 0x4F, 0x97, 0x7E, 0xE2, 0x48, 0x7E, 0x4F, 0x3F, 0x68 };
        //private byte[] CONFOUNDER = new byte[] { 0x56, 0x65, 0x6C, 0x6F, 0x63, 0x69, 0x74, 0x79 };

        public Account(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        public static Account Decrypt(Stream inputStream, ConsoleType consoleType)
        {
            var hmac = new HMACSHA1(consoleType == ConsoleType.Retail ? RetailKey : DevkitKey);
            var hash = inputStream.ReadBytes(16);
            var rc4Key = hmac.ComputeHash(hash);
            Array.Resize(ref rc4Key, 16);

            var rest = inputStream.ReadBytes(388);
            var body = RC4.Decrypt(rc4Key, rest);

            var compareBuffer = hmac.ComputeHash(body);
            if (!memcmp(hash, compareBuffer, 16))
                throw new InvalidDataException("Keys do not match");
            return ModelFactory.GetModel<Account>(body.Skip(8).ToArray());
        }

        private static bool memcmp(byte[] data1, byte[] data2, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (data1[i] != data2[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}