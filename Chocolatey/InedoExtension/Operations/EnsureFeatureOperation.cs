using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Extensibility.Operations;
using Inedo.Extensions.Chocolatey.Configurations;

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Ensure Chocolatey Feature")]
    [Description("Ensure a feature in Chocolatey is enabled or disabled.")]
    [ScriptAlias("Ensure-Feature")]
    [Tag("chocolatey")]
    [Obsolete]
    [Undisclosed]
    public sealed class EnsureFeatureOperation : EnsureOperation<ChocolateyFeatureConfiguration>
    {
        public override Task<PersistedConfiguration> CollectAsync(IOperationCollectionContext context)
        {
            throw new InvalidOperationException("Ensure-feature has been deprecated.");
        }

        public override Task ConfigureAsync(IOperationExecutionContext context)
        {
            throw new InvalidOperationException("Ensure-feature has been deprecated.");
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(
                new RichDescription("Ensure Chocolatey feature (deprecated)")
            );
        }
    }
}
