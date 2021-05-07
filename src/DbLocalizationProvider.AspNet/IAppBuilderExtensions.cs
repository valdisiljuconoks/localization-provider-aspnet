// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Diagnostics;
using System.Globalization;
using System.Web.Mvc;
using DbLocalizationProvider.AspNet.Cache;
using DbLocalizationProvider.AspNet.Queries;
using DbLocalizationProvider.Cache;
using DbLocalizationProvider.DataAnnotations;
using DbLocalizationProvider.Queries;
using DbLocalizationProvider.Sync;
using Owin;

namespace DbLocalizationProvider
{
    /// <summary>
    /// Static class description no one reads
    /// </summary>
    public static class IAppBuilderExtensions
    {
        /// <summary>
        ///     This si the method you are likely to call if you want to use LocalizationProvider in your ASP.NET MVC application.
        /// </summary>
        /// <param name="builder">AppBuilder instance</param>
        /// <param name="setup">Your custom setup lambda</param>
        /// <returns>The same app builder instance to support chaining</returns>
        public static IAppBuilder UseDbLocalizationProvider(this IAppBuilder builder, Action<ConfigurationContext> setup = null)
        {
            var sw = new Stopwatch();
            sw.Start();

            var ctx = ConfigurationContext.Current;

            // setup default implementations
            ctx.TypeFactory.ForQuery<AvailableLanguages.Query>().SetHandler<DefaultAvailableLanguagesHandler>();
            ctx.TypeFactory.ForQuery<GetAllResources.Query>().DecorateWith<CachedGetAllResourcesHandler>();
            ctx.TypeFactory.ForQuery<GetAllTranslations.Query>().SetHandler<GetAllTranslationsHandler>();
            ctx.TypeFactory.ForQuery<DetermineDefaultCulture.Query>().SetHandler<DetermineDefaultCulture.Handler>();
            ctx.TypeFactory.ForCommand<ClearCache.Command>().SetHandler<ClearCacheHandler>();

            ctx.CacheManager = new HttpCacheManager();

            // custom callback invoke here (before rest of the config is finished)
            if (setup != null) ConfigurationContext.Setup(setup);

            // also we need to make sure that invariant culture is last in the list if fallback to invariant is true
            if (ctx.EnableInvariantCultureFallback)
            {
                foreach (var fallback in ConfigurationContext.Current.FallbackList)
                {
                    fallback.Value.Add(CultureInfo.InvariantCulture);
                }
            }

            // if we do have a before sync callback
            // hook in there and wait for the signal to continue
            if (ctx.SynchronizationCoordinator.BeforeSyncCallback != null)
            {
                ctx.SynchronizationCoordinator.BeforeSyncCallback();
            }

            // if we need to sync - then it's good time to do it now
            var sync = new Synchronizer();
            sync.SyncResources(ctx.DiscoverAndRegisterResources);

            ctx.SynchronizationCoordinator.SyncCompleted();

            if (ctx.ManualResourceProvider != null)
            {
                sync.RegisterManually(ctx.ManualResourceProvider.GetResources());
            }

            // set model metadata providers
            if (ctx.ModelMetadataProviders.ReplaceProviders)
            {

                if (ctx.ModelMetadataProviders.SetupCallback != null)
                {
                    ctx.ModelMetadataProviders.SetupCallback();
                }
                else
                {
                    // set current provider
                    if (ModelMetadataProviders.Current == null)
                    {
                        if (ctx.ModelMetadataProviders.UseCachedProviders)
                        {
                            ModelMetadataProviders.Current = new CachedLocalizedMetadataProvider();
                        }
                        else
                        {
                            ModelMetadataProviders.Current = new LocalizedMetadataProvider();
                        }
                    }
                    else
                    {
                        if (ctx.ModelMetadataProviders.UseCachedProviders)
                        {
                            ModelMetadataProviders.Current = new CompositeModelMetadataProvider<CachedLocalizedMetadataProvider>(ModelMetadataProviders.Current);
                        }
                        else
                        {
                            ModelMetadataProviders.Current = new CompositeModelMetadataProvider<LocalizedMetadataProvider>(ModelMetadataProviders.Current);
                        }
                    }

                    for (var i = 0; i < ModelValidatorProviders.Providers.Count; i++)
                    {
                        var provider = ModelValidatorProviders.Providers[i];

                        if (!(provider is DataAnnotationsModelValidatorProvider)) continue;

                        ModelValidatorProviders.Providers.RemoveAt(i);
                        ModelValidatorProviders.Providers.Insert(i, new LocalizedModelValidatorProvider());

                        break;
                    }
                }
            }

            sw.Stop();
            ctx.Logger?.Debug($"DbLocalizationProvider overall initialization took: {sw.ElapsedMilliseconds}ms");

            return builder;
        }
    }
}
