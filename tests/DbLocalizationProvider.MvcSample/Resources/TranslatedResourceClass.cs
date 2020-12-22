using System.Web;
using DbLocalizationProvider.Abstractions;

namespace DbLocalizationProvider.MvcSample.Resources
{
    [LocalizedResource]
    public class TranslatedResourceClass
    {
        public string SomeProperty { get; set; } = "This is some property value";

        [LocalizedResource]
        public class NestedResourceClass
        {
            public string NestedProperty { get; set; } = "Nested property";
        }
    }
}
