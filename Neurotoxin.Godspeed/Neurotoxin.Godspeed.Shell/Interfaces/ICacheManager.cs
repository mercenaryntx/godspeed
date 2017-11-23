using System;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface ICacheManager
    {
        CacheItem Get(CacheComplexKey key);
        CacheItem Get(string key);
        CacheItem Get(string key, long? size, DateTime? date);

        CacheItem Set(string key, FileSystemItem fileSystemItem);
        CacheItem Set(string key, FileSystemItem fileSystemItem, DateTime? expiration);
        CacheItem Set(string key, FileSystemItem fileSystemItem, DateTime? expiration, DateTime? date, long? size);
        CacheItem Set(string key, FileSystemItem fileSystemItem, DateTime? expiration, DateTime? date, long? size, byte[] content);

        void Clear();
        void Remove(string key);
        byte[] GetBinaryContent(string key);
        void UpdateTitle(string key, FileSystemItem item);

        int EntryCount(Func<CacheItem, bool> predicate);
        bool ContainsKey(string key);
    }
}