using System;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Events
{
    public class DurationEventArgs : EventArgs
    {
        public string Description { get; private set; }
        public TimeSpan Duration { get; private set; }

        public DurationEventArgs(string description, TimeSpan duration)
        {
            Description = description;
            Duration = duration;
        }
    }
}