// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using JsonConverter = DbLocalizationProvider.Json.JsonConverter;

namespace DbLocalizationProvider.JsResourceHandler
{
    public class HttpHandler : IHttpHandler
    {
        private readonly JsonConverter _converter;

        public HttpHandler()
        {
            _converter = new JsonConverter();
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/javascript";

            var filename = ExtractFileName(context);

            if(filename == Constants.DeepMergeScriptName)
            {
                context.Response.Write("/* https://github.com/KyleAMathews/deepmerge */ var jsResourceHandler=function(){function r(r){return!(c=r,!c||'object'!=typeof c||(t=r,n=Object.prototype.toString.call(t),'[object RegExp]'===n||'[object Date]'===n||(o=t,o.$$typeof===e)));var t,n,o,c}var e='function'==typeof Symbol&&Symbol.for?Symbol.for('react.element'):60103;function t(e,t){var n;return(!t||!1!==t.clone)&&r(e)?o((n=e,Array.isArray(n)?[]:{}),e,t):e}function n(r,e,n){return r.concat(e).map(function(r){return t(r,n)})}function o(e,c,a){var u,f,y,i,b=Array.isArray(c);return b===Array.isArray(e)?b?((a||{arrayMerge:n}).arrayMerge||n)(e,c,a):(f=c,y=a,i={},r(u=e)&&Object.keys(u).forEach(function(r){i[r]=t(u[r],y)}),Object.keys(f).forEach(function(e){r(f[e])&&u[e]?i[e]=o(u[e],f[e],y):i[e]=t(f[e],y)}),i):t(c,a)}return{deepmerge:function(r,e,t){return o(r,e,t)}}}();");
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
                return;
            }

            var debugMode = context.Request.QueryString["debug"] != null;
            var camelCase = context.Request.QueryString["camel"] != null;
            var alias = string.IsNullOrEmpty(context.Request.QueryString["alias"]) ? "jsl10n" : context.Request.QueryString["alias"];
            var languageName = string.IsNullOrEmpty(context.Request.QueryString["lang"]) ? ConfigurationContext.Current.DefaultResourceCulture.Name : context.Request.QueryString["lang"];
            var cacheKey = CacheKeyHelper.GenerateKey(filename, languageName, debugMode, camelCase);
            var cache = ConfigurationContext.Current.CacheManager;
            var windowAlias = true;

            // try to guess whether response should be made in JSON
            if(context.Request.QueryString["json"] != null && context.Request.QueryString["json"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                windowAlias = false;
            }
            else
            {
                if(context.Request.Headers.AllKeys.Contains("X-Requested-With")
                   && context.Request.Headers["X-Requested-With"].Equals("XMLHttpRequest", StringComparison.InvariantCultureIgnoreCase))
                {
                    windowAlias = false;
                }
                else if(context.Request.AcceptTypes != null && context.Request.AcceptTypes.Contains("application/json"))
                {
                    windowAlias = false;
                }
            }

            if(!(cache.Get(cacheKey) is string responseObject))
            {
                responseObject = GetJson(filename, context, languageName, debugMode, camelCase);
                cache.Insert(cacheKey, responseObject, false);
            }

            if(windowAlias)
            {
                responseObject = $"window.{alias} = jsResourceHandler.deepmerge(window.{alias} || {{}}, {responseObject})";
            }
            else
            {
                context.Response.ContentType = "application/json";
            }

            context.Response.Write(responseObject);
        }

        public bool IsReusable { get; }

        public string GetJson(string filename, HttpContext context, string languageName, bool debugMode, bool camelCase)
        {
            var settings = new JsonSerializerSettings();

            if(camelCase) settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            if(debugMode) settings.Formatting = Formatting.Indented;

            return JsonConvert.SerializeObject(_converter.GetJson(filename, languageName, camelCase), settings);
        }

        private static string ExtractFileName(HttpContext context)
        {
            var result = context.Request.Path.Replace(Constants.PathBase, string.Empty);
            result = result.StartsWith("/") ? result.TrimStart('/') : result;
            result = result.EndsWith("/") ? result.TrimEnd('/') : result;

            return result.Replace("---", "+");
        }
    }
}
