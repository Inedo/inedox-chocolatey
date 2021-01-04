using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inedo.Extensibility;
using Inedo.Web;
#if NET452
using NuGet;
#else
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
#endif

namespace Inedo.Extensions.Chocolatey.SuggestionProviders
{
    internal sealed class PackageNameSuggestionProvider : ISuggestionProvider
    {
        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.FromResult(GetSuggestions(config["PackageName"], AH.CoalesceString(config["Source"], "https://chocolatey.org/api/v2")));
        }

        private static IEnumerable<string> GetSuggestions(string packageName, string source)
        {
            if (SpecialSourceSuggestionProvider.SpecialSources.Contains(source))
                return Enumerable.Empty<string>();
#if NET452
            var repository = PackageRepositoryFactory.Default.CreateRepository(source);
            var results = repository.Search(packageName, false).ToList();
            return results.OrderBy(x=>x.Id).Select(pkg => pkg.Id).AsEnumerable().Distinct();
#else
            ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;
            SourceCacheContext cache = new SourceCacheContext();
            SourceRepository repository = Repository.Factory.GetCoreV2(new PackageSource(source) { ProtocolVersion = 2 });
            var resource = repository.GetResource<AutoCompleteResource>();
            var resultsTask = resource.IdStartsWith(packageName, false, logger, cancellationToken);
            resultsTask.Wait();
            return resultsTask.Result;
#endif
        }
    }
}
