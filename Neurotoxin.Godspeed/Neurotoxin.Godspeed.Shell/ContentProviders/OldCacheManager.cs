using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Models;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class OldCacheManager
    {
        private bool _cachePopupated;
        private const string KeyPrefix = "CacheEntry_";
        private readonly IEventAggregator _eventAggregator;
        private readonly IWorkHandler _workHandler;
        private readonly EsentPersistentDictionary _cacheStore = EsentPersistentDictionary.Instance;
        private readonly Dictionary<string, CacheEntry<FileSystemItem>> _inMemoryCache = new Dictionary<string,CacheEntry<FileSystemItem>>();

        public OldCacheManager(IEventAggregator eventAggregator, IWorkHandler workHandler)
        {
            _eventAggregator = eventAggregator;
            _workHandler = workHandler;

            //Read cache to memory to fasten access
            _workHandler.Run(() =>
                                 {
                                     //TODO: for cache migration

                                     //var dbFactory = new OrmLiteConnectionFactory(GetConnectionString(), SqliteDialect.Provider);
                                     //using (var db = dbFactory.Open())
                                     //{
                                     //    db.CreateTableIfNotExists<CacheItem>();
                                     //    var sw = new Stopwatch();
                                     //    sw.Start();
                                     //    var keys = _cacheStore.Keys;
                                     //    Debug.WriteLine("[DEBUG] {0} keys in cache", keys.Length);
                                     //    foreach (var key in keys.Where(k => k.StartsWith(KeyPrefix)))
                                     //    {
                                     //        var v = Get(key);
                                     //        var c = new CacheItem
                                     //                    {
                                     //                        Id = key.SubstringAfter(KeyPrefix),
                                     //                        Date = v.Date,
                                     //                        Expiration = v.Expiration,
                                     //                        Size = v.Size,
                                     //                        Title = v.Content.Title,
                                     //                        Type = v.Content.Type,
                                     //                        TitleType = v.Content.TitleType,
                                     //                        ContentType = v.Content.ContentType,
                                     //                        Thumbnail = v.Content.Thumbnail,
                                     //                        Content =
                                     //                            string.IsNullOrEmpty(v.TempFilePath)
                                     //                                ? null
                                     //                                : File.ReadAllBytes(v.TempFilePath)
                                     //                    };
                                     //        db.Insert(c);
                                     //    }
                                     //    sw.Stop();
                                     //    _cachePopupated = true;
                                     //    Debug.WriteLine("Cache fetched [{0}]", sw.Elapsed);
                                     //}
                                     //return _inMemoryCache;

                                     var sw = new Stopwatch();
                                     sw.Start();
                                     var keys = _cacheStore.Keys;
                                     Debug.WriteLine("[DEBUG] {0} keys in cache", keys.Length);
                                     foreach (var key in keys.Where(k => k.StartsWith(KeyPrefix)))
                                     {
                                         Get(key);
                                     }
                                     sw.Stop();
                                     _cachePopupated = true;
                                     Debug.WriteLine("Cache fetched [{0}]", sw.Elapsed);
                                     return _inMemoryCache;
                                 },
                                b =>
                                _eventAggregator.GetEvent<CachePopulatedEvent>().Publish(new CachePopulatedEventArgs(b, _cacheStore)));
        }

        private CacheEntry<FileSystemItem> Get(string hashKey)
        {
            lock (_inMemoryCache)
            {
                if (!_inMemoryCache.ContainsKey(hashKey))
                {
                    var entry = _cacheStore.Get<CacheEntry<FileSystemItem>>(hashKey);
                    if (entry.Content == null || string.IsNullOrEmpty(entry.Content.Title))
                    {
                        _cacheStore.Remove(hashKey);
                        _inMemoryCache.Remove(hashKey);
                        Debug.WriteLine("[!] Invalid Cache Entry Removed: " + hashKey);
                    }
                    _inMemoryCache.Add(hashKey, entry);
                }
                return _inMemoryCache[hashKey];
            }
        }

        public CacheEntry<FileSystemItem> GetEntry(CacheComplexKey key)
        {
            return GetEntry(key.Key, key.Size, key.Date);
        }

        public CacheEntry<FileSystemItem> GetEntry(string key, long? size = null, DateTime? date = null)
        {
            var hashKey = HashKey(key);
            if (!_cacheStore.ContainsKey(hashKey)) return null;

            var item = Get(hashKey);

            if ((item.Expiration.HasValue && item.Expiration < DateTime.Now) ||
                (size.HasValue && item.Size.HasValue && item.Size.Value < size) ||
                (date.HasValue && item.Date.HasValue && (date.Value - item.Date.Value).TotalSeconds > 1))
            {
                ClearCache(key);
                return null;
            }

            return item;
        }

        public CacheEntry<FileSystemItem> SaveEntry(string key, FileSystemItem content, DateTime? expiration = null, DateTime? date = null, long? size = null, string tmpPath = null)
        {
            if (content == null)
            {
                Debugger.Break();
            }
            var entry = new CacheEntry<FileSystemItem>
                {
                    Expiration = expiration,
                    Date = date,
                    Size = size,
                    Content = content,
                    TempFilePath = tmpPath
                };
            var hashKey = HashKey(key);
            _inMemoryCache.Remove(hashKey);
            _inMemoryCache.Add(hashKey, entry);
            _cacheStore.Update(hashKey, entry);
            return entry;
        }

        public void UpdateEntry(string key, FileSystemItem content)
        {
            if (content == null)
            {
                Debugger.Break();
            }
            var hashKey = HashKey(key);
            if (!_cacheStore.ContainsKey(hashKey)) 
            {
                SaveEntry(key, content);
                return;
            }
            var item = Get(hashKey);
            item.Content = content;
            item.Expiration = null;
            _inMemoryCache.Remove(hashKey);
            _inMemoryCache.Add(hashKey, item);
            _cacheStore.Update(hashKey, item);
        }

        public void ClearCache()
        {
            foreach (var key in _cacheStore.Keys.Where(key => key.StartsWith(KeyPrefix)))
            {
                RemoveCacheEntry(key);
            }
        }

        public void ClearCache(string key)
        {
            RemoveCacheEntry(HashKey(key));
        }

        public int EntryCount(Func<FileSystemItem, bool> predicate)
        {
            while (!_cachePopupated) Thread.Sleep(100);
            return _inMemoryCache.Count(kvp => predicate(kvp.Value.Content));
        }

        private void RemoveCacheEntry(string hashKey)
        {
            if (!_cacheStore.ContainsKey(hashKey)) return;
            var item = Get(hashKey);
            if (!string.IsNullOrEmpty(item.TempFilePath)) File.Delete(item.TempFilePath);
            _cacheStore.Remove(hashKey);
            _inMemoryCache.Remove(hashKey);
        }

        private static string HashKey(string s)
        {
            return KeyPrefix + s.Hash();
        }

    }
}