// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Cache;
using DbLocalizationProvider.Queries;

namespace DbLocalizationProvider.AspNet.Queries
{
    public class GetTranslationHandler : IQueryHandler<GetTranslation.Query, string>
    {
        public string Execute(GetTranslation.Query query)
        {
            if(!ConfigurationContext.Current.EnableLocalization()) return query.Key;

            var key = query.Key;
            var language = query.Language;
            var cacheKey = CacheKeyHelper.BuildKey(key);
            var localizationResource = ConfigurationContext.Current.CacheManager.Get(cacheKey) as LocalizationResource;

            if(localizationResource != null)
            {
                return localizationResource.Translations.GetValueWithFallback(language, ConfigurationContext.Current.FallbackCultures);
            }

            LocalizationResource resource = null;

            try
            {
                resource = new GetResource.Query(key).Execute();
            }
            catch(KeyNotFoundException)
            {
                // this can be a case when Episerver initialization infrastructure calls localization provider way too early
                // and there is no registration for the GetResourceHandler - so we can fallback to default implementation
                // TODO: maybe we should just have default mapping (even if init has not been called)
                // (before any of the setup code in the provider is executed). this happens if you have DisplayChannels in codebase

                //resource = new GetResourceHandler().Execute(new GetResource.Query(query.Key));
            }

            string localization = null;
            if (resource == null) resource = LocalizationResource.CreateNonExisting(key);
            else localization = resource.Translations.GetValueWithFallback(language, ConfigurationContext.Current.FallbackCultures);

            ConfigurationContext.Current.CacheManager.Insert(cacheKey, resource, true);

            return localization;
        }
    }
}
