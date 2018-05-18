using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inedo.Extensibility;
using Inedo.Web;

namespace Inedo.Extensions.Chocolatey.SuggestionProviders
{
    internal sealed class SpecialSourceSuggestionProvider : ISuggestionProvider
    {
        internal static readonly IReadOnlyCollection<string> SpecialSources = Array.AsReadOnly(new [] { "ruby", "webpi", "cygwin", "python", "windowsfeatures" });

        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.FromResult(SpecialSources.Where(source => source.Contains(config["Source"])));
        }
    }
}
