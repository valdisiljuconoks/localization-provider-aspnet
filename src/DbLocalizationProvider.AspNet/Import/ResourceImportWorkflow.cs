// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DbLocalizationProvider.Cache;
using DbLocalizationProvider.Internal;

namespace DbLocalizationProvider.Import
{
    public class ResourceImportWorkflow
    {
        public object Import(IEnumerable<LocalizationResource> newResources, bool importOnlyNewContent)
        {
            var count = 0;

            //using(var db = new LanguageEntities())
            //{
            //    // if we are overwriting old content - we need to get rid of it first

            //    if(!importOnlyNewContent)
            //    {
            //        var existingResources = db.Set<LocalizationResource>();
            //        db.LocalizationResources.RemoveRange(existingResources);
            //        db.SaveChanges();
            //    }

            //    foreach(var localizationResource in newResources)
            //    {
            //        if(importOnlyNewContent)
            //        {
            //            // look for existing resource
            //            var existingResource = db.LocalizationResources
            //                                     .Include(r => r.Translations)
            //                                     .FirstOrDefault(r => r.ResourceKey == localizationResource.ResourceKey);

            //            if(existingResource == null)
            //            {
            //                // resource with this key does not exist - so we can just add it
            //                AddNewResource(db, localizationResource);
            //                count++;
            //            }
            //            else
            //            {
            //                // there is a resource with this key - looking for missing translations
            //                foreach(var translation in localizationResource.Translations)
            //                {
            //                    var existingTranslation = existingResource.Translations.FirstOrDefault(t => t.Language == translation.Language);

            //                    if(existingTranslation == null)
            //                    {
            //                        // there is no translation in that language - adding one
            //                        // but before adding that - we need to fix its reference to resource (exported file might have different id)
            //                        translation.ResourceId = existingResource.Id;
            //                        db.LocalizationResourceTranslations.Add(translation);
            //                    }
            //                    else if(string.IsNullOrEmpty(existingTranslation.Value))
            //                    {
            //                        // we can check - if content of the translation is empty - for us - it's the same as translation would not exist
            //                        existingTranslation.Value = translation.Value;
            //                    }
            //                }
            //            }
            //        }
            //        else
            //        {
            //            // don't care about state in DB
            //            // if we are importing all resources once again - all will be gone anyway
            //            AddNewResource(db, localizationResource);
            //            count++;
            //        }
            //    }

            //    db.SaveChanges();

            //    var c = new ClearCache.Command();
            //    c.Execute();
            //}

            return $"Import successful. Imported {count} resources";
        }

        //private static void AddNewResource(LanguageEntities db, LocalizationResource localizationResource)
        //{
        //    db.LocalizationResources.Add(localizationResource);
        //}

        public ICollection<DetectedImportChange> DetectChanges(ICollection<LocalizationResource> importingResources, IEnumerable<LocalizationResource> existingResources)
        {
            var result = new List<DetectedImportChange>();

            // deleted deletes
            var resourceComparer = new ResourceComparer();
            var deletes = existingResources.Except(importingResources, resourceComparer);
            result.AddRange(deletes.Select(d => new DetectedImportChange(ChangeType.Delete, LocalizationResource.CreateNonExisting(d.ResourceKey), d)));

            foreach(var incomingResource in importingResources.Except(deletes, resourceComparer))
            {
                var existing = existingResources.FirstOrDefault(r => r.ResourceKey == incomingResource.ResourceKey);
                if(existing != null)
                {
                    var comparer = new TranslationComparer(true);
                    var existingTranslations = existing.Translations.Where(_ => _ != null).ToList();
                    var differences = incomingResource.Translations.Except(existingTranslations, comparer).Where(_ => _ != null).ToList();

                    // some of the translations are different - so marking this resource as potential update
                    if(differences.Any())
                    {
                        // here we need to check whether incoming resource is overriding existing translation (exists translation in given language)
                        // or we are maybe importing exported language that had no translations
                        // this could happen if you export language with no translations in xliff format
                        // then new exported target language will have translation as empty string
                        // these cases we need to filter out

                        var detectedChangedLanguages = differences.Select(_ => _.Language).Distinct().ToList();
                        var existingLanguages = existingTranslations.Select(_ => _.Language).Distinct().ToList();

                        if(!differences.All(r => string.IsNullOrEmpty(r.Value)) || !detectedChangedLanguages.Except(existingLanguages).Any())
                        {
                            result.Add(new DetectedImportChange(ChangeType.Update, incomingResource, existing)
                                       {
                                           ChangedLanguages = detectedChangedLanguages
                                       });
                        }
                    }
                }
                else
                {
                    result.Add(new DetectedImportChange(ChangeType.Insert, incomingResource, LocalizationResource.CreateNonExisting(incomingResource.ResourceKey))
                               {
                                   ChangedLanguages = incomingResource.Translations.Select(t => t.Language).ToList()
                               });
                }
            }

            return result;
        }

        public IEnumerable<string> ImportChanges(ICollection<DetectedImportChange> changes)
        {
            var result = new List<string>();
            var inserts = 0;
            var updates = 0;
            var deletes = 0;

            //using(var db = new LanguageEntities())
            //{
            //    // process deletes
            //    foreach(var delete in changes.Where(c => c.ChangeType == ChangeType.Delete))
            //    {
            //        var existingResource = db.LocalizationResources.FirstOrDefault(r => r.ResourceKey == delete.ExistingResource.ResourceKey);
            //        if(existingResource != null)
            //            db.LocalizationResources.Remove(existingResource);
            //    }

            //    // process inserts
            //    foreach(var insert in changes.Where(c => c.ChangeType == ChangeType.Insert))
            //    {
            //        // fix incoming incomplete resource from web
            //        insert.ImportingResource.ModificationDate = DateTime.UtcNow;
            //        insert.ImportingResource.Author = "import";
            //        insert.ImportingResource.IsModified = false;

            //        // fix incoming resource translation invariant language (if any)
            //        insert.ImportingResource.Translations.ForEach(t => t.Language = t.Language ?? "");

            //        AddNewResource(db, insert.ImportingResource);
            //        inserts++;
            //    }

            //    // process updates
            //    foreach(var update in changes.Where(c => c.ChangeType == ChangeType.Update))
            //    {
            //        // look for existing resource
            //        var existingResource = db.LocalizationResources
            //                                 .Include(r => r.Translations)
            //                                 .FirstOrDefault(r => r.ResourceKey == update.ImportingResource.ResourceKey);

            //        if(existingResource == null)
            //        {
            //            // resource with this key does not exist - so we can just add it
            //            update.ImportingResource.ModificationDate = DateTime.UtcNow;
            //            update.ImportingResource.Author = "import";
            //            update.ImportingResource.IsModified = false;

            //            AddNewResource(db, update.ImportingResource);
            //            inserts++;
            //            continue;
            //        }

            //        foreach(var translation in update.ImportingResource.Translations.Where(_ => _.Value != null))
            //        {
            //            var existingTranslation = existingResource.Translations.FirstOrDefault(t => t.Language == translation.Language);

            //            if(existingTranslation == null)
            //            {
            //                // there is no translation in that language - adding one
            //                // but before adding that - we need to fix its reference to resource (exported file might have different id)
            //                translation.ResourceId = existingResource.Id;
            //                db.LocalizationResourceTranslations.Add(translation);
            //            }
            //            else
            //            {
            //                existingTranslation.Value = translation.Value;
            //            }
            //        }

            //        updates++;
            //    }

            //    db.SaveChanges();

            //    var clearCommand = new ClearCache.Command();
            //    clearCommand.Execute();
            //}

            if(inserts > 0)
                result.Add($"Inserted {inserts} resources.");

            if(updates > 0)
                result.Add($"Updated {updates} resources.");

            if(deletes > 0)
                result.Add($"Deleted {deletes} resources.");

            return result;
        }
    }
}
