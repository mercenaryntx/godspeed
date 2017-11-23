namespace Neurotoxin.Godspeed.Core.Constants
{
    public enum BlockStatus
    {
        Unallocated = 0,
        PreviouslyAllocated = 0x40,
        Allocated = 0x80,
        NewlyAllocated = 0xC0
    }
}