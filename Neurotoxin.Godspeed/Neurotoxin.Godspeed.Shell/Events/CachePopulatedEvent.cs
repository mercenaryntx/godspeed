using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class CachePopulatedEvent : CompositePresentationEvent<CachePopulatedEventArgs> { }

    public class CachePopulatedEventArgs
    {
        //public Dictionary<string, CacheEntry<FileSystemItem>> InMemoryCacheItems { get; private set; }
        //public EsentPersistentDictionary CacheStore { get; private set; }

        //public CachePopulatedEventArgs(Dictionary<string, CacheEntry<FileSystemItem>> inMemoryCacheItems, EsentPersistentDictionary cacheStore)
        //{
        //    InMemoryCacheItems = inMemoryCacheItems;
        //    CacheStore = cacheStore;
        //}
    }
}