using System;
using System.Configuration;
using DbLocalizationProvider.Commands;
using DbLocalizationProvider.Queries;
using DbLocalizationProvider.Sync;

namespace DbLocalizationProvider.Storage.MsSql
{
    public static class ConfigurationContextExtensions
    {
        public static void UseEntityFramework(this ConfigurationContext context, string connectionName)
        {
            if (connectionName == null) throw new ArgumentNullException(nameof(connectionName));

            Settings.DbContextConnectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

            // overwriting default implementation
            ConfigurationContext.Current.TypeFactory.ForQuery<AvailableLanguages.Query>().SetHandler<AvailableLanguagesHandler>();

            // must have handlers
            ConfigurationContext.Current.TypeFactory.ForQuery<SyncResources.Query>().SetHandler<ResourceSynchronizer>();

            ConfigurationContext.Current.TypeFactory.ForQuery<GetAllResources.Query>().SetHandler<GetAllResourcesHandler>();
            ConfigurationContext.Current.TypeFactory.ForQuery<GetResource.Query>().SetHandler<GetResourceHandler>();

            ConfigurationContext.Current.TypeFactory.ForCommand<CreateNewResource.Command>().SetHandler<CreateNewResourceHandler>();
            ConfigurationContext.Current.TypeFactory.ForCommand<DeleteResource.Command>().SetHandler<DeleteResourceHandler>();
            ConfigurationContext.Current.TypeFactory.ForCommand<RemoveTranslation.Command>().SetHandler<RemoveTranslationHandler>();
            ConfigurationContext.Current.TypeFactory.ForCommand<CreateOrUpdateTranslation.Command>().SetHandler<CreateOrUpdateTranslationHandler>();
        }
    }
}
