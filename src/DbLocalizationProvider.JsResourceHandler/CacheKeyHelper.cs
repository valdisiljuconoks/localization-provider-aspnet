// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;

namespace DbLocalizationProvider.JsResourceHandler
{
    public class CacheKeyHelper
    {
        private static readonly string _separator = "_|_";

        public static string GenerateKey(string filename, string language, bool isDebugMode, bool camelCase)
        {
            return $"{filename}{_separator}{language}__{(isDebugMode ? "debug" : "release")}__{camelCase}";
        }

        public static string GetContainerName(string key)
        {
            if(key == null)
                throw new ArgumentNullException(nameof(key));

            return !key.Contains(_separator) ? null : key.Substring(0, key.IndexOf(_separator, StringComparison.Ordinal));
        }
    }
}
