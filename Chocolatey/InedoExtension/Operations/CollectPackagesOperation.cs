using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Extensibility.Operations;
using Inedo.Extensions.Chocolatey.Configurations;

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Collect Chocolatey Packages")]
    [Description("Collects the names and versions of chocolatey packages installed on a server.")]
    [ScriptAlias("Collect-Packages")]
    [Tag("chocolatey")]
    public sealed class CollectPackagesOperation : Extensibility.Operations.CollectPackagesOperation
    {
        public override string PackageType => "Chocolatey";

        protected async override Task<IEnumerable<PackageConfiguration>> CollectPackagesAsync(IOperationCollectionContext context)
        {
            var packages = new List<PackageConfiguration>();
            var output = await this.ExecuteChocolateyAsync(context, "list --limit-output --local-only").ConfigureAwait(false);

            if (output == null)
                return null;

            foreach (var values in output)
            {
                string name = values[0];
                string version = values[1];

                packages.Add(new ChocolateyPackageCollectionConfiguration
                {
                    PackageName = name,
                    PackageVersion = version
                });
            }

            return packages;
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(
                new RichDescription("Collect Chocolatey Packages")
            );
        }
    }
}
