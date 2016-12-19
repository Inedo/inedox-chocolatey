using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using NuGet;
#if Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Web.Controls;
#elif BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Web.Controls;
#endif

namespace Inedo.Extensions.Chocolatey.SuggestionProviders
{
    internal sealed class PackageNameSuggestionProvider : ISuggestionProvider
    {
        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.Run(() => this.GetSuggestions(config["PackageName"], AH.CoalesceString(config["Source"], "https://chocolatey.org/api/v2")));
        }

        private IEnumerable<string> GetSuggestions(string packageName, string source)
        {
            if (SpecialSourceSuggestionProvider.SpecialSources.Contains(source))
                return Enumerable.Empty<string>();

            var repository = PackageRepositoryFactory.Default.CreateRepository(source);
            return repository.Search(packageName, false).AsEnumerable().Select(pkg => pkg.Id).Distinct();
        }
    }
}
