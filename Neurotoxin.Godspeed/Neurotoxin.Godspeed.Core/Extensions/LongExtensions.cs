namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class LongExtensions
    {
        public static int GetHigh32Bits(this long v)
        {
            return (int)(v >> 32);
        }

        public static int GetLow32Bits(this long v)
        {
            return (int)v;
        }
    }
}
