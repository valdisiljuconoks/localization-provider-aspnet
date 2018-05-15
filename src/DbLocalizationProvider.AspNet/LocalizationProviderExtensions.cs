using System.Globalization;
using Newtonsoft.Json;
using JsonConverter = DbLocalizationProvider.AspNet.Json.JsonConverter;

namespace DbLocalizationProvider.AspNet
{
    public static class LocalizationProviderExtensions
    {
        public static T Translate<T>(this LocalizationProvider provider)
        {
            return Translate<T>(provider, CultureInfo.CurrentUICulture);
        }

        public static T Translate<T>(this LocalizationProvider provider, CultureInfo language)
        {
            var converter = new JsonConverter();
            var className = typeof(T).FullName;

            var json = converter.GetJson(className, language.Name);

            // get the actual class Json representation (we need to select token through FQN of the class)
            // to supported nested classes - we need to fix a bit resource key name
            var jsonToken = json.SelectToken(className.Replace("+", "."));

            if(jsonToken == null)
                return default(T);

            return JsonConvert.DeserializeObject<T>(jsonToken.ToString());
        }
    }
}
