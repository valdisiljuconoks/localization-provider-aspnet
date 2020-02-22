// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;

namespace DbLocalizationProvider.AdminUI.ApiModels
{
    public class ResourceListItemApiModel
    {
        public ResourceListItemApiModel(string key, ICollection<ResourceItemApiModel> translations, bool syncedFromCode)
        {
            Key = key;
            Value = translations;
            SyncedFromCode = syncedFromCode;
            AllowDelete = !syncedFromCode;
        }

        public string Key { get; }

        public ICollection<ResourceItemApiModel> Value { get; }

        public bool SyncedFromCode { get; }

        public bool AllowDelete { get; set; }

        public string DisplayKey { get; set; }
    }
}
