using System;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Events
{
    public class ContentCountEventArgs : EventArgs
    {
        public int Count { get; private set; }

        public ContentCountEventArgs(int count)
        {
            Count = count;
        }
    }
}