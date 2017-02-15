using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Inedo.Agents;
#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Operations;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Configurations;
using Inedo.Otter.Extensibility.Operations;
#endif
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.Extensions.Chocolatey.Configurations;

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Ensure Chocolatey Package")]
    [Description("Ensures that a Chocolatey package is installed on a server.")]
    [ScriptNamespace("Chocolatey")]
    [ScriptAlias("Ensure-Package")]
    public sealed class EnsurePackageOperation : EnsureOperation<ChocolateyPackageConfiguration>
    {
#if Otter
        public override async Task<PersistedConfiguration> CollectAsync(IOperationExecutionContext context)
        {
            var buffer = new StringBuilder("upgrade --yes --limit-output --fail-on-unfound --what-if ", 200);
            if (!string.IsNullOrEmpty(this.Template.Source))
            {
                buffer.Append("--source \"");
                buffer.Append(this.Template.Source);
                buffer.Append("\" ");
            }
            buffer.Append('\"');
            buffer.Append(this.Template.PackageName);
            buffer.Append('\"');

            var args = new List<string>();
            args.Add("upgrade");
            args.Add("--yes");
            args.Add("--limit-output");
            args.Add("--fail-on-unfound");
            args.Add("--what-if");
            if (!string.IsNullOrEmpty(this.Template.Source))
            {
                args.Add("--source");
                args.Add(this.Template.Source);
            }
            args.Add(this.Template.PackageName);

            var output = await this.ExecuteChocolateyAsync(context, buffer.ToString()).ConfigureAwait(false);
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

        private async Task<List<string[]>> ExecuteChocolateyAsync(IOperationExecutionContext context, string args)
        {
            var agent = await context.Agent.GetServiceAsync<IRemoteProcessExecuter>().ConfigureAwait(false);
            using (var process = agent.CreateProcess(new RemoteProcessStartInfo { FileName = "choco", Arguments = args }))
            {
                var output = new List<string[]>();

                process.OutputDataReceived +=
                    (s, e) =>
                    {
                        this.LogDebug(e.Data);
                        var data = e.Data.Trim().Split('|');
                        if (data.Length >= 2)
                            output.Add(data);
                    };

                bool error = false;

                process.ErrorDataReceived +=
                    (s, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            error = true;
                            this.LogError(e.Data);
                        }
                    };

                process.Start();
                await process.WaitAsync(context.CancellationToken).ConfigureAwait(false);
                if (process.ExitCode < 0)
                {
                    this.LogError("Chocolatey returned exit code " + process.ExitCode);
                    return null;
                }
                else if (error)
                {
                    return null;
                }

                return output;
            }
        }
#endif

        public override async Task ConfigureAsync(IOperationExecutionContext context)
        {
            var buffer = new StringBuilder("upgrade --yes --fail-on-unfound ", 200);
            if (context.Simulation)
                buffer.Append("--what-if ");

            if (!string.IsNullOrEmpty(this.Template.Version))
            {
                buffer.Append("--version \"");
                buffer.Append(this.Template.Version);
                buffer.Append("\" ");
            }

            if (!string.IsNullOrEmpty(this.Template.Source))
            {
                buffer.Append("--source \"");
                buffer.Append(this.Template.Source);
                buffer.Append("\" ");
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
            ).ConfigureAwait(false);

            if (exitCode != 0)
                this.LogError("Process exited with code " + exitCode);
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            if (string.IsNullOrEmpty(config[nameof(ChocolateyPackageConfiguration.Version)]))
            {
                return new ExtendedRichDescription(
                    new RichDescription(
                        "Ensure latest version of ",
                        new Hilite(config[nameof(ChocolateyPackageConfiguration.PackageName)]),
                        " from Chocolatey is installed"
                    )
                );
            }

            return new ExtendedRichDescription(
                new RichDescription(
                    "Ensure version ",
                    new Hilite(config[nameof(ChocolateyPackageConfiguration.Version)]),
                    " of ",
                    new Hilite(config[nameof(ChocolateyPackageConfiguration.PackageName)]),
                    " from Chocolatey is installed"
                )
            );
        }
    }
}
