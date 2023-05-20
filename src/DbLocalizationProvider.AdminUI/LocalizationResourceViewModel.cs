// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DbLocalizationProvider.AdminUI
{
    public class LocalizationResourceViewModel
    {
        public LocalizationResourceViewModel(List<ResourceListItem> resources,
            IEnumerable<CultureInfo> languages,
            IEnumerable<string> selectedLanguages,
            int maxLength)
        {
            Resources = resources;
            Languages = languages;
            SelectedLanguages = selectedLanguages?.Select(l => new CultureInfo(l == "__invariant" ? string.Empty : l))
                                                 .Where(sl => languages.Any(al => sl.EnglishName == al.EnglishName)) ?? languages;

            Resources.ForEach(r =>
            {
                var trimmed = new string(r.Key.Take(maxLength).ToArray());
                r.DisplayKey = r.Key.Length <= maxLength ? trimmed : $"{trimmed}...";
            });
        }

        public List<ResourceListItem> Resources { get; }

        public IEnumerable<CultureInfo> Languages { get; }

        public IEnumerable<CultureInfo> SelectedLanguages { get; }

        public bool ShowMenu { get; set; }

        public bool AdminMode { get; set; }

        public IEnumerable<ResourceTreeItem> Tree { get; set; }

        public bool IsTreeView { get; set; }

        public bool IsTreeViewEnabled { get; set; }

        public bool IsTableViewEnabled { get; set; }

        public bool IsDbSearchEnabled { get; set; }

        public bool IsRemoveTranslationButtonDisabled { get; set; }

        public bool IsDeleteButtonVisible { get; set; }

        public string Query { get; set; }

        public long TotalRowCount { get; set; }
    }
}
