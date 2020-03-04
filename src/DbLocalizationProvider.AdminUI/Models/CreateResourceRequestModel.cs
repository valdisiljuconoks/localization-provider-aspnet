// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using Newtonsoft.Json;

namespace DbLocalizationProvider.AdminUI.Models
{
    public class CreateResourceRequestModel
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "translations")]
        public List<TranslationRequestModel> Translations { get; set; }
    }
}
