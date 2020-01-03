// Copyright (c) 2019 Valdis Iljuconoks.
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Cache;
using DbLocalizationProvider.Queries;

namespace DbLocalizationProvider.AspNet.Queries
{
    public class GetTranslationHandler : GetTranslation.GetTranslationHandlerBase, IQueryHandler<GetTranslation.Query, string>
    {
        public string Execute(GetTranslation.Query query)
        {
            if(!ConfigurationContext.Current.EnableLocalization())
                return query.Key;

            var key = query.Key;
            var language = query.Language;
            var cacheKey = CacheKeyHelper.BuildKey(key);
            var localizationResource = ConfigurationContext.Current.CacheManager.Get(cacheKey) as LocalizationResource;

            if(localizationResource != null)
                return GetTranslationFromAvailableList(localizationResource.Translations, language, query.UseFallback)?.Value;

            LocalizationResourceTranslation localization = null;
            LocalizationResource resource;

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

                resource = new GetResourceHandler().Execute(new GetResource.Query(query.Key));
            }

            if(resource == null)
                resource = LocalizationResource.CreateNonExisting(key);
            else
                localization = GetTranslationFromAvailableList(resource.Translations, language, query.UseFallback);

            ConfigurationContext.Current.CacheManager.Insert(cacheKey, resource, true);
            return localization?.Value;
        }
    }
}
