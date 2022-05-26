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
using Inedo.Extensions.Chocolatey.Configurations;

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Ensure Chocolatey Source")]
    [Description("Ensure a source is configured in Chocolatey.")]
    [ScriptAlias("Ensure-Source")]
    [Tag("chocolatey")]
    [Obsolete]
    [Undisclosed]
    public sealed class EnsureSourceOperation : EnsureOperation<ChocolateySourceConfiguration>
    {
        private ChocolateySourceConfiguration Collected { get; set; }

        public override Task<PersistedConfiguration> CollectAsync(IOperationCollectionContext context)
        {
            throw new NotImplementedException("Ensure-Source has been depricated.");
        }

        public override Task<ComparisonResult> CompareAsync(PersistedConfiguration other, IOperationCollectionContext context)
        {

            throw new NotImplementedException("Ensure-Source has been depricated.");
        }

        public override Task ConfigureAsync(IOperationExecutionContext context)
        {

            throw new NotImplementedException("Ensure-Source has been depricated.");
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(new RichDescription("Ensure Chocolatey source (debrecated)"));
        }
    }
}
