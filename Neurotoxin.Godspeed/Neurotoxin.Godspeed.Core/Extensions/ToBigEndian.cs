using System;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class ShortToBigEndian
    {
        public static byte[] ToBigEndian(this ushort v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            Array.Reverse(bytes);
            return bytes;
        }

        public static ushort ToBigEndianValue(this ushort v)
        {
            var bigEndian = v.ToBigEndian();
            return BitConverter.ToUInt16(bigEndian, 0);
        }
    }

    public static class IntToBigEndian
    {
        public static byte[] ToBigEndian(this uint v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            Array.Reverse(bytes);
            return bytes;
        }

        public static uint ToBigEndianValue(this uint v)
        {
            var bigEndian = v.ToBigEndian();
            return BitConverter.ToUInt32(bigEndian, 0);
        }
    }

}