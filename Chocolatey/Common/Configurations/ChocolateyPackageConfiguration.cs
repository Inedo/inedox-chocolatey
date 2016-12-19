using System;
using System.ComponentModel;
using Inedo.Documentation;
using Inedo.Serialization;
#if Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Configurations;
#elif BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Configurations;
#endif

namespace Inedo.Extensions.Chocolatey.Configurations
{
    [Serializable]
    [DisplayName("Chocolatey Package")]
    public sealed class ChocolateyPackageConfiguration : PersistedConfiguration, IExistential
    {
        [Required]
        [Persistent]
        [ConfigurationKey]
        [ScriptAlias("Name")]
        [DisplayName("Package name")]
        public string PackageName { get; set; }

        [Persistent]
        [ScriptAlias("Version")]
        [Description("The version number of the package to install. Leave blank for the latest version.")]
        public string Version { get; set; }

        [Persistent]
        public bool IsLatestVersion { get; set; }

        [Persistent]
        [ScriptAlias("Exists")]
        [DefaultValue(true)]
        public bool Exists { get; set; } = true;

        [Persistent]
        [ScriptAlias("Source")]
        [ConfigurationKey]
        [DefaultValue("https://chocolatey.org/api/v2")]
        public string Source { get; set; } = "https://chocolatey.org/api/v2";
    }
}
