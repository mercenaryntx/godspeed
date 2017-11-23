using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using ServiceStack.OrmLite;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class CacheManager : ICacheManager
    {
        private bool _isPersisting;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWorkHandler _workHandler;
        private readonly IDbContext _dbContext;
        private readonly Dictionary<string, CacheItem> _inMemoryCache = new Dictionary<string, CacheItem>();
        private readonly Timer _timer;

        public CacheManager(IEventAggregator eventAggregator, IWorkHandler workHandler, IDbContext dbContext)
        {
            _eventAggregator = eventAggregator;
            _workHandler = workHandler;
            _dbContext = dbContext;
            PopulateItems();
            _timer = new Timer(PersistCacheData, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void PopulateItems()
        {
            var sw = new Stopwatch();
            sw.Start();
            using (var db = _dbContext.Open())
            {
                db.Get<CacheItem>(ci => _inMemoryCache.Add(ci.Id, ci));
            }
            sw.Stop();
            Debug.WriteLine("[CACHE] Populated in {0}. Item count: {1}", sw.Elapsed, _inMemoryCache.Count);
        } 

        private void PersistCacheData(object state)
        {
            if (_isPersisting)
            {
                ResetTimer();
                return;
            }

            _isPersisting = true;
            var dirties = _inMemoryCache.Values.Where(item => item.ItemState != ItemState.Persisted).ToList();
            if (!dirties.Any()) return;
            using (var db = _dbContext.Open(true))
            {
                foreach (var item in dirties)
                {
                    db.Persist(item);
                    //Debug.WriteLine("[{0}] {1}", i, db.GetLastSql());
                }
            }
            dirties.ForEach(item => item.Persisted());
            _isPersisting = false;
        }

        public CacheItem Get(CacheComplexKey key)
        {
            return Get(key.Key, key.Size, key.Date);
        }

        public CacheItem Get(string key)
        {
            return Get(key, null, null);
        }

        public CacheItem Get(string key, long? size, DateTime? date)
        {
            var hashKey = key.Hash();
            if (!_inMemoryCache.ContainsKey(hashKey)) return null;

            var item = _inMemoryCache[hashKey];

            if ((item.Expiration.HasValue && item.Expiration < DateTime.Now) ||
                (size.HasValue && item.Size.HasValue && item.Size.Value < size) ||
                (date.HasValue && item.Date.HasValue && (date.Value - item.Date.Value).TotalSeconds > 1))
            {
                Remove(key);
                return null;
            }

            return item;
        }

        public CacheItem Set(string key, FileSystemItem fileSystemItem)
        {
            return Set(key, fileSystemItem, null, null, null, null);
        }

        public CacheItem Set(string key, FileSystemItem fileSystemItem, DateTime? expiration)
        {
            return Set(key, fileSystemItem, expiration, null, null, null);
        }

        public CacheItem Set(string key, FileSystemItem fileSystemItem, DateTime? expiration, DateTime? date, long? size)
        {
            return Set(key, fileSystemItem, expiration, date, size, null);
        }

        public CacheItem Set(string key, FileSystemItem fileSystemItem, DateTime? expiration, DateTime? date, long? size, byte[] content)
        {
            var hashKey = key.Hash();
            var entry = new CacheItem
            {
                Id = hashKey,
                Expiration = expiration,
                Date = date,
                Size = size,
                Title = fileSystemItem.Title,
                Type = (int)fileSystemItem.Type,
                TitleType = (int)fileSystemItem.TitleType,
                ContentType = (int)fileSystemItem.ContentType,
                RecognitionState = (int)fileSystemItem.RecognitionState,
                Thumbnail = fileSystemItem.Thumbnail,
                Content = content
            };
            _inMemoryCache.Remove(hashKey);
            _inMemoryCache.Add(hashKey, entry);
            ResetTimer();
            return entry;
        }

        public void Clear()
        {
            _inMemoryCache.Clear();
            using (var db = _dbContext.Open())
            {
                db.DeleteAll<CacheItem>();    
            }
        }

        public void Remove(string key)
        {
            if (!_inMemoryCache.ContainsKey(key)) return;
            _inMemoryCache[key].MarkDeleted();
            ResetTimer();
        }

        public byte[] GetBinaryContent(string key)
        {
            using (var db = _dbContext.Open())
            {
                return db.ReadField<CacheItem, byte[]>(key.Hash(), "Content");
            }
        }

        public void UpdateTitle(string key, FileSystemItem item)
        {
            var cacheItem = _inMemoryCache[key.Hash()];
            cacheItem.Title = item.Title;
            cacheItem.Thumbnail = item.Thumbnail;
            ResetTimer();
        }

        public int EntryCount(Func<CacheItem, bool> predicate)
        {
            return _inMemoryCache.Count(kvp => predicate(kvp.Value));
        }

        public bool ContainsKey(string key)
        {
            return _inMemoryCache.ContainsKey(key.Hash());
        }

        private void ResetTimer()
        {
            lock (_timer)
            {
                _timer.Change(2000, Timeout.Infinite);
            }
        }
    }
}