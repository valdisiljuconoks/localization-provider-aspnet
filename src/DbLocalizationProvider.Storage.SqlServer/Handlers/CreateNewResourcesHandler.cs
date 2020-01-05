// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Linq;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Commands;

namespace DbLocalizationProvider.Storage.SqlServer.Handlers
{
    public class CreateNewResourcesHandler : ICommandHandler<CreateNewResources.Command>
    {
        public void Execute(CreateNewResources.Command command)
        {
            if (command.LocalizationResources == null || !command.LocalizationResources.Any()) return;

            using (var db = new LanguageEntities())
            {
                foreach (var resource in command.LocalizationResources)
                {
                    resource.ModificationDate = DateTime.UtcNow;
                    db.LocalizationResources.Add(resource);
                }
                //else
                //{
                //    var existingResource = db.LocalizationResources.FirstOrDefault(r => r.ResourceKey == command.Key);
                //    if (existingResource != null) throw new InvalidOperationException($"Resource with key `{command.Key}` already exists");

                //    db.LocalizationResources.Add(new LocalizationResource(command.Key)
                //    {
                //        ModificationDate = DateTime.UtcNow, FromCode = command.FromCode, IsModified = false, Author = command.UserName
                //    });
                //}

                db.SaveChanges();
            }
        }
    }
}