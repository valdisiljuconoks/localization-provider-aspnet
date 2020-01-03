// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Web.Routing;

namespace DbLocalizationProvider.JsResourceHandler
{
    internal class IgnoreRoute : Route
    {
        public IgnoreRoute(string url) : base(url, new StopRoutingHandler()) { }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues) => null;
    }
}
