using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Inedo.Extensibility;
using Inedo.Extensibility.Credentials;
using Inedo.Extensibility.SecureResources;
using Inedo.Extensions.Chocolatey.Credentials;
using Inedo.Extensions.Credentials;
using Inedo.Web;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Inedo.Extensions.Chocolatey.SuggestionProviders
{
    internal sealed class VersionSuggestionProvider : ISuggestionProvider
    {
        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.FromResult(GetSuggestions(config, AH.CoalesceString(config["PackageName"], config["Name"]), config["Version"], config["Source"], AH.CoalesceString(config["ResourceName"], config["From"]), config["UserName"], AH.CreateSecureString(config["Password"])));
        }

        internal static IEnumerable<string> GetSuggestions(IComponentConfiguration config, string packageName, string partialVersion, string source, string resourceName, string userName, SecureString password)
        {
            if (SpecialSourceSuggestionProvider.SpecialSources.Contains(source))
                return Enumerable.Empty<string>();

            ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;
            SourceCacheContext cache = new SourceCacheContext();


            var packageSource = string.IsNullOrWhiteSpace(resourceName) ? null : (ChocolateySourceSecureResource)SecureResource.Create(resourceName, config.EditorContext as IResourceResolutionContext);
            var sourceUrl = AH.CoalesceString(source, packageSource?.SourceUrl, "https://chocolatey.org/api/v2");

            SourceRepository repository = Repository.Factory.GetCoreV2(
                new PackageSource(sourceUrl)
                {
                    ProtocolVersion = 2,
                    Credentials = packageSource?.GetCredentials(config.EditorContext as ICredentialResolutionContext) is not UsernamePasswordCredentials credentials
                                    ? null
                                    : new PackageSourceCredential(
                                        sourceUrl,
                                        AH.CoalesceString(userName, credentials.UserName),
                                        AH.CoalesceString(AH.Unprotect(password), AH.Unprotect(credentials.Password)),
                                        true,
                                        null
                                    )
                }
            );
            var resource = repository.GetResource<AutoCompleteResource>();
            var versionsTask = resource.VersionStartsWith(packageName, partialVersion, false, cache, logger, cancellationToken);
            versionsTask.Wait();
            return versionsTask.Result.OrderByDescending(v => v).Select(v => v.ToFullString());

        }
    }
}
