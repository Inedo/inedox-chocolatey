using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inedo.Extensibility;
using Inedo.Web;

namespace Inedo.Extensions.Chocolatey.SuggestionProviders
{
    internal sealed class ChocolateyFeatureSuggestionProvider : ISuggestionProvider
    {
        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.FromResult(new[]
            {
                "checksumFiles",
                "autoUninstaller",
                "allowGlobalConfirmation",
                "failOnAutoUninstaller",
                "failOnStandardError",
                "powershellHost",
                "logEnvironmentValues",
                "virusCheck",
                "failOnInvalidOrMissingLicense",
                "ignoreInvalidOptionsSwitches",
                "usePackageExitCodes",
                "useFipsCompliantChecksums",
                "allowEmptyChecksums",
                "allowEmptyChecksumsSecure",
                "scriptsCheckLastExitCode",
                "showNonElevatedWarnings",
                "showDownloadProgress",
                "stopOnFirstPackageFailure",
                "useRememberedArgumentsForUpgrades",
                "ignoreUnfoundPackagesOnUpgradeOutdated",
                "removePackageInformationOnUninstall",
                "logWithoutColor"
            }.Where(f => f.ToLowerInvariant().Contains(config["Feature"].ToString().ToLowerInvariant())));
        }
    }
}
