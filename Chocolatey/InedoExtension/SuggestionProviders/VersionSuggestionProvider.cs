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
    internal sealed class VersionSuggestionProvider : ISuggestionProvider
    {
        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.FromResult(GetSuggestions(config["PackageName"], config["Version"], AH.CoalesceString(config["Source"], "https://chocolatey.org/api/v2")));
        }

        internal static IEnumerable<string> GetSuggestions(string packageName, string partialVersion, string source)
        {
            if (SpecialSourceSuggestionProvider.SpecialSources.Contains(source))
                return Enumerable.Empty<string>();

#if NET452
            var repository = PackageRepositoryFactory.Default.CreateRepository(source);
            return repository.FindPackagesById(packageName).OrderByDescending(o => o.Version).Select(pkg => pkg.Version.ToOriginalString()).Where(v => v.Contains(partialVersion));
#else
            ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;
            SourceCacheContext cache = new SourceCacheContext();                        
            SourceRepository repository = Repository.Factory.GetCoreV2(new PackageSource(source) { ProtocolVersion = 2 });
            var resource = repository.GetResource<AutoCompleteResource>();
            var versionsTask = resource.VersionStartsWith(packageName, partialVersion, false, cache, logger, cancellationToken);
            versionsTask.Wait();
            return versionsTask.Result.OrderByDescending(v => v).Select(v => v.ToFullString());
#endif
        }
    }
}
