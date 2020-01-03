// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Cache;
using DbLocalizationProvider.Queries;

namespace DbLocalizationProvider.AspNet.Queries
{
    public class AvailableLanguagesHandler : IQueryHandler<AvailableLanguages.Query, IEnumerable<CultureInfo>>
    {
        public IEnumerable<CultureInfo> Execute(AvailableLanguages.Query query)
        {
            var cacheKey = CacheKeyHelper.BuildKey($"AvailableLanguages_{query.IncludeInvariant}");

            if(HttpRuntime.Cache?.Get(cacheKey) is IEnumerable<CultureInfo> cachedLanguages)
                return cachedLanguages;

            var languages = GetAvailableLanguages(query.IncludeInvariant);
            HttpRuntime.Cache?.Insert(cacheKey, languages);

            return languages;
        }

        private IEnumerable<CultureInfo> GetAvailableLanguages(bool includeInvariant)
        {
            using(var db = new LanguageEntities())
            {
                var availableLanguages = db.LocalizationResourceTranslations
                    .Select(t => t.Language)
                    .Distinct()
                    .Where(l => includeInvariant || l != CultureInfo.InvariantCulture.Name)
                    .ToList()
                    .Select(l => new CultureInfo(l)).ToList();

                return availableLanguages;
            }
        }
    }
}
