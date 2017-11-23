using System;
using System.Globalization;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class StringToByteArray
    {
        public static byte[] ToByteArray(this string s)
        {
            return ToByteArray(s, s.Length);
        }

        public static byte[] ToByteArray(this string s, int length) 
        {
            return ExtensionHelper.BlockCopy(s.ToCharArray(), length);
        }

        public static byte[] FromHex(this string s)
        {
            var j = s.Length/2;
            var bytes = new byte[j];
            for (var i = 0; i < s.Length; i=i+2)
            {
                var b = s.Substring(i, 2);
                j--;
                bytes[j] = byte.Parse(b, NumberStyles.HexNumber);
            }
            return bytes;
        }
    }

    public static class UIntToByteArray 
    {
        public static byte[] ToByteArray(this uint v, int length)
        {
            return ExtensionHelper.BlockCopy(BitConverter.GetBytes(v), length);
        }
    }

    public static class ByteArrayToByteArray
    {
        public static byte[] ToByteArray(this byte[] a, int length)
        {
            return ExtensionHelper.BlockCopy(a, length);
        }
    }
}