using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
#if Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Web.Controls;
#elif BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Web.Controls;
#else
using Inedo.Extensibility;
using Inedo.Web;
#endif

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
