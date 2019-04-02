// Copyright (c) 2019 Valdis Iljuconoks.
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System.Threading;
using System.Web.Routing;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace DbLocalizationProvider.JsResourceHandler
{
    public static class IAppBuilderExtensions
    {
        public static IAppBuilder UseDbLocalizationProviderJsHandler(this IAppBuilder builder)
        {
            // this is required to make like this is because if you add ignore route *after* ordinal ones - this one will not be invoked at all
            // so it must appear before in the routing list (and RouteTable.Routes.IgnoreRoute() does not expose index to insert route at)
            RouteTable.Routes.Insert(0, new IgnoreRoute(Constants.IgnoreRoute));

            ConfigurationContext.Current.CacheManager.OnRemove += CacheInvalidationService.CacheManagerOnOnRemove;

            var properties = new AppProperties(builder.Properties);
            var token = properties.OnAppDisposing;

            if(token != CancellationToken.None)
            {
                token.Register(() => { ConfigurationContext.Current.CacheManager.OnRemove -= CacheInvalidationService.CacheManagerOnOnRemove; });
            }

            return builder;
        }
    }
}
