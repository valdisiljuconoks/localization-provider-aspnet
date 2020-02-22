// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Web;
using DbLocalizationProvider.Cache;

namespace DbLocalizationProvider.JsResourceHandler
{
    internal class CacheInvalidationService
    {
        internal static void CacheManagerOnOnRemove(CacheEventArgs cacheEventArgs)
        {
            var existingKeys = HttpContext.Current.Cache.GetEnumerator();
            var entriesToRemove = new List<string>();

            while(existingKeys.MoveNext())
            {
                var key = existingKeys.Key?.ToString();
                var existingKey = CacheKeyHelper.GetContainerName(key);
                if(existingKey != null && cacheEventArgs.ResourceKey.StartsWith(existingKey))
                    entriesToRemove.Add(key);
            }

            foreach(var entry in entriesToRemove)
                ConfigurationContext.Current.CacheManager.Remove(entry);
        }
    }
}
