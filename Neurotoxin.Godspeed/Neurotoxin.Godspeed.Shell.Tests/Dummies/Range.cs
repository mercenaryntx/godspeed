namespace Neurotoxin.Godspeed.Shell.Tests.Dummies
{
    public struct Range
    {
        public int Min { get; private set; }
        public int Max { get; private set; }

        public Range(int min, int max) : this()
        {
            Min = min;
            Max = max;
        }

        public static implicit operator Range(int value)
        {
            return new Range(value, value);
        }
    }
}