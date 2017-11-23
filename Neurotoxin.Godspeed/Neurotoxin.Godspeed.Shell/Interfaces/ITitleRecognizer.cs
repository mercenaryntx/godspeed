using System;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface ITitleRecognizer
    {
        Func<FileSystemItem, bool> RecognizeInaccessibleProfile { get; set; }

        bool MergeWithCachedEntry(FileSystemItem item);
        bool RecognizeType(FileSystemItem item);
        CacheItem RecognizeTitle(FileSystemItem item);
        bool UpdateTitle(FileSystemItem item);
        void ThrowCache(FileSystemItem item);
        byte[] GetBinaryContent(FileSystemItem item);
    }
}