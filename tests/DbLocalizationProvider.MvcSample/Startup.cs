using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.AdminUI;
using DbLocalizationProvider.Cache;
using DbLocalizationProvider.JsResourceHandler;
using DbLocalizationProvider.MvcSample;
using DbLocalizationProvider.MvcSample.Resources;
using DbLocalizationProvider.Queries;
using DbLocalizationProvider.Storage.SqlServer;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace DbLocalizationProvider.MvcSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var inst = LocalizationProvider.Current;

            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en");

            app.UseDbLocalizationProvider(ctx =>
            {
                ctx.EnableInvariantCultureFallback = true;
                ctx.DefaultResourceCulture = CultureInfo.InvariantCulture;
                ctx.EnableLegacyMode = () => true;
                ctx.ModelMetadataProviders.MarkRequiredFields = true;
                ctx.ModelMetadataProviders.RequiredFieldResource = () => HomePageResources.RequiredFieldIndicator;
                ctx.CustomAttributes = new[]
                {
                    new CustomAttributeDescriptor(typeof(HelpTextAttribute), false),
                    new CustomAttributeDescriptor(typeof(FancyHelpTextAttribute), false),
                    new CustomAttributeDescriptor(typeof(TableHeaderTitleAttribute))
                };

                ctx.ForeignResources.Add(typeof(ForeignResources));
                ctx.CacheManager.OnRemove += CacheManagerOnOnRemove;

                ctx.UseSqlServer(ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString);

                ctx.TypeFactory.ForQuery<AvailableLanguages.Query>().SetHandler<SampleAvailableLanguagesHandler>();
            });

            app.UseDbLocalizationProviderAdminUI("/localization-admin",
                _ =>
                {
                    _.ShowInvariantCulture = true;
                    _.HideDeleteButton = false;
                    _.DisableRemoveTranslationButton = false;
                });
            app.UseDbLocalizationProviderJsHandler();
        }

        private void CacheManagerOnOnRemove(CacheEventArgs args) { }
    }

    public class SampleAvailableLanguagesHandler : IQueryHandler<AvailableLanguages.Query, IEnumerable<CultureInfo>>
    {
        private static readonly List<CultureInfo> _cultureInfos = new List<CultureInfo>
        {
            new CultureInfo("en"), new CultureInfo("no"), new CultureInfo("lv")
        };

        public IEnumerable<CultureInfo> Execute(AvailableLanguages.Query query)
        {
            if (query.IncludeInvariant && !_cultureInfos.Contains(CultureInfo.InvariantCulture))
            {
                _cultureInfos.Insert(0, CultureInfo.InvariantCulture);
            }

            return _cultureInfos;
        }
    }
}
