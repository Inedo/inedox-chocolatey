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
using Inedo.Extensibility.Operations;
using Inedo.Extensions.Chocolatey.Configurations;

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
            if (!string.IsNullOrEmpty(this.Template.Source))
            {
                buffer.Append("--source \"");
                buffer.Append(this.Template.Source);
                buffer.Append("\" ");
            }

            if (!string.IsNullOrWhiteSpace(this.Template.AdditionalInstallArguments))
            {
                buffer.Append(this.Template.AdditionalInstallArguments);
                buffer.Append(' ');
            }

            buffer.Append('\"');
            buffer.Append(this.Template.PackageName);
            buffer.Append('\"');

            var output = await this.ExecuteChocolateyAsync(context, buffer.ToString());
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

        public override ComparisonResult Compare(PersistedConfiguration other)
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

            return new ComparisonResult(diffs);
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

                if (!string.IsNullOrEmpty(this.Template.Source))
                {
                    buffer.Append("--source \"");
                    buffer.Append(this.Template.Source);
                    buffer.Append("\" ");
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

            int exitCode = await this.ExecuteCommandLineAsync(
                context,
                new RemoteProcessStartInfo
                {
                    FileName = "choco",
                    Arguments = buffer.ToString()
                }
            );

            if (exitCode != 0)
                this.LogError("Process exited with code " + exitCode);
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
