namespace Neurotoxin.Godspeed.Core.Models
{
    public class BinaryLocation
    {
        public int Offset { get; set; }
        public int Length { get; set; }

        public BinaryLocation(int offset = -1, int length = -1)
        {
            Offset = offset;
            Length = length;
        }
    }
}