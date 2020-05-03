// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DbLocalizationProvider.AdminUI.ApiModels
{
    public class LocalizationResourceApiModel
    {
        public LocalizationResourceApiModel(ICollection<LocalizationResource> resources, IEnumerable<CultureInfo> languages)
        {
            if(resources == null) throw new ArgumentNullException(nameof(resources));
            if(languages == null) throw new ArgumentNullException(nameof(languages));

            Resources = resources.Select(r =>
            {
                return new ResourceListItemApiModel(r.ResourceKey,
                    r.Translations.Select(t => new ResourceItemApiModel(r.ResourceKey, t.Value, t.Language)).ToList(),
                    r.FromCode);
            }).ToList();

            Languages = languages.Select(l => new CultureApiModel(l.Name, l.EnglishName));

            Resources.ForEach(r =>
            {
                var trimmed = new string(r.Key.Take(UiConfigurationContext.Current.MaxResourceKeyDisplayLength).ToArray());

                r.DisplayKey = r.Key.Length <= UiConfigurationContext.Current.MaxResourceKeyDisplayLength
                    ? trimmed
                    : $"{trimmed}...";
            });
        }

        public List<ResourceListItemApiModel> Resources { get; }

        public IEnumerable<CultureApiModel> Languages { get; }

        public bool AdminMode { get; set; }

        public bool HideDeleteButton { get; set; }
    }
}
