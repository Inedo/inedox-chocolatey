using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Inedo.Agents;
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Extensibility.Credentials;
using Inedo.Extensibility.Operations;
using Inedo.Extensibility.SecureResources;
using Inedo.Extensions.Chocolatey.Configurations;
using Inedo.Extensions.Chocolatey.Credentials;
using Inedo.Extensions.Credentials;

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Ensure Chocolatey Package")]
    [Description("Ensures that a Chocolatey package is installed on a server.")]
    [ScriptAlias("Ensure-Package")]
    [Tag("chocolatey")]
    public sealed class EnsurePackageOperation : EnsureOperation<ChocolateyPackageConfiguration>
    {
        public override async Task<PersistedConfiguration> CollectAsync(IOperationCollectionContext context)
        {
            var buffer = new StringBuilder("upgrade --yes --limit-output --fail-on-unfound --what-if ", 200);
            
            var packageSource = string.IsNullOrWhiteSpace(this.Template.ResourceName) ? null : (ChocolateySourceSecureResource)SecureResource.Create(SecureResourceType.General, this.Template.ResourceName, (IResourceResolutionContext)context);
            var source = AH.CoalesceString(this.Template.Source, packageSource?.SourceUrl);
            if (!string.IsNullOrEmpty(source))
            {
                this.LogDebug("Using source " + source);
                buffer.Append("--source=\"");
                buffer.Append(source);
                buffer.Append("\" ");
            }

            if (!string.IsNullOrWhiteSpace(this.Template.UserName) && this.Template.Password != null)
            {
                this.LogDebug("Using user " + this.Template.UserName);
                buffer.Append("--user=\"");
                buffer.Append(this.Template.UserName);
                buffer.Append("\" ");

                buffer.Append("--password=\"");
                buffer.Append(AH.Unprotect(this.Template.Password));
                buffer.Append("\" ");
            }
            else
            {
                var credentials = packageSource?.GetCredentials((ICredentialResolutionContext)context);
                if (credentials != null)
                {
                    if (credentials is UsernamePasswordCredentials usernamePassword)
                    {
                        this.LogDebug("Using user " + usernamePassword.UserName);
                       buffer.Append(@$"--user=""{usernamePassword.UserName}"" --password=""{AH.Unprotect(usernamePassword.Password)}"" ");
                    }
                    else if (credentials is TokenCredentials token)
                    {
                        this.LogDebug("Using ProGet API Key");
                        buffer.Append(@$"--user=""api"" --password=""{AH.Unprotect(token.Token)}"" ");
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.Template.Version))
            {
                buffer.Append("--version \"");
                buffer.Append(this.Template.Version);
                buffer.Append("\" ");
                buffer.Append("--allow-downgrade ");
            }

            if (!string.IsNullOrWhiteSpace(this.Template.AdditionalInstallArguments))
            {
                buffer.Append(this.Template.AdditionalInstallArguments);
                buffer.Append(' ');
            }            

            buffer.Append('\"');
            buffer.Append(this.Template.PackageName);
            buffer.Append('\"');

            var output = await this.ExecuteChocolateyAsync(context, buffer.ToString(), true);
            if (output == null || output.Count < 1 || output[0].Length < 4 || !string.Equals(output[0][3], "false", StringComparison.OrdinalIgnoreCase))
            {
                // this assumes packages are never pinned
                this.LogInformation($"Package {this.Template.PackageName} is not installed.");
                return new ChocolateyPackageConfiguration
                {
                    Exists = false,
                    PackageName = this.Template.PackageName,
                    Source = this.Template.Source
                };
            }

            var installedVersion = output[0][1];
            var availableVersion = output[0][2];

            this.LogInformation($"Package {this.Template.PackageName} is at version {availableVersion}.");
            this.LogInformation($"Version {installedVersion} is installed.");

            return new ChocolateyPackageConfiguration
            {
                Exists = true,
                PackageName = this.Template.PackageName,
                Version = installedVersion,
                IsLatestVersion = string.Equals(installedVersion, availableVersion, StringComparison.OrdinalIgnoreCase),
                Source = this.Template.Source
            };
        }

        public override Task<ComparisonResult> CompareAsync(PersistedConfiguration other, IOperationCollectionContext context)
        {
            var diffs = new List<Difference>();
            var config = (ChocolateyPackageConfiguration)other;

            if (this.Template.Exists != config.Exists)
                diffs.Add(new Difference(nameof(ChocolateyPackageConfiguration.Exists), this.Template.Exists, config.Exists));

            if (this.Template.Exists && config.Exists)
            {
                if (string.IsNullOrEmpty(this.Template.Version) && !config.IsLatestVersion)
                    diffs.Add(new Difference(nameof(ChocolateyPackageConfiguration.IsLatestVersion), true, false));

                if (!string.IsNullOrEmpty(this.Template.Version) && !string.Equals(this.Template.Version, config.Version, StringComparison.OrdinalIgnoreCase))
                    diffs.Add(new Difference(nameof(ChocolateyPackageConfiguration.Version), this.Template.Version, config.Version));
            }

            return Task.FromResult(new ComparisonResult(diffs));
        }

        public override async Task ConfigureAsync(IOperationExecutionContext context)
        {
            var buffer = new StringBuilder(200);

            if (this.Template.Exists)
            {
                buffer.Append("upgrade --yes --fail-on-unfound ");
                if (context.Simulation)
                    buffer.Append("--what-if ");

                if (!string.IsNullOrEmpty(this.Template.Version))
                {
                    buffer.Append("--version \"");
                    buffer.Append(this.Template.Version);
                    buffer.Append("\" ");
                    buffer.Append("--allow-downgrade ");
                }

                var packageSource = string.IsNullOrWhiteSpace(this.Template.ResourceName) ? null : (ChocolateySourceSecureResource)SecureResource.Create(SecureResourceType.General, this.Template.ResourceName, (IResourceResolutionContext)context);
                var source = AH.CoalesceString(this.Template.Source, packageSource?.SourceUrl);
                if (!string.IsNullOrEmpty(source))
                {
                    this.LogDebug("Using source " + source);
                    buffer.Append("--source=\"");
                    buffer.Append(source);
                    buffer.Append("\" ");
                }

                if (!string.IsNullOrWhiteSpace(this.Template.UserName) && this.Template.Password != null)
                {
                    this.LogDebug("Using user " + this.Template.UserName);
                    buffer.Append("--user=\"");
                    buffer.Append(this.Template.UserName);
                    buffer.Append("\" ");

                    buffer.Append("--password=\"");
                    buffer.Append(AH.Unprotect(this.Template.Password));
                    buffer.Append("\" ");
                }
                else
                {
                    var credentials = packageSource?.GetCredentials((ICredentialResolutionContext)context);
                    if (credentials != null)
                    {
                        if (credentials is UsernamePasswordCredentials usernamePassword)
                        {
                            this.LogDebug("Using user " + usernamePassword.UserName);
                            buffer.Append(@$"--user=""{usernamePassword.UserName}"" --password=""{AH.Unprotect(usernamePassword.Password)}"" ");
                        }
                        else if (credentials is TokenCredentials token)
                        {
                            this.LogDebug("Using ProGet API Key");
                            buffer.Append(@$"--user=""api"" --password=""{AH.Unprotect(token.Token)}"" ");
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(this.Template.AdditionalInstallArguments))
                {
                    buffer.Append(this.Template.AdditionalInstallArguments);
                    buffer.Append(' ');
                }
            }
            else
            {
                buffer.Append("uninstall --yes --remove-dependencies ");
                if (context.Simulation)
                    buffer.Append("--what-if ");
            }

            buffer.Append('\"');
            buffer.Append(this.Template.PackageName);
            buffer.Append('\"');


            await this.ExecuteChocolateyAsync(context, buffer.ToString());
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            var state = "installed";
            if (string.Equals(config[nameof(ChocolateyPackageConfiguration.Exists)], "false", StringComparison.OrdinalIgnoreCase))
                state = "not installed";

            if (string.IsNullOrEmpty(config[nameof(ChocolateyPackageConfiguration.Version)]))
            {
                return new ExtendedRichDescription(
                    new RichDescription(
                        "Ensure latest version of ",
                        new Hilite(config[nameof(ChocolateyPackageConfiguration.PackageName)]),
                        " from Chocolatey is " + state
                    )
                );
            }

            return new ExtendedRichDescription(
                new RichDescription(
                    "Ensure version ",
                    new Hilite(config[nameof(ChocolateyPackageConfiguration.Version)]),
                    " of ",
                    new Hilite(config[nameof(ChocolateyPackageConfiguration.PackageName)]),
                    " from Chocolatey is " + state
                )
            );
        }
    }
}
