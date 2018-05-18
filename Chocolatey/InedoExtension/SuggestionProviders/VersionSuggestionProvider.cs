using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inedo.Extensibility;
using Inedo.Web;
using NuGet;

namespace Inedo.Extensions.Chocolatey.SuggestionProviders
{
    internal sealed class VersionSuggestionProvider : ISuggestionProvider
    {
        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.Run(() => GetSuggestions(config["PackageName"], config["Version"], AH.CoalesceString(config["Source"], "https://chocolatey.org/api/v2")));
        }

        private static IEnumerable<string> GetSuggestions(string packageName, string partialVersion, string source)
        {
            if (SpecialSourceSuggestionProvider.SpecialSources.Contains(source))
                return Enumerable.Empty<string>();

            var repository = PackageRepositoryFactory.Default.CreateRepository(source);
            return repository.FindPackagesById(packageName).Select(pkg => pkg.Version.ToOriginalString()).Where(v => v.Contains(partialVersion));
        }
    }
}
