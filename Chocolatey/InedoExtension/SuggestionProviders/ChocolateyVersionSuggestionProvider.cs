using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inedo.Extensibility;
using Inedo.Web;

namespace Inedo.Extensions.Chocolatey.SuggestionProviders
{
    internal sealed class ChocolateyVersionSuggestionProvider : ISuggestionProvider
    {
        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.Run(() => new[] { "latest" }.Concat(VersionSuggestionProvider.GetSuggestions("chocolatey", config["Version"], AH.CoalesceString(config["Source"], "https://chocolatey.org/api/v2"))));
        }
    }
}
