using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DbLocalizationProvider.Queries;
using Newtonsoft.Json.Linq;

namespace DbLocalizationProvider.AspNet.Json
{
    public class JsonConverter
    {
        public JObject GetJson(string resourceClassName)
        {
            return GetJson(resourceClassName, CultureInfo.CurrentUICulture.Name);
        }

        public JObject GetJson(string resourceClassName, string languageName)
        {
            var resources = new GetAllResources.Query().Execute();
            var filteredResources = resources.Where(r => r.ResourceKey.StartsWith(resourceClassName, StringComparison.InvariantCultureIgnoreCase)).ToList();

            // we need to process key names and supported tested classes with "+" symbols in keys -> so we replace those with dots to have proper object nesting on client side
            filteredResources.ForEach(r => r.ResourceKey = r.ResourceKey.Replace("+", "."));

            return Convert(filteredResources, languageName, ConfigurationContext.Current.EnableInvariantCultureFallback);
        }

        private JObject Convert(ICollection<LocalizationResource> resources, string language, bool invariantCultureFallback)
        {
            var result = new JObject();

            foreach (var resource in resources)
            {
                if(!resource.ResourceKey.Contains("."))
                    continue;

                var segments = resource.ResourceKey.Split(new[] { "." }, StringSplitOptions.None);
                var lastSegment = segments.Last();

                if(!resource.Translations.ExistsLanguage(language) && !invariantCultureFallback)
                    continue;

                var translation = resource.Translations.ByLanguage(language, invariantCultureFallback);

                segments.Aggregate(result,
                    (e, segment) =>
                    {
                        if(e[segment] == null)
                            e[segment] = new JObject();

                        if(segment == lastSegment)
                            e[segment] = translation;

                        return e[segment] as JObject;
                    });
            }

            return result;
        }
    }
}
