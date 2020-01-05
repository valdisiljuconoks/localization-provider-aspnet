// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Configuration;
using System.Linq;
using DbLocalizationProvider.Commands;
using DbLocalizationProvider.Queries;
using DbLocalizationProvider.Storage.SqlServer.Handlers;
using DbLocalizationProvider.Sync;

namespace DbLocalizationProvider.Storage.SqlServer
{
    public static class ConfigurationContextExtensions
    {
        public static void UseSqlServer(this ConfigurationContext context, string connectionName)
        {
            if (connectionName == null) throw new ArgumentNullException(nameof(connectionName));
            if (ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().All(s => s.Name.Equals(connectionName, StringComparison.InvariantCultureIgnoreCase))) throw new ArgumentNullException(nameof(connectionName), $"Can not find connection named `{connectionName}`");

            Settings.DbContextConnectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

            // overwriting default implementation
            ConfigurationContext.Current.TypeFactory.ForQuery<AvailableLanguages.Query>().SetHandler<AvailableLanguagesHandler>();

            // must have handlers
            ConfigurationContext.Current.TypeFactory.ForQuery<SyncResources.Query>().SetHandler<ResourceSynchronizer>();

            ConfigurationContext.Current.TypeFactory.ForQuery<GetAllResources.Query>().SetHandler<GetAllResourcesHandler>();
            ConfigurationContext.Current.TypeFactory.ForQuery<GetResource.Query>().SetHandler<GetResourceHandler>();

            ConfigurationContext.Current.TypeFactory.ForCommand<CreateNewResources.Command>().SetHandler<CreateNewResourcesHandler>();
            ConfigurationContext.Current.TypeFactory.ForCommand<DeleteAllResources.Command>().SetHandler<DeleteAllResourcesHandler>();
            ConfigurationContext.Current.TypeFactory.ForCommand<DeleteResource.Command>().SetHandler<DeleteResourceHandler>();
            ConfigurationContext.Current.TypeFactory.ForCommand<RemoveTranslation.Command>().SetHandler<RemoveTranslationHandler>();
            ConfigurationContext.Current.TypeFactory.ForCommand<CreateOrUpdateTranslation.Command>().SetHandler<CreateOrUpdateTranslationHandler>();
        }
    }
}
