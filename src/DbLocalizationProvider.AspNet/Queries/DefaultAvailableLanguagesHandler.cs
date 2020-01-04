using System.Collections.Generic;
using System.Globalization;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Queries;

namespace DbLocalizationProvider.AspNet.Queries
{
    public class DefaultAvailableLanguagesHandler : IQueryHandler<AvailableLanguages.Query, IEnumerable<CultureInfo>>
    {
        private readonly List<CultureInfo> _noLanguages = new List<CultureInfo> { CultureInfo.InvariantCulture };

        public IEnumerable<CultureInfo> Execute(AvailableLanguages.Query query) => _noLanguages;
    }
}