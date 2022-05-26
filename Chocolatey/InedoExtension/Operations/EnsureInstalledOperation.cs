using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
                var agent = await context.Agent.GetServiceAsync<IRemoteProcessExecuter>().ConfigureAwait(false);
                var fileOps = await context.Agent.GetServiceAsync<IFileOperationsExecuter>();

                var installLocation = AH.CoalesceString(await agent.GetEnvironmentVariableValueAsync("ChocolateyInstall"), @"C:\ProgramData\chocolatey");
                var chocoExistsAtInstallLocation = await fileOps.FileExistsAsync(Path.Combine(installLocation, "choco.exe"));

                if (chocoExistsAtInstallLocation)
                    return this.Collected = new ChocolateyInstalledConfiguration { Exists = true };

                var execOps = await context.Agent.GetServiceAsync<IRemoteProcessExecuter>();
                var startInfo = new RemoteProcessStartInfo
                {
                    FileName = fileOps.CombinePath(await execOps.GetEnvironmentVariableValueAsync("SystemRoot"), @"System32\WindowsPowerShell\v1.0\powershell.exe"),
                    Arguments = @"-NoProfile -InputFormat None -ExecutionPolicy Bypass -Command ""Get-Command -Name choco.exe -ErrorAction SilentlyContinue"""
                };
                var config = new ChocolateyInstalledConfiguration();
               
                using var process = agent.CreateProcess(startInfo);
                var output = new List<string>();

                process.OutputDataReceived +=
                    (s, e) =>
                    {
                        this.LogDebug(e.Data);
                        output.Add(e.Data.Trim());
                    };
                process.ErrorDataReceived +=
                    (s, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            this.LogError(e.Data);
                        }
                    };

                await process.StartAsync(context.CancellationToken);
                await process.WaitAsync(context.CancellationToken).ConfigureAwait(false);

                if (process.ExitCode == 0)
                {
                    if(output.Count > 0)
                        config.Exists = true;
                }

                this.Collected = config;
            }

            return this.Collected;
        }

        private ComparisonResult CompareInternal(PersistedConfiguration other)
        {
            var actual = (ChocolateyInstalledConfiguration)other;
            if (!actual.Exists)
            {
                return new ComparisonResult(new[] { new Difference(nameof(this.Template.Exists), true, actual.Exists) });
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
           

            var collected = await this.CollectAsync(context);

            if (!collected.Exists)
            {
                var startInfo = new RemoteProcessStartInfo();
                this.LogDebug("Installing Chocolatey...");

                startInfo.FileName = fileOps.CombinePath(await execOps.GetEnvironmentVariableValueAsync("SystemRoot"), @"System32\WindowsPowerShell\v1.0\powershell.exe");
                startInfo.Arguments = @$"-NoProfile -InputFormat None -ExecutionPolicy Bypass -Command ""[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('{this.Template.InstallScriptUrl}'))""";

                if (context.Simulation)
                    return;

                await this.ExecuteCommandLineAsync(context, startInfo);
                
            }
            else
            {
                this.LogDebug("No action needed.");
                return;
            }
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            var summary = new RichDescription("Ensure Chocolatey is installed");
            var details = new RichDescription();
            if (!string.IsNullOrEmpty(config[nameof(this.Template.InstallScriptUrl)]))
            {
                details.AppendContent("from ", new Hilite(config[nameof(this.Template.InstallScriptUrl)]));
            }

            return new ExtendedRichDescription(summary, details);
        }
    }
}
