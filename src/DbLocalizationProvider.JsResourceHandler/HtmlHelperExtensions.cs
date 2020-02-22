// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using DbLocalizationProvider.Internal;
using ExpressionHelper = DbLocalizationProvider.Internal.ExpressionHelper;

namespace DbLocalizationProvider.JsResourceHandler
{
    /// <summary>
    /// Analyzer is happy now and soon to we will be friends again..
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Gets the translated model.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="helper">The helper.</param>
        /// <param name="containerType">Type of the container.</param>
        /// <param name="language">The language.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="debug">if set to <c>true</c> [debug].</param>
        /// <param name="camelCase">if set to <c>true</c> [camel case].</param>
        /// <returns></returns>
        public static MvcHtmlString GetTranslations<TModel>(
            this HtmlHelper<TModel> helper,
            Type containerType,
            string language = null,
            string alias = null,
            bool debug = false,
            bool camelCase = false)
        {
            return GetTranslations((HtmlHelper)helper, containerType, language, alias, debug, camelCase);
        }

        /// <summary>
        /// Gets the translated model.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="containerType">Type of the container.</param>
        /// <param name="language">The language.</param>
        /// <param name="alias">Alias to use when assigning value to `window` global object.</param>
        /// <param name="debug">if set to <c>true</c> json is humanly formatted so someone can really read it if needed.</param>
        /// <param name="camelCase">if set to <c>true</c> returned json is in camelCase.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">containerType</exception>
        public static MvcHtmlString GetTranslations(
            this HtmlHelper helper,
            Type containerType,
            string language = null,
            string alias = null,
            bool debug = false,
            bool camelCase = false)
        {
            if(containerType == null)
                throw new ArgumentNullException(nameof(containerType));

            return GenerateScriptTag(language, alias, debug, ResourceKeyBuilder.BuildResourceKey(containerType), camelCase);
        }

        /// <summary>
        /// Gets the translated model.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="model">The model.</param>
        /// <param name="language">The language.</param>
        /// <param name="alias">Alias to use when assigning value to `window` global object.</param>
        /// <param name="debug">if set to <c>true</c> json is humanly formatted so someone can really read it if needed.</param>
        /// <param name="camelCase">if set to <c>true</c> returned json is in camelCase.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">model</exception>
        public static MvcHtmlString GetTranslations(
            this HtmlHelper helper,
            Expression<Func<object>> model,
            string language = null,
            string alias = null,
            bool debug = false,
            bool camelCase = false)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            return GenerateScriptTag(language, alias, debug, ExpressionHelper.GetFullMemberName(model), camelCase);
        }

        private static MvcHtmlString GenerateScriptTag(string language, string alias, bool debug, string resourceKey, bool camelCase)
        {
            // if 1st request
            var mergeScript = string.Empty;
            if(HttpContext.Current?.Items["__DbLocalizationProvider_JsHandler_1stRequest"] == null)
            {
                HttpContext.Current?.Items.Add("__DbLocalizationProvider_JsHandler_1stRequest", false);
                mergeScript = $"<script src=\"/{Constants.PathBase}/{Constants.DeepMergeScriptName}\"></script>";
            }

            var url = $"/{Constants.PathBase}/{resourceKey.Replace("+", "---")}";
            var parameters = new Dictionary<string, string>();

            if(!string.IsNullOrEmpty(language))
                parameters.Add("lang", language);

            if(!string.IsNullOrEmpty(alias))
                parameters.Add("alias", alias);

            if(debug)
                parameters.Add("debug", "true");

            if(camelCase)
                parameters.Add("camel", "true");

            if(parameters.Any())
                url += "?" + ToQueryString(parameters);

            return new MvcHtmlString($"{mergeScript}<script src=\"{url}\"></script>");
        }

        private static string ToQueryString(Dictionary<string, string> parameters)
        {
            if(parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if(!parameters.Any())
                return string.Empty;

            return string.Join("&", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
        }
    }
}
