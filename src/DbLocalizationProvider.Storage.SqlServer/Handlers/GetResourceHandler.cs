// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Data.Entity;
using System.Linq;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Queries;

namespace DbLocalizationProvider.Storage.SqlServer.Handlers
{
    public class GetResourceHandler : IQueryHandler<GetResource.Query, LocalizationResource>
    {
        public LocalizationResource Execute(GetResource.Query query)
        {
            using(var db = new LanguageEntities())
            {
                var resource = db.LocalizationResources
                                 .Include(r => r.Translations)
                                 .FirstOrDefault(r => r.ResourceKey == query.ResourceKey);

                return resource;
            }
        }
    }
}
