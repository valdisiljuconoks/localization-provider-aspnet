// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DbLocalizationProvider.AdminUI.Infrastructure;
using DbLocalizationProvider.AdminUI.Models;
using DbLocalizationProvider.AspNet.Import;
using DbLocalizationProvider.Cache;
using DbLocalizationProvider.Commands;
using DbLocalizationProvider.Export;
using DbLocalizationProvider.Import;
using DbLocalizationProvider.Internal;
using DbLocalizationProvider.Queries;

namespace DbLocalizationProvider.AdminUI
{
    public class JsonServiceResult
    {
        public string Message { get; set; }
    }

    [AuthorizeRoles]
    public class LocalizationResourcesController : Controller
    {
        private bool _showInvariantCulture;
        private readonly int _maxLength;
        private const string _cookieName = ".DbLocalizationProvider-SelectedLanguages";
        private const string _viewCookieName = ".DbLocalizationProvider-DefaultView";

        public LocalizationResourcesController()
        {
            _showInvariantCulture = UiConfigurationContext.Current.ShowInvariantCulture;
            _maxLength = UiConfigurationContext.Current.MaxResourceKeyDisplayLength;
        }

        public ActionResult Index(string query)
        {
            return View(PrepareViewModel(false, query));
        }

        public ActionResult Main(string query)
        {
            return View("Index", PrepareViewModel(true, query));
        }

        private LocalizationResourceViewModel PrepareViewModel(bool showMenu, string query = "")
        {
            var availableLanguagesQuery = new AvailableLanguages.Query { IncludeInvariant = _showInvariantCulture };
            var languages = availableLanguagesQuery.Execute();
            long? rowCount;
            var allResources = GetAllResources(query, out rowCount);

            var user = HttpContext.User;
            var isAdmin = user.Identity.IsAuthenticated && UiConfigurationContext.Current.AuthorizedAdminRoles.Any(r => user.IsInRole(r));

            // cookies override default view from config
            var isTreeView = UiConfigurationContext.Current.DefaultView == ResourceListView.Tree;
            if(Request.Cookies[_viewCookieName] != null)
            {
                isTreeView = UiConfigurationContext.Current.IsTableViewDisabled || Request.Cookies[_viewCookieName]?.Value == "tree";
            }

            var result = new LocalizationResourceViewModel(allResources, languages, GetSelectedLanguages(), _maxLength)
            {
                ShowMenu = showMenu,
                AdminMode = isAdmin,
                IsTreeView = isTreeView,
                IsTreeViewEnabled = !UiConfigurationContext.Current.IsTreeViewDisabled,
                IsTableViewEnabled = !UiConfigurationContext.Current.IsTableViewDisabled,
                IsRemoveTranslationButtonDisabled = UiConfigurationContext.Current.DisableRemoveTranslationButton,
                IsDeleteButtonVisible = !UiConfigurationContext.Current.HideDeleteButton,
                IsDbSearchEnabled = UiConfigurationContext.Current.EnableDbSearch,
                Query = query,
                TotalRowCount = rowCount ?? 0
            };

            // build tree
            var builder = new ResourceTreeBuilder();
            var sorter = new ResourceTreeSorter();
            result.Tree = sorter.Sort(builder.BuildTree(allResources, ConfigurationContext.Current.EnableLegacyMode()));

            return result;
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create([ModelBinder(typeof(RawBodyBinder))] CreateResourceRequestModel model)
        {
            try
            {
                var resourceKey = model.Key;

                // validate resource key
                var whitelist = ConfigurationContext.Current.ResourceKeyNameFilter ?? new Regex(".");
                if(!whitelist.IsMatch(resourceKey)) throw new ArgumentException("Invalid resource key value");
                if(!model.Translations.Any()) throw new InvalidOperationException("At least single translations is required!");

                var resource = new LocalizationResource(resourceKey)
                {
                    Author = HttpContext.User.Identity.Name,
                    FromCode = false,
                    IsModified = false,
                    IsHidden = false,
                    ModificationDate = DateTime.UtcNow,
                };

                // fill in translations
                model.Translations.ForEach(t =>
                {
                    resource.Translations.Add(new LocalizationResourceTranslation
                    {
                        Language = t.Language.Equals("invariant", StringComparison.InvariantCultureIgnoreCase) ? string.Empty : t.Language,
                        Value = t.Translation
                    });
                });

                // check if we have invariant translation; if not - fill in from 1st translation
                if (resource.Translations.FindByLanguage(CultureInfo.InvariantCulture) == null)
                {
                    resource.Translations.Add(new LocalizationResourceTranslation
                    {
                        Language = CultureInfo.InvariantCulture.Name,
                        Value = resource.Translations.FirstOrDefault()?.Value ?? "N/A"
                    });
                }

                var c = new CreateNewResources.Command(new List<LocalizationResource> { resource });
                c.Execute();

                // raise new resources created event
                // TODO: maybe this is not the best place for event
                UiConfigurationContext.Current.Events.InvokeNewResourceCreated(resourceKey);

                return Json("");
            }
            catch (Exception e)
            {
                Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                return Json(new JsonServiceResult
                            {
                                Message = e.Message
                            });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Delete([Bind(Prefix = "pk")] string resourceKey, string returnUrl)
        {
            try
            {
                var user = HttpContext.User;
                var isAdmin = user.Identity.IsAuthenticated && UiConfigurationContext.Current.AuthorizedAdminRoles.Any(r => user.IsInRole(r));

                if (isAdmin && !UiConfigurationContext.Current.HideDeleteButton)
                {
                    var c = new DeleteResource.Command(resourceKey);
                    c.Execute();
                }

                return Redirect(returnUrl);
            }
            catch (Exception e)
            {
                Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                return Json(new JsonServiceResult
                            {
                                Message = e.Message
                            });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update([Bind(Prefix = "pk")] string resourceKey,
                                 [Bind(Prefix = "value")] string newValue,
                                 [Bind(Prefix = "name")] string language)
        {
            var c = new CreateOrUpdateTranslation.Command(resourceKey,
                language.Equals("invariant", StringComparison.InvariantCultureIgnoreCase)
                    ? CultureInfo.InvariantCulture
                    : new CultureInfo(language),
                newValue);

            c.Execute();

            return Json("");
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Remove([Bind(Prefix = "pk")] string resourceKey,
                                 [Bind(Prefix = "name")] string language)
        {
            var c = new RemoveTranslation.Command(resourceKey, new CultureInfo(language));
            c.Execute();

            return Json("");
        }

        [HttpPost]
        public ActionResult UpdateLanguages(string[] languages, bool? showMenu)
        {
            // issue cookie to store selected languages
            WriteSelectedLanguages(languages);

            return RedirectToAction(showMenu.HasValue && showMenu.Value ? "Main" : "Index");
        }

        public FileResult ExportResources(string format = "json")
        {
            var exporter = ConfigurationContext.Current.Export.Providers.FindById(format);
            var resources = new GetAllResources.Query(true).Execute();
            var languages = new AvailableLanguages.Query().Execute();

            foreach(var resource in resources)
            {
                var exportableTranslations = new List<LocalizationResourceTranslation>();
                var invariantTranslation = resource.Translations.FindByLanguage(CultureInfo.InvariantCulture);
                if(invariantTranslation != null)
                    exportableTranslations.Add(invariantTranslation);

                foreach(var language in languages)
                {
                    var t = resource.Translations.FindByLanguage(language);
                    if(t != null)
                        exportableTranslations.Add(t);
                }

                resource.Translations = exportableTranslations;
            }

            var result = exporter.Export(resources.ToList(), Request.Params);

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(result.SerializedData);
            writer.Flush();
            stream.Position = 0;

            return File(stream, result.FileMimeType, result.FileName);
        }

        public ActionResult Tree(bool? showMenu)
        {
            var cookie = new HttpCookie(_viewCookieName, "tree") { HttpOnly = true };
            Response.Cookies.Add(cookie);

            return RedirectToAction(showMenu.HasValue && showMenu.Value ? "Main" : "Index");
        }

        public ActionResult Table(bool? showMenu)
        {
            var cookie = new HttpCookie(_viewCookieName, "table") { HttpOnly = true };
            Response.Cookies.Add(cookie);

            return RedirectToAction(showMenu.HasValue && showMenu.Value ? nameof(Main) : nameof(Index));
        }

        public ActionResult CleanCache(bool? showMenu)
        {
            new ClearCache.Command().Execute();

            return RedirectToAction(showMenu.HasValue && showMenu.Value ? nameof(Main) : nameof(Index));
        }

        [AuthorizeRoles(Mode = UiContextMode.Admin)]
        public ViewResult ImportResources(bool? showMenu)
        {
            return View("ImportResources",
                new ImportResourcesViewModel
                {
                    ShowMenu = showMenu ?? false
                });
        }

        [HttpPost]
        [AuthorizeRoles(Mode = UiContextMode.Admin)]
        [ValidateInput(false)]
        public ViewResult ImportResources(bool? previewImport, HttpPostedFileBase importFile, bool? showMenu)
        {
            var model = new ImportResourcesViewModel { ShowMenu = showMenu ?? false };
            if(importFile == null || importFile.ContentLength == 0)
            {
                return View("ImportResources", model);
            }

            var fileInfo = new FileInfo(importFile.FileName);
            var potentialParser = ConfigurationContext.Current.Import.Providers.FindByExtension(fileInfo.Extension);

            if(potentialParser == null)
            {
                ModelState.AddModelError("file", $"Unknown file extension - `{fileInfo.Extension}`");
                return View("ImportResources", model);
            }

            var workflow = new ResourceImportWorkflow();
            var streamReader = new StreamReader(importFile.InputStream);
            var fileContent = streamReader.ReadToEnd();

            try
            {
                var parseResult = potentialParser.Parse(fileContent);
                if (previewImport.HasValue && previewImport.Value)
                {
                    var changes = workflow.DetectChanges(parseResult.Resources, new GetAllResources.Query(true).Execute());
                    var changedLanguages = changes.SelectMany(c => c.ChangedLanguages).Distinct().Select(l => new CultureInfo(l));

                    var previewModel = new PreviewImportResourcesViewModel(changes, showMenu ?? false, changedLanguages);

                    return View("ImportPreview", previewModel);
                }

                var result = workflow.Import(parseResult.Resources, true);
                ViewData["LocalizationProvider_ImportResult"] = result;
            }
            catch (Exception e)
            {
                ModelState.AddModelError("importFailed", $"Import failed! Reason: {e.Message}");
            }

            return View("ImportResources", model);
        }

        [HttpPost]
        [AuthorizeRoles(Mode = UiContextMode.Admin)]
        [ValidateInput(false)]
        public ViewResult CommitImportResources(bool? previewImport, bool? showMenu, ICollection<DetectedImportChange> changes)
        {
            var model = new ImportResourcesViewModel
            {
                ShowMenu = showMenu ?? false
            };

            try
            {
                // prepare incoming model a bit
                // if change is selected and translation and/or language is `null` -> most probably this means that translation was empty
                // but Mvc model binder set it to `null` -> we need to fix this to get functionality to set empty translations via import process
                var importer = new ResourceImportWorkflow();
                var detectedImportChanges = changes.Where(c => c.Selected)
                    .ForEach(c =>
                    {
                        c.ImportingResource.Translations.ForEach(t =>
                        {
                            t.Value = t.Value ?? (t.Value = string.Empty);
                            t.Language = t.Language ?? (t.Language = string.Empty);
                        });
                    })
                    .ToList();

                var result = importer.ImportChanges(detectedImportChanges);

                ViewData["LocalizationProvider_ImportResult"] = string.Join("<br/>", result);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("importFailed", $"Import failed! Reason: {e.Message}");
            }

            return View("ImportResources", model);
        }

        private IEnumerable<string> GetSelectedLanguages()
        {
            var cookie = Request.Cookies[_cookieName];
            return cookie?.Value?.Split(new[]
                                        {
                                            "|"
                                        },
                                        StringSplitOptions.RemoveEmptyEntries);
        }

        private List<ResourceListItem> GetAllResources(string query, out long? rowCount)
        {
            var result = new List<ResourceListItem>();
            List<LocalizationResource> resources;

            rowCount = 0;
            if (UiConfigurationContext.Current.EnableDbSearch)
            {
                var queryResult = new GetSearchResources.Query(query, 1, UiConfigurationContext.Current.PageSize, true).Execute();
                rowCount = queryResult.RowCount;
                resources = queryResult.Result
                                       .OrderBy(r => r.ResourceKey)
                                       .ToList();
            }
            else
            {
                resources = new GetAllResources.Query(true)
                            .Execute()
                            .OrderBy(r => r.ResourceKey)
                            .ToList();
            }

            foreach (var resource in resources)
            {
                result.Add(new ResourceListItem(resource.ResourceKey,
                    resource.Translations
                            .Select(t => new ResourceItem(resource.ResourceKey, t.Value, new CultureInfo(t.Language)))
                            .ToList(),
                    !resource.FromCode,
                    resource.IsHidden.HasValue && resource.IsHidden.Value,
                    resource.IsModified.HasValue && resource.IsModified.Value));
            }

            return result;
        }

        private void WriteSelectedLanguages(IEnumerable<string> languages)
        {
            var cookie = new HttpCookie(_cookieName, string.Join("|", languages ?? new[] { string.Empty }))
                         {
                             HttpOnly = true
                         };
            Response.Cookies.Add(cookie);
        }
    }
}
