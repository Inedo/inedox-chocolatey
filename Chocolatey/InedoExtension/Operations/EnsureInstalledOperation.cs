using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inedo.Agents;
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Extensibility.Operations;
using Inedo.Extensions.Chocolatey.Configurations;
using Inedo.IO;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Ensure Chocolatey Installed")]
    [Description("Ensure Chocolatey is installed.")]
    [ScriptAlias("Ensure-Installed")]
    [Tag("chocolatey")]
    public sealed class EnsureInstalledOperation : EnsureOperation<ChocolateyInstalledConfiguration>
    {
        private ChocolateyInstalledConfiguration Collected { get; set; }

        public override async Task<PersistedConfiguration> CollectAsync(IOperationCollectionContext context)
        {
            return await this.CollectAsync((IOperationExecutionContext)context);
        }
        private async Task<ChocolateyInstalledConfiguration> CollectAsync(IOperationExecutionContext context)
        {
            if (this.Collected == null)
            {
                var args = "upgrade --what-if --limit-output ";
                if (!string.IsNullOrWhiteSpace(this.Template.Source))
                    args += $"--source \"{this.Template.Source}\" ";

                args += "chocolatey";

                var output = await this.ExecuteChocolateyAsync(context, args, true);
                if (output == null)
                {
                    this.Collected = new ChocolateyInstalledConfiguration
                    {
                        Version = "not-installed"
                    };
                }
                else
                {
                    this.Collected = new ChocolateyInstalledConfiguration
                    {
                        Version = output[0][1],
                        LatestVersion = output[0][2]
                    };
                }
            }

            return this.Collected;
        }

        private ComparisonResult CompareInternal(PersistedConfiguration other)
        {
            var actual = (ChocolateyInstalledConfiguration)other;
            if (string.IsNullOrEmpty(this.Template.Version))
            {
                if (actual.Version == "not-installed")
                {
                    return new ComparisonResult(new[] { new Difference(nameof(this.Template.Version), "any", "not-installed") });
                }
            }
            else if (string.Equals(this.Template.Version, "latest", StringComparison.OrdinalIgnoreCase))
            {
                if (actual.Version != actual.LatestVersion)
                {
                    return new ComparisonResult(new[] { new Difference(nameof(this.Template.Version), actual.LatestVersion, actual.Version) });
                }
            }
            else if (this.Template.Version != actual.Version)
            {
                return new ComparisonResult(new[] { new Difference(nameof(this.Template.Version), this.Template.Version, actual.Version) });
            }

            return ComparisonResult.Identical;
        }

        public override Task<ComparisonResult> CompareAsync(PersistedConfiguration other, IOperationCollectionContext context)
        {
            return Task.FromResult(this.CompareInternal(other));
        }

        public override async Task ConfigureAsync(IOperationExecutionContext context)
        {
            var fileOps = await context.Agent.GetServiceAsync<IFileOperationsExecuter>();
            var execOps = await context.Agent.GetServiceAsync<IRemoteProcessExecuter>();
            var startInfo = new RemoteProcessStartInfo();

            var collected = await this.CollectAsync(context);

            if (collected.Version == "not-installed")
            {
                this.LogDebug("Installing Chocolatey...");

                startInfo.FileName = fileOps.CombinePath(await execOps.GetEnvironmentVariableValueAsync("SystemRoot"), @"System32\WindowsPowerShell\v1.0\powershell.exe");
                startInfo.Arguments = @"-NoProfile -InputFormat None -ExecutionPolicy AllSigned -Command ""iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))""";

                bool specificVersion = !string.IsNullOrEmpty(this.Template.Version) && !string.Equals(this.Template.Version, "latest", StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(this.Template.Source))
                {
                    if (!specificVersion)
                    {

#if NET452
                        var client = PackageRepositoryFactory.Default.CreateRepository(this.Template.Source);
                        var package = client.FindPackage("chocolatey", (SemanticVersion)null, false, false);
                        this.Template.Version = package.Version.ToOriginalString();
#else
                        NuGet.Common.ILogger logger = NullLogger.Instance;
                        SourceCacheContext cache = new SourceCacheContext();                        
                        SourceRepository repository = Repository.Factory.GetCoreV2(new PackageSource(this.Template.Source) { ProtocolVersion = 2 });
                        var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
                        var versions = await resource.GetAllVersionsAsync("chocolatey", cache, logger, context.CancellationToken);
                        this.Template.Version = versions.Where(v => !v.IsPrerelease).OrderByDescending(v => v).FirstOrDefault().ToFullString();
#endif
                    }
                    startInfo.EnvironmentVariables["chocolateyDownloadUrl"] = PathEx.Combine(this.Template.Source, "package", "chocolatey", this.Template.Version);
                }
                else if (specificVersion)
                {
                    startInfo.EnvironmentVariables["chocolateyVersion"] = this.Template.Version;
                }

                if (context.Simulation)
                    return;
            }
            else if (this.CompareInternal(collected).AreEqual)
            {
                this.LogDebug("No action needed.");
                return;
            }
            else
            {
                this.LogDebug("Upgrading Chocolatey...");

                var buffer = new StringBuilder(200);

                buffer.Append("upgrade --yes --fail-on-unfound ");
                if (context.Simulation)
                    buffer.Append("--what-if ");

                if (!string.IsNullOrEmpty(this.Template.Version) && !string.Equals(this.Template.Version, "latest", StringComparison.OrdinalIgnoreCase))
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

                buffer.Append("chocolatey");
            }

            await this.ExecuteCommandLineAsync(context, startInfo);
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            var summary = new RichDescription("Ensure ");
            var version = config[nameof(this.Template.Version)];
            if (!string.IsNullOrEmpty(version))
            {
                if (string.Equals(version, "latest", StringComparison.OrdinalIgnoreCase))
                {
                    summary.AppendContent(new Hilite("latest"), " version of ");
                }
                else
                {
                    summary.AppendContent("version ", new Hilite(version), " of ");
                }
            }
            summary.AppendContent("Chocolatey is installed");

            var details = new RichDescription();
            if (!string.IsNullOrEmpty(config[nameof(this.Template.Source)]))
            {
                details.AppendContent("from ", new Hilite(config[nameof(this.Template.Source)]));
            }

            return new ExtendedRichDescription(summary, details);
        }
    }
}
