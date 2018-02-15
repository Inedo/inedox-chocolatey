using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Inedo.Agents;
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.Serialization;
using Inedo.Extensions.Chocolatey.SuggestionProviders;
#if Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Operations;
using Inedo.Otter.Web.Controls;
using SuggestableValueAttribute = Inedo.Otter.Web.Controls.SuggestibleValueAttribute;
#elif BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMaster.Web.Controls;
using SuggestableValueAttribute = Inedo.BuildMaster.Web.Controls.SuggestibleValueAttribute;
#else
using Inedo.Extensibility;
using Inedo.Extensibility.Operations;
using Inedo.Web;
#endif

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Install Chocolatey Package")]
    [Description("Installs a Chocolatey package on a server.")]
    [ScriptNamespace("Chocolatey")]
    [ScriptAlias("Install-Package")]
    [DefaultProperty(nameof(PackageName))]
    [Tag("chocolatey")]
    public sealed class InstallPackageOperation : ExecuteOperation
    {
        [Required]
        [Persistent]
        [ScriptAlias("Name")]
        [DisplayName("Package name")]
        [SuggestableValue(typeof(PackageNameSuggestionProvider))]
        public string PackageName { get; set; }

        [Persistent]
        [ScriptAlias("Version")]
        [DisplayName("Version")]
        [Description("The version number of the package to install. Leave blank for the latest version.")]
        [SuggestableValue(typeof(VersionSuggestionProvider))]
        public string Version { get; set; }

        [Persistent]
        [ScriptAlias("Source")]
        [DisplayName("Package source")]
        [Description("The source containing the package. Can be a NuGet repository or one of the alternative sources.")]
        [DefaultValue("https://chocolatey.org/api/v2")]
        [SuggestableValue(typeof(SpecialSourceSuggestionProvider))]
        public string Source { get; set; } = "https://chocolatey.org/api/v2";

        public override async Task ExecuteAsync(IOperationExecutionContext context)
        {
            var buffer = new StringBuilder("upgrade --yes --fail-on-unfound ", 200);

            if (context.Simulation)
                buffer.Append("--what-if ");

            if (!string.IsNullOrEmpty(this.Version))
            {
                buffer.Append("--version \"");
                buffer.Append(this.Version);
                buffer.Append("\" ");
                buffer.Append("--allow-downgrade ");
            }

            if (!string.IsNullOrEmpty(this.Source))
            {
                buffer.Append("--source \"");
                buffer.Append(this.Source);
                buffer.Append("\" ");
            }

            buffer.Append('\"');
            buffer.Append(this.PackageName);
            buffer.Append('\"');

            int exitCode = await this.ExecuteCommandLineAsync(
                context,
                new RemoteProcessStartInfo
                {
                    FileName = "choco",
                    Arguments = buffer.ToString()
                }
            ).ConfigureAwait(false);

            if (exitCode != 0)
                this.LogError("Process exited with code " + exitCode);
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            if (string.IsNullOrEmpty(this.Version))
            {
                return new ExtendedRichDescription(
                    new RichDescription(
                        "Install latest version of ",
                        new Hilite(config[nameof(this.PackageName)]),
                        " from Chocolatey"
                    )
                );
            }

            return new ExtendedRichDescription(
                new RichDescription(
                    "Install version ",
                    new Hilite(config[nameof(this.Version)]),
                    " of ",
                    new Hilite(config[nameof(this.PackageName)]),
                    " from Chocolatey"
                )
            );
        }
    }
}