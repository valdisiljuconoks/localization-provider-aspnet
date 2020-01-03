// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Globalization;

namespace DbLocalizationProvider.AdminUI.ApiModels
{
    public class ResourceItemApiModel
    {
        public ResourceItemApiModel(string key, string value, string sourceCulture)
        {
            Key = key;
            Value = value;

            var culture = new CultureInfo(sourceCulture);
            SourceCulture = new CultureApiModel(culture.Name, culture.EnglishName);
        }

        public string Key { get; }

        public string Value { get; }

        public CultureApiModel SourceCulture { get; set; }
    }
}
