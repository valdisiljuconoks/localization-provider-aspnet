// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace DbLocalizationProvider.AdminUI
{
    /// <summary>
    /// Static class description no one reads..
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Use this method to map AdminUI to particular path.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="path">The path to map AdminUI.</param>
        /// <param name="setup">pass in lambda to setup and configure AdminUI</param>
        /// <param name="additionalSetup">The additional setup callback if you need to. Usually not used</param>
        /// <returns>The same app builder to support API call chaining</returns>
        public static IAppBuilder UseDbLocalizationProviderAdminUI(this IAppBuilder builder, string path, Action<UiConfigurationContext> setup = null, Action<IAppBuilder> additionalSetup = null)
        {
            builder.Map(path, b => b.MapAdminUI(setup, additionalSetup));

            return builder;
        }

        /// <summary>
        /// Maps the admin UI.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="setup">The setup.</param>
        /// <param name="additionalSetup">The additional setup.</param>
        /// <returns></returns>
        internal static IAppBuilder MapAdminUI(this IAppBuilder builder, Action<UiConfigurationContext> setup = null, Action<IAppBuilder> additionalSetup = null)
        {
            setup?.Invoke(UiConfigurationContext.Current);
            additionalSetup?.Invoke(builder);

            builder.UseFileServer(new FileServerOptions
            {
                EnableDefaultFiles = true,
                DefaultFilesOptions = {DefaultFileNames = {"index.html"}},
                FileSystem = new EmbeddedResourceFileSystem(typeof(AppBuilderExtensions).Assembly, "DbLocalizationProvider.AdminUI")
            });

            builder.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/res"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(AppBuilderExtensions).Assembly, "DbLocalizationProvider.AdminUI.ClientResources")
            });

            var config = new HttpConfiguration();

            // explicitly registering required routes in order to avoid double calls for register attribute based routes
            config.Routes.MapHttpRoute("resources-get", "api/get", new {controller = "ResourcesApi", action = "Get"});
            config.Routes.MapHttpRoute("resources-update", "api/update", new {controller = "ResourcesApi", action = "Update"});
            config.Routes.MapHttpRoute("resources-remove", "api/remove", new {controller = "ResourcesApi", action = "Remove"});

            builder.UseWebApi(config);
            builder.UseStageMarker(PipelineStage.MapHandler);

            return builder;
        }
    }
}
