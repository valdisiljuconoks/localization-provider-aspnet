// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Diagnostics;
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
                var sync = new Synchronizer();
                sync.SyncResources();
            }

            // set model metadata providers
            if (ConfigurationContext.Current.ModelMetadataProviders.ReplaceProviders)
            {

                if (ConfigurationContext.Current.ModelMetadataProviders.SetupCallback != null)
                {
                    ConfigurationContext.Current.ModelMetadataProviders.SetupCallback();
                }
                else
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
            }

            sw.Stop();
            ConfigurationContext.Current.Logger?.Debug($"DbLocalizationProvider overall initialization took: {sw.ElapsedMilliseconds}ms");

            return builder;
        }
    }
}
