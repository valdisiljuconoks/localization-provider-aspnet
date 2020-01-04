// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Queries;

namespace DbLocalizationProvider.Storage.SqlServer.Handlers
{
    public class GetAllResourcesHandler : IQueryHandler<GetAllResources.Query, IEnumerable<LocalizationResource>>
    {
        public IEnumerable<LocalizationResource> Execute(GetAllResources.Query query)
        {
            using(var db = new LanguageEntities())
            {
                return db.LocalizationResources.Include(r => r.Translations).ToList();
            }
        }
    }
}
