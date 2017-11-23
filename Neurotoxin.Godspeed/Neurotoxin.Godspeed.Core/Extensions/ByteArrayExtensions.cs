using System;
using System.Linq;
using System.Text;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToHex(this byte[] a)
        {
            return String.Join(String.Empty, a.Select(b => b.ToString("X2")));
        }

        public static void SwapBytes(this byte[] a, int div)
        {
            if (a.Length % div != 0)
                throw new InvalidOperationException("Array Length is not divisible by " + div);

            var temp1 = new byte[div];
            var temp2 = new byte[div];

            for (var i = 0; i < a.Length / 2; i += div)
            {
                Buffer.BlockCopy(a, i, temp1, 0, div);
                Buffer.BlockCopy(a, a.Length - i - div, temp2, 0, div);

                Buffer.BlockCopy(temp2, 0, a, i, div);
                Buffer.BlockCopy(temp1, 0, a, a.Length - i - div, div);
            }
        }

        public static void SwapEndian(this byte[] a, int div)
        {
            var temp = new byte[div];
            for (var i = 0; i < a.Length; i += div)
            {
                Buffer.BlockCopy(a, i, temp, 0, div);
                Array.Reverse(temp);
                Buffer.BlockCopy(temp, 0, a, i, div);
            }
        }

        public static DateTime ToDateTime(byte[] buffer)
        {
            buffer.SwapEndian(4);
            var high = BitConverter.ToUInt32(buffer, 0);
            var low = BitConverter.ToUInt32(buffer, 4);
            var time = ((long)high << 32) + low;
            return DateTime.FromFileTime(time);
        }

        public static byte[] FromDateTime(DateTime value)
        {
            var time = ((DateTime)value).ToFileTime();
            var high = BitConverter.GetBytes(time.GetHigh32Bits());
            var low = BitConverter.GetBytes(time.GetLow32Bits());
            var buffer = new byte[8];
            Buffer.BlockCopy(high, 0, buffer, 0, 4);
            Buffer.BlockCopy(low, 0, buffer, 4, 4);
            buffer.SwapEndian(4);
            return buffer;
        }

        public static Version ToVersion(byte[] buffer)
        {
            switch (buffer.Length)
            {
                case 4:
                    var v = BitConverter.ToInt32(buffer, 0);
                    //TODO: Check why it used v >> 28 and v >> 24 (STFS?)
                    return new Version(v >> 24, (v >> 16) & 0xF, (v >> 8) & 0xFFFF, v & 0xFF);
                case 8:
                    var v1 = BitConverter.ToInt16(buffer, 6);
                    var v2 = BitConverter.ToInt16(buffer, 4);
                    var v3 = BitConverter.ToInt16(buffer, 2);
                    var v4 = BitConverter.ToInt16(buffer, 0);
                    return new Version(v1, v2, v3, v4);
                default:
                    throw new NotSupportedException("Invalid buffer size: " + buffer.Length);
            }
        }

        public static byte[] FromVersion(Version version)
        {
            uint temp = 0;
            temp |= ((uint)version.Major & 0xF) << 28;
            temp |= ((uint)version.Minor & 0xF) << 24;
            temp |= ((uint)version.Build & 0xFFFF) << 8;
            temp |= (uint)version.Revision & 0xFF;
            return BitConverter.GetBytes(temp);
        }

        public static string ToTrimmedString(byte[] buffer, Encoding encoding)
        {
            var encoded = encoding.GetChars(buffer);
            var i = 0;
            while (i < encoded.Length && encoded[i] != 0) i++;
            return encoding.GetString(encoding.GetBytes(encoded.Take(i).ToArray()));
        }

        public static bool EqualsWith(this byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            return !a.Where((t, i) => t != b[i]).Any();
        }
    }
}