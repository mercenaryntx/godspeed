using Microsoft.Isam.Esent.Collections.Generic;
using System;
using ServiceStack;
using System.Linq;

namespace Neurotoxin.Godspeed.Core.Caching
{
	public class EsentPersistentDictionary : IDisposable
	{
		private PersistentDictionary<string, string> _persistentDictionary;

		public EsentPersistentDictionary(string path)
		{
			_persistentDictionary = new PersistentDictionary<string, string>(path);
		}

	    public string[] Keys
	    {
            get
            {
                try
                {
                    return _persistentDictionary.Keys.ToArray();
                }
                catch
                {
                    return new string[0];
                }
            }
	    }

	    public T Get<T>(string key)
		{
		    var content = _persistentDictionary[key];
            try
            {
                return content.FromJson<T>();
            }
            catch
            {
                return default(T);
            }
		}

        public T TryGet<T>(string key)
        {
            return !ContainsKey(key) ? default(T) : Get<T>(key);
        }

        public T TryGet<T>(string key, T defaultValue)
        {
            return !ContainsKey(key) ? defaultValue : Get<T>(key);
        }

	    public void Set<T>(string key, T value)
        {
            if (ContainsKey(key)) return;
            Put(key, value);
        }

	    public void Put<T>(string key, T value)
	    {
            _persistentDictionary.Add(key, value.ToJson());
            _persistentDictionary.Flush();
	    }

        public void Update<T>(string key, T newvalue)
        {
            if (_persistentDictionary.ContainsKey(key)) _persistentDictionary.Remove(key);
            Put(key, newvalue);
        }

        public void Remove(string key)
        {
            if (!_persistentDictionary.ContainsKey(key)) return;
            _persistentDictionary.Remove(key);
            _persistentDictionary.Flush();
        }

	    public bool ContainsKey(string key)
	    {
	        return Keys.Contains(key);
	    }

	    public void Dispose()
	    {
            _persistentDictionary.Dispose();
	        _persistentDictionary = null;
            GC.Collect();
	    }
	}
}