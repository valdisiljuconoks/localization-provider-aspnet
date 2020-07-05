// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
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
            var keysToRemove = new List<string>();

            while (existingKeys.MoveNext())
            {
                var key = existingKeys.Key?.ToString();
                var existingKey = CacheKeyHelper.GetContainerName(key);
                if (existingKey != null
                    && cacheEventArgs.ResourceKey.StartsWith(existingKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                ConfigurationContext.Current.CacheManager.Remove(key);
            }
        }
    }
}
