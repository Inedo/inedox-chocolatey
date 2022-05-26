using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Inedo.Agents;
using Inedo.Diagnostics;
using Inedo.Extensibility.Operations;

namespace Inedo.Extensions.Chocolatey
{
    internal static class Extensions
    {
        public static async Task<List<string[]>> ExecuteChocolateyAsync(this Operation operation, IOperationExecutionContext context, string args, bool missingChocolateyOk = false)
        {
            var agent = await context.Agent.GetServiceAsync<IRemoteProcessExecuter>().ConfigureAwait(false);
            var fileOps = await context.Agent.GetServiceAsync<IFileOperationsExecuter>().ConfigureAwait(false);
            
            var installLocation = AH.CoalesceString(await agent.GetEnvironmentVariableValueAsync("ChocolateyInstall"), @"C:\ProgramData\chocolatey");
            var chocoExistsAtInstallLocation = await fileOps.FileExistsAsync(Path.Combine(installLocation, "choco.exe"));
            
            var startInfo = new RemoteProcessStartInfo { FileName = "choco.exe", Arguments = args };

            if (chocoExistsAtInstallLocation)
                startInfo.FileName = Path.Combine(installLocation, "choco.exe");

            using (var process = agent.CreateProcess(startInfo))
            {
                var output = new List<string[]>();

                process.OutputDataReceived +=
                    (s, e) =>
                    {
                        operation.LogDebug(e.Data);
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
                            operation.LogError(e.Data);
                        }
                    };

                try
                {
                    await process.StartAsync(context.CancellationToken);
                    await process.WaitAsync(context.CancellationToken).ConfigureAwait(false);
                    if (process.ExitCode != 0)
                    {
                        operation.LogError("Chocolatey returned exit code " + process.ExitCode);
                        return null;
                    }
                    else if (error)
                    {
                        return null;
                    }

                    return output;
                }
                catch (AggregateException aex) when (aex.InnerException is Exception iox)
                {
                    logChocolateyException(iox);
                    return null;
                }
                catch (Exception iox)
                {
                    logChocolateyException(iox);
                    return null;
                }

                void logChocolateyException(Exception wex)
                {
                    if (missingChocolateyOk)
                        return;

                    operation.LogError(
                        "There was an error executing chocolatey. Ensure that chocolatey is installed on the remote server, and that 'choco.exe' is in the PATH. "
                      + "If chocolatey has been installed while the Inedo Agent was already running, the Inedo Agent service should be restarted. ");
                    operation.LogDebug("The underlying error was: " + wex.Message);
                }
            }
        }
    }
}
