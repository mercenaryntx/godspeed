using System;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    internal static class ExtensionHelper
    {
        public static byte[] BlockCopy(Array src, int length)
        {
            var bytes = new byte[length];
            Buffer.BlockCopy(src, 0, bytes, 0, length);
            return bytes;
        }
    }
}