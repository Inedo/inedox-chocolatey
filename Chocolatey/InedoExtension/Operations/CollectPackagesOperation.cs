using System.ComponentModel;
using System.Threading.Tasks;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Extensibility.Operations;

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Collect Chocolatey Packages")]
    [Description("Collects the names and versions of chocolatey packages installed on a server.")]
    [ScriptAlias("Collect-Packages")]
    [Tag("chocolatey")]
    public sealed class CollectPackagesOperation : CollectOperation<DictionaryConfiguration>
    {
        public async override Task<DictionaryConfiguration> CollectConfigAsync(IOperationCollectionContext context)
        {
            using (var serverContext = context.GetServerCollectionContext())
            {
                var output = await this.ExecuteChocolateyAsync(context, "list --limit-output --local-only").ConfigureAwait(false);

                if (output == null)
                    return null;

                await serverContext.ClearAllPackagesAsync("Chocolatey").ConfigureAwait(false);

                foreach (var values in output)
                {
                    string name = values[0];
                    string version = values[1];

                    await serverContext.CreateOrUpdatePackageAsync(
                        packageType: "Chocolatey",
                        packageName: name,
                        packageVersion: version,
                        packageUrl: null
                    ).ConfigureAwait(false);
                }

                return null;
            }
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(
                new RichDescription("Collect Chocolatey Packages")
            );
        }
    }
}
