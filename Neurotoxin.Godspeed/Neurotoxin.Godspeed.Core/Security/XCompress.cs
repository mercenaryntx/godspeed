using System;
using System.Runtime.InteropServices;

namespace Neurotoxin.Godspeed.Core.Security
{
    public static class XCompress
    {
        private readonly static bool IsMachine64Bit;

        private static bool Is64Bit
        {
            get
            {
                return XCompress.IsMachine64Bit;
            }
        }

        static XCompress()
        {
            XCompress.IsMachine64Bit = IntPtr.Size == 8;
        }

        public static int LDICreateDecompression(ref int pcbDataBlockMax, ref XCompress.LzxDecompress pvConfiguration, int pfnma, int pfnmf, IntPtr pcbSrcBufferMin, ref int unknown, ref int ldiContext)
        {
            if (XCompress.Is64Bit)
            {
                return XCompress.LDICreateDecompression64(ref pcbDataBlockMax, ref pvConfiguration, pfnma, pfnmf, pcbSrcBufferMin, ref unknown, ref ldiContext);
            }
            return XCompress.LDICreateDecompression32(ref pcbDataBlockMax, ref pvConfiguration, pfnma, pfnmf, pcbSrcBufferMin, ref unknown, ref ldiContext);
        }

        [DllImport("xcompress32.dll", CharSet = CharSet.None, EntryPoint = "LDICreateDecompression", ExactSpelling = false)]
        private static extern int LDICreateDecompression32(ref int pcbDataBlockMax, ref XCompress.LzxDecompress pvConfiguration, int pfnma, int pfnmf, IntPtr pcbSrcBufferMin, ref int unknown, ref int ldiContext);

        [DllImport("xcompress64.dll", CharSet = CharSet.None, EntryPoint = "LDICreateDecompression", ExactSpelling = false)]
        private static extern int LDICreateDecompression64(ref int pcbDataBlockMax, ref XCompress.LzxDecompress pvConfiguration, int pfnma, int pfnmf, IntPtr pcbSrcBufferMin, ref int unknown, ref int ldiContext);

        public static int LDIDecompress(int context, byte[] pbSrc, int cbSrc, byte[] pbDst, ref int pcbDecompressed)
        {
            if (XCompress.Is64Bit)
            {
                return XCompress.LDIDecompress64(context, pbSrc, cbSrc, pbDst, ref pcbDecompressed);
            }
            return XCompress.LDIDecompress32(context, pbSrc, cbSrc, pbDst, ref pcbDecompressed);
        }

        [DllImport("xcompress32.dll", CharSet = CharSet.None, EntryPoint = "LDIDecompress", ExactSpelling = false)]
        private static extern int LDIDecompress32(int context, byte[] pbSrc, int cbSrc, byte[] pbDst, ref int pcbDecompressed);

        [DllImport("xcompress64.dll", CharSet = CharSet.None, EntryPoint = "LDIDecompress", ExactSpelling = false)]
        private static extern int LDIDecompress64(int context, byte[] pbSrc, int cbSrc, byte[] pbDst, ref int pcbDecompressed);

        public static int LDIDestroyDecompression(int context)
        {
            if (XCompress.Is64Bit)
            {
                return XCompress.LDIDestroyDecompression64(context);
            }
            return XCompress.LDIDestroyDecompression32(context);
        }

        [DllImport("xcompress32.dll", CharSet = CharSet.None, EntryPoint = "LDIDestroyDecompression", ExactSpelling = false)]
        private static extern int LDIDestroyDecompression32(int context);

        [DllImport("xcompress64.dll", CharSet = CharSet.None, EntryPoint = "LDIDestroyDecompression", ExactSpelling = false)]
        private static extern int LDIDestroyDecompression64(int context);

        public static int LDIResetDecompression(int context)
        {
            if (XCompress.Is64Bit)
            {
                return XCompress.LDIResetDecompression64(context);
            }
            return XCompress.LDIResetDecompression32(context);
        }

        [DllImport("xcompress32.dll", CharSet = CharSet.None, EntryPoint = "LDIResetDecompression", ExactSpelling = false)]
        private static extern int LDIResetDecompression32(int context);

        [DllImport("xcompress64.dll", CharSet = CharSet.None, EntryPoint = "LDIResetDecompression", ExactSpelling = false)]
        private static extern int LDIResetDecompression64(int context);

        public static int LDISetWindowData(int context, byte[] window, int size)
        {
            if (XCompress.Is64Bit)
            {
                return XCompress.LDISetWindowData64(context, window, size);
            }
            return XCompress.LDISetWindowData32(context, window, size);
        }

        [DllImport("xcompress32.dll", CharSet = CharSet.None, EntryPoint = "LDISetWindowData", ExactSpelling = false)]
        private static extern int LDISetWindowData32(int context, byte[] window, int size);

        [DllImport("xcompress64.dll", CharSet = CharSet.None, EntryPoint = "LDISetWindowData", ExactSpelling = false)]
        private static extern int LDISetWindowData64(int context, byte[] window, int size);

        public struct LzxDecompress
        {
            public long WindowSize;

            public long CpuType;
        }
    }
}