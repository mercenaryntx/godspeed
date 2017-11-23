using System;

namespace Neurotoxin.Godspeed.Core.Net
{
    public class FtpGetListingEventArgs : EventArgs
    {
        public int Size { get; private set; }

        public FtpGetListingEventArgs(int size)
        {
            Size = size;
        }
    }
}