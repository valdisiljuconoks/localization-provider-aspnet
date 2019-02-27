using Newtonsoft.Json;

namespace DbLocalizationProvider.AdminUI.Models {
    [JsonObject]
    public class CreateOrUpdateTranslationRequestModel
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("newTranslation")]
        public string Translation { get; set; }
    }
}