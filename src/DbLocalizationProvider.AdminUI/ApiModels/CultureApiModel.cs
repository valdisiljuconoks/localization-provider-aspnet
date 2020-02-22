// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;

namespace DbLocalizationProvider.AdminUI.ApiModels
{
    public class CultureApiModel
    {
        public CultureApiModel(string code, string display)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Display = display ?? throw new ArgumentNullException(nameof(display));
        }

        public string Code { get; set; }

        public string Display { get; set; }
    }
}
