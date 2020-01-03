// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

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
