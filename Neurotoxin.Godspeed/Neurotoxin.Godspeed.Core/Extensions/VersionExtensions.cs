using System;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class VersionExtensions
    {
        public static uint ToUInt32(this Version version)
        {
            uint temp = 0;
            temp |= ((uint)version.Major & 0xF) << 28;
            temp |= ((uint)version.Minor & 0xF) << 24;
            temp |= ((uint)version.Build & 0xFFFF) << 8;
            temp |= (uint)version.Revision & 0xFF;
            return temp;
        }

        public static Version FromUInt32(uint value)
        {
            int v = (int)value;
            var version = new Version(v >> 28, (v >> 24) & 0xF, (v >> 8) & 0xFFFF, v & 0xFF);
            return version;
        }
    }
}