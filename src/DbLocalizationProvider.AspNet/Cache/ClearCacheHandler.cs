// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Cache;

namespace DbLocalizationProvider.AspNet.Cache
{
    public class ClearCacheHandler : ICommandHandler<ClearCache.Command>
    {
        public void Execute(ClearCache.Command command)
        {
            var manager = ConfigurationContext.Current.CacheManager;
            foreach(var key in ConfigurationContext.Current.BaseCacheManager.KnownKeys)
            {
                var cachedKey = CacheKeyHelper.BuildKey(key);
                manager.Remove(cachedKey);
            }
        }
    }
}
