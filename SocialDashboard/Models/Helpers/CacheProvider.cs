using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace SocialDashboard.Models
{
    /// <summary>
    /// A cache memory management provider.
    /// </summary>
    public class CacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        /// <summary>
        /// Gets the cache item with the specified key value or obtains the item from the callback method and saves it with the given key.
        /// </summary>
        /// <param name="key">The cache dictionary key associated with the item.</param>
        /// <param name="getItemCallback">The callback method to get the item if no entry found with the given key.</param>
        /// <param name="expirationTime">The time for which the item will be held in the cache.</param>
        /// <param name="highPriority">If set to true, the item will never be erased by automatic cache clearance for memory saving, 
        /// only on excplicit Remove() call or with application restarts.</param>
        public T GetOrSet<T>(string key, Func<T> getItemCallback, TimeSpan expirationTime, bool highPriority = false) where T : class
        {
            T item = (T) Cache.Get(key);
            if (item == null)
            {
                item = getItemCallback();
                if (item != null)
                {
                    if (highPriority)
                    {
                        Cache.Add(key, item, new CacheItemPolicy()
                        {
                            AbsoluteExpiration = DateTime.Now.Add(expirationTime),
                            Priority = CacheItemPriority.NotRemovable
                        });
                    }
                    else
                    {
                        Cache.Add(key, item, new CacheItemPolicy()
                        {
                            AbsoluteExpiration = DateTime.Now.Add(expirationTime)
                        });
                    }
                }
            }
            return item;
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }
    }
}