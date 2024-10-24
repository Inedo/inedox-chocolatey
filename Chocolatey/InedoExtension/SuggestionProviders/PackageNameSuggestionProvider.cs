using System.Collections.Generic;
using System.Linq;
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
    internal sealed class PackageNameSuggestionProvider : ISuggestionProvider
    {
        public async Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
        
            if (SpecialSourceSuggestionProvider.SpecialSources.Contains(config["Source"]))
                return Enumerable.Empty<string>();
            ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;
            SourceCacheContext cache = new SourceCacheContext();


            var packageSource = string.IsNullOrWhiteSpace(config["ResourceName"]) ? null : (ChocolateySourceSecureResource)SecureResource.Create(SecureResourceType.General, config["ResourceName"], config.EditorContext as IResourceResolutionContext);
            var sourceUrl = AH.CoalesceString(config["Source"], packageSource?.SourceUrl, "https://chocolatey.org/api/v2");

            PackageSourceCredential credentials = null;
            if (!string.IsNullOrWhiteSpace(config["UserName"]) && !string.IsNullOrWhiteSpace(config["Password"]))
            {
                credentials = new PackageSourceCredential(
                                        sourceUrl,
                                        config["UserName"],
                                        config["Password"],
                                        true,
                                        null
                                    );
            }
            else
            {
                var packageCredentials = packageSource?.GetCredentials(config.EditorContext as ICredentialResolutionContext);
                if (packageCredentials != null && packageCredentials is UsernamePasswordCredentials usernamePassword)
                {
                    credentials = new PackageSourceCredential(
                                        sourceUrl,
                                        usernamePassword.UserName,
                                        AH.Unprotect(usernamePassword.Password),
                                        true,
                                        null
                                    );
                }
                else if (packageCredentials != null && packageCredentials is TokenCredentials token)
                {
                    credentials = new PackageSourceCredential(
                                        sourceUrl,
                                        "API",
                                        AH.Unprotect(token.Token),
                                        true,
                                        null
                                    );
                }
            }


            SourceRepository repository = Repository.Factory.GetCoreV2(new PackageSource(sourceUrl) { ProtocolVersion = 2, Credentials = credentials });
            var resource = repository.GetResource<AutoCompleteResource>();
            return await resource.IdStartsWith(config["PackageName"], false, logger, cancellationToken);
           
        }
    }
}
