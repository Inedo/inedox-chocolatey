using System;
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
    [DisplayName("Ensure Chocolatey Feature")]
    [Description("Ensure a feature in Chocolatey is enabled or disabled.")]
    [ScriptAlias("Ensure-Feature")]
    [Tag("chocolatey")]
    public sealed class EnsureFeatureOperation : EnsureOperation<ChocolateyFeatureConfiguration>
    {
        public override async Task<PersistedConfiguration> CollectAsync(IOperationCollectionContext context)
        {
            var features = await this.ExecuteChocolateyAsync(context, "feature list --limit-output");
            var feature = features.Find(f => string.Equals(f[0], this.Template.Feature, StringComparison.OrdinalIgnoreCase));

            if (feature == null)
            {
                this.LogError($"No such chocolatey feature: {this.Template.Feature}");
                return new ChocolateyFeatureConfiguration();
            }

            return new ChocolateyFeatureConfiguration
            {
                Feature = feature[0],
                Exists = AH.Switch<string, bool>(feature[1])
                    .Case("Enabled", true)
                    .Case("Disabled", false)
                    .End()
            };
        }

        public override async Task ConfigureAsync(IOperationExecutionContext context)
        {
            var buffer = new StringBuilder(200);
            buffer.Append("feature ");
            buffer.Append(this.Template.Exists ? "enable" : "disable");
            buffer.Append(" --name=\"");
            buffer.Append(this.Template.Feature);
            buffer.Append("\"");

            this.LogDebug($"{(this.Template.Exists ? "Enabling" : "Disabling")} Chocolatey feature \"{this.Template.Feature}\"...");
            if (context.Simulation)
                return;

            await this.ExecuteCommandLineAsync(context, new RemoteProcessStartInfo
            {
                FileName = "choco",
                Arguments = buffer.ToString()
            });
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            var details = new RichDescription();
            if (bool.TryParse(config[nameof(this.Template.Exists)], out var enabled))
            {
                details.AppendContent("is ", new Hilite(enabled ? "enabled" : "disabled"));
            }

            return new ExtendedRichDescription(
                new RichDescription("Ensure Chocolatey feature ", new Hilite(config[nameof(this.Template.Feature)])),
                details
            );
        }
    }
}
