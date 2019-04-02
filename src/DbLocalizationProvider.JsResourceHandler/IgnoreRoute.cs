using System.Web.Routing;

namespace DbLocalizationProvider.JsResourceHandler
{
    internal class IgnoreRoute : Route
    {
        public IgnoreRoute(string url) : base(url, new StopRoutingHandler()) { }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues) => null;
    }
}
