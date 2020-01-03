// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Web;
using DbLocalizationProvider.Cache;

namespace DbLocalizationProvider.AspNet.Cache
{
    public class HttpCacheManager : ICacheManager
    {
        public void Insert(string key, object value, bool insertIntoKnownResourceKeys)
        {
            HttpRuntime.Cache?.Insert(key, value);
        }

        public object Get(string key)
        {
            return HttpRuntime.Cache?.Get(key);
        }

        public void Remove(string key)
        {
            HttpRuntime.Cache?.Remove(key);
        }

        public event CacheEventHandler OnInsert;
        public event CacheEventHandler OnRemove;
    }
}
