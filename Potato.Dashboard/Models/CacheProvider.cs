using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace Potato.Dashboard.Models
{
    public class CacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public T GetOrSet<T>(string key, Func<T> getItemCallback) where T : class
        {
            T item = (T) Cache.Get(key);
            if (item == null)
            {
                item = getItemCallback();
                Cache.Add(key, item, DateTime.MaxValue);
            }
            return item;
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }
    }
}