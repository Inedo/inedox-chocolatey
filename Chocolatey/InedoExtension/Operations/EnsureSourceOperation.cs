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
    public sealed class EnsureSourceOperation : EnsureOperation<ChocolateySourceConfiguration>
    {
        private ChocolateySourceConfiguration Collected { get; set; }

        public override async Task<PersistedConfiguration> CollectAsync(IOperationCollectionContext context)
        {
            return await this.CollectAsync((IOperationExecutionContext)context);
        }
        private async Task<ChocolateySourceConfiguration> CollectAsync(IOperationExecutionContext context)
        {
            this.Template.SetCredentialProperties((ICredentialResolutionContext)context);

            if (this.Collected != null)
            {
                return this.Collected;
            }

            var output = await this.ExecuteChocolateyAsync(context, "source list --limit-output");
            var source = output.Find(s => string.Equals(s[0], this.Template.Name, StringComparison.OrdinalIgnoreCase));
            if (source == null)
            {
                this.Collected = new ChocolateySourceConfiguration
                {
                    Name = this.Template.Name,
                    Exists = false
                };
            }
            else
            {
                this.Collected = new ChocolateySourceConfiguration
                {
                    Name = this.Template.Name,
                    Url = source[1],
                    UserName = AH.NullIf(source[3], string.Empty),
                    Priority = int.Parse(source[5]),
                    Exists = true,
                    Disabled = bool.Parse(source[2])
                };
            }

            return this.Collected; 
        }

        public override Task<ComparisonResult> CompareAsync(PersistedConfiguration other, IOperationCollectionContext context)
        {

            this.Template.SetCredentialProperties((ICredentialResolutionContext)context);
            var actual = (ChocolateySourceConfiguration)other;
            if (this.Template.Exists != actual.Exists)
                return Task.FromResult(new ComparisonResult(new[] { new Difference(nameof(this.Template.Exists), this.Template.Exists, actual.Exists) }));

            if (!this.Template.Exists)
                return Task.FromResult(ComparisonResult.Identical);

            var differences = new List<Difference>();

            if (this.Template.Url != actual.Url)
                differences.Add(new Difference(nameof(this.Template.Url), this.Template.Url, actual.Url));
            if (this.Template.UserName != actual.UserName)
                differences.Add(new Difference(nameof(this.Template.UserName), this.Template.UserName, actual.UserName));
            // Can't check password
            if (this.Template.Priority != actual.Priority)
                differences.Add(new Difference(nameof(this.Template.Priority), this.Template.Priority, actual.Priority));
            if (actual.Disabled)
                differences.Add(new Difference(nameof(this.Template.Disabled), false, true));

            return Task.FromResult(new ComparisonResult(differences));
        }

        public override async Task ConfigureAsync(IOperationExecutionContext context)
        {

            this.Template.SetCredentialProperties((ICredentialResolutionContext)context);
            var buffer = new StringBuilder(200);

            if (this.Template.Exists)
            {
                var collected = await this.CollectAsync(context);
                if (collected.Disabled)
                {
                    this.LogDebug("Enabling source...");
                    if (context.Simulation)
                    {
                        this.LogDebug("Updating source...");
                        return;
                    }

                    await this.ExecuteCommandLineAsync(context, new RemoteProcessStartInfo
                    {
                        FileName = "choco",
                        Arguments = $"source enable --name=\"{this.Template.Name}\""
                    });
                }

                this.LogDebug($"{(collected.Exists ? "Updating" : "Adding")} source...");
                buffer.Append("source add --name=\"");
                buffer.Append(this.Template.Name);
                buffer.Append("\" --source=\"");
                buffer.Append(this.Template.Url);
                buffer.Append("\" ");

                if (!string.IsNullOrEmpty(this.Template.UserName))
                {
                    buffer.Append("--user=\"");
                    buffer.Append(this.Template.UserName);
                    buffer.Append("\" ");
                }
                if (this.Template.Password?.Length > 0)
                {
                    buffer.Append("--password=\"");
                    buffer.Append(AH.Unprotect(this.Template.Password));
                    buffer.Append("\" ");
                }

                buffer.Append("--priority=");
                buffer.Append(this.Template.Priority);
            }
            else
            {
                this.LogDebug("Removing source...");

                buffer.Append("source remove --name=\"");
                buffer.Append(this.Template.Name);
                buffer.Append("\"");
            }

            if (context.Simulation)
            {
                return;
            }

            await this.ExecuteCommandLineAsync(context, new RemoteProcessStartInfo
            {
                FileName = "choco",
                Arguments = buffer.ToString()
            });
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(new RichDescription("Ensure Chocolatey source ", new Hilite(config[nameof(this.Template.Name)])));
        }
    }
}
