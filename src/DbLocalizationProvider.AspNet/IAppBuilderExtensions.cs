// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DbLocalizationProvider.AspNet.Cache;
using DbLocalizationProvider.AspNet.Queries;
using DbLocalizationProvider.Cache;
using DbLocalizationProvider.DataAnnotations;
using DbLocalizationProvider.Internal;
using DbLocalizationProvider.Queries;
using DbLocalizationProvider.Sync;
using Owin;

namespace DbLocalizationProvider
{
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
            // setup default implementations
            ConfigurationContext.Current.TypeFactory.ForQuery<AvailableLanguages.Query>().SetHandler<DefaultAvailableLanguagesHandler>();
            ConfigurationContext.Current.TypeFactory.ForQuery<GetTranslation.Query>().SetHandler<GetTranslationHandler>();
            ConfigurationContext.Current.TypeFactory.ForQuery<GetAllResources.Query>().DecorateWith<CachedGetAllResourcesHandler>();
            ConfigurationContext.Current.TypeFactory.ForQuery<GetAllTranslations.Query>().SetHandler<GetAllTranslationsHandler>();
            ConfigurationContext.Current.TypeFactory.ForQuery<DetermineDefaultCulture.Query>().SetHandler<DetermineDefaultCulture.Handler>();
            ConfigurationContext.Current.TypeFactory.ForCommand<ClearCache.Command>().SetHandler<ClearCacheHandler>();

            ConfigurationContext.Current.CacheManager = new HttpCacheManager();

            // custom callback invoke here (before rest of the config is finished)
            if (setup != null) ConfigurationContext.Setup(setup);

            // if we need to sync - then it's good time to do it now
            if (ConfigurationContext.Current.DiscoverAndRegisterResources)
            {
                var syncCommand = new SyncResources.Query();
                var syncedResources = syncCommand.Execute();

                StoreKnownResourcesAndPopulateCache(syncedResources);
            }

            // set model metadata providers
            if (ConfigurationContext.Current.ModelMetadataProviders.ReplaceProviders)
            {
                // set current provider
                if (ModelMetadataProviders.Current == null)
                {
                    if (ConfigurationContext.Current.ModelMetadataProviders.UseCachedProviders)
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
                    if (ConfigurationContext.Current.ModelMetadataProviders.UseCachedProviders)
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

            // in cases when there has been already a call to LocalizationProvider.Current (some static weird things)
            // and only then setup configuration is ran - here we need to reset instance once again with new settings
            LocalizationProvider.Initialize();

            return builder;
        }

        private static void StoreKnownResourcesAndPopulateCache(IEnumerable<LocalizationResource> syncedResources)
        {
            if (ConfigurationContext.Current.PopulateCacheOnStartup)
            {
                new ClearCache.Command().Execute();

                foreach (var resource in syncedResources)
                {
                    var key = CacheKeyHelper.BuildKey(resource.ResourceKey);
                    ConfigurationContext.Current.CacheManager.Insert(key, resource, true);
                }
            }
            else
            {
                // just store resource cache keys
                syncedResources.ForEach(r => ConfigurationContext.Current.BaseCacheManager.StoreKnownKey(r.ResourceKey));
            }
        }
    }
}
