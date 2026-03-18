using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Inedo.Extensibility;
using Inedo.Web;

namespace Inedo.Extensions.Chocolatey.SuggestionProviders;

internal sealed class SpecialSourceSuggestionProvider : ISuggestionProvider
{
    public static readonly ImmutableArray<string> SpecialSources = ["ruby", "webpi", "cygwin", "python", "windowsfeatures"];

    public IAsyncEnumerable<string> GetSuggestionsAsync(IComponentConfiguration config, CancellationToken cancellationToken)
    {
        return SpecialSources.Where(source => source.Contains(config["Source"])).ToAsyncEnumerable();
    }
}
