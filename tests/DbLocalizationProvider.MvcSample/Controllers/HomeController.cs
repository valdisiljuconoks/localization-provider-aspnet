using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using DbLocalizationProvider.AspNet;
using DbLocalizationProvider.MvcSample.Models;
using DbLocalizationProvider.MvcSample.Resources;

namespace DbLocalizationProvider.MvcSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly LocalizationProvider _provider;

        public HomeController()
        {
            _provider = LocalizationProvider.Current;
        }

        public ActionResult Index(string l)
        {
            var translatedObject = _provider.Translate<TranslatedResourceClass>();
            var nestedTranslatedObject = _provider.Translate<TranslatedResourceClass.NestedResourceClass>();
            var invalidClass = _provider.Translate<HomeController>();

            ViewData["FromTranslateObject"] = translatedObject.SomeProperty;

            if (!string.IsNullOrEmpty(l))
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(l);
            }

            var zz = LocalizationProvider.Current.GetString(() => HomePageResources.Header);
            var t2 = LocalizationProvider.Current.GetStringByCulture(() => HomePageResources.Header, CultureInfo.GetCultureInfo("no"));

            return View(new HomeViewModel());
        }

        [HttpPost]
        public ActionResult Index(HomeViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            return View(new HomeViewModel());
        }
    }
}
