// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Linq;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Queries;

namespace DbLocalizationProvider.AspNet.Queries
{
    public class GetAllTranslationsHandler : IQueryHandler<GetAllTranslations.Query, IEnumerable<ResourceItem>>
    {
        public IEnumerable<ResourceItem> Execute(GetAllTranslations.Query query)
        {
            var q = new GetAllResources.Query();
            var allResources = q.Execute().Where(r =>
                                                     r.ResourceKey.StartsWith(query.Key) &&
                                                     r.Translations != null && r.Translations.Any(t => t.Language == query.Language.Name)).ToList();

            if(!allResources.Any())
            {
                return Enumerable.Empty<ResourceItem>();
            }

            return allResources.Select(r => new ResourceItem(r.ResourceKey,
                                                             r.Translations.First(t => t.Language == query.Language.Name).Value,
                                                             query.Language)).ToList();
        }
    }
}
