using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Operations;
using Inedo.Serialization;

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Install Chocolatey Package")]
    [Description("Installs a Chocolatey package on a server.")]
    [ScriptAlias("Install-Package")]
    [DefaultProperty(nameof(PackageName))]
    [Tag("chocolatey")]
    [Obsolete]
    [Undisclosed]
    public sealed class InstallPackageOperation : ExecuteOperation
    {
        [Required]
        [Persistent]
        [ScriptAlias("Name")]
        [DisplayName("Package name")]
        public string PackageName { get; set; }

        [Persistent]
        [ScriptAlias("Version")]
        [DisplayName("Version")]
        [Description("The version number of the package to install. Leave blank for the latest version.")]
        public string Version { get; set; }

        [Persistent]
        [ScriptAlias("Source")]
        [DisplayName("Package source")]
        [Description("The source containing the package. Can be a NuGet repository or one of the alternative sources.")]
        [DefaultValue("https://chocolatey.org/api/v2")]
        public string Source { get; set; } = "https://chocolatey.org/api/v2";

        [Persistent]
        [ScriptAlias("AdditionalArguments")]
        [DisplayName("Additional arguments")]
        [Description("Arguments supplied here are passed directly to choco when a package is installed or upgraded.")]
        public string AdditionalArguments { get; set; }

        public override Task ExecuteAsync(IOperationExecutionContext context)
        {
            throw new NotImplementedException("Install-Package has been depricated.");
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(
                new RichDescription("Install Package (depricated)")
            );
        }
    }
}