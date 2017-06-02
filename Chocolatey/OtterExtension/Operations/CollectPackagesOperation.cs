using Inedo.Agents;
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.Otter.Data;
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Operations;
using Inedo.Otter.Extensions.Configurations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Collect Chocolatey Packages")]
    [Description("Collects the names and versions of chocolatey packages installed on a server.")]
    [ScriptAlias("Collect-Packages")]
    [Tag("chocolatey")]
    [ScriptNamespace("Chocolatey")]
    public sealed class CollectPackagesOperation : CollectOperation<DictionaryConfiguration>
    {
        public async override Task<DictionaryConfiguration> CollectConfigAsync(IOperationExecutionContext context)
        {
            var output = await this.ExecuteChocolateyAsync(context, "list --limit-output --local-only").ConfigureAwait(false);

            using (var db = new DB.Context())
            {
                await db.ServerPackages_DeletePackagesAsync(Server_Id: context.ServerId, PackageType_Name: "Chocolately").ConfigureAwait(false);

                foreach (var values in output)
                {
                    string name = values[0];
                    string version = values[1];

                    await db.ServerPackages_CreateOrUpdatePackageAsync(
                        Server_Id: context.ServerId,
                        PackageType_Name: "Chocolatey",
                        Package_Name: name,
                        Package_Version: version,
                        CollectedOn_Execution_Id: context.ExecutionId,
                        Url_Text: null,
                        CollectedFor_ServerRole_Id: context.ServerRoleId
                    ).ConfigureAwait(false);
                }
            }

            return null;
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(
                new RichDescription("Collect Chocolatey Packages")
            );
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

                try
                {
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
                catch (Win32Exception ex)
                {
                    this.LogError("There was an error executing chocolatey. Ensure that chocolatey is installed on the remote server. Error was: " + ex.Message);
                    return null;
                }
            }
        }
    }
}
