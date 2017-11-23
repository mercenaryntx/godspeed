using System;

namespace Neurotoxin.Godspeed.Core.Io.Stfs.Events
{
    public class ContentParsedEventArgs : EventArgs
    {
        public object Content { get; private set; }

        public ContentParsedEventArgs(object content)
        {
            Content = content;
        }
    }
}