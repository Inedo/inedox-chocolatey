using System;
using System.ComponentModel;
using Inedo.Documentation;
using Inedo.Serialization;
using Inedo.Extensions.Chocolatey.SuggestionProviders;
#if Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Configurations;
using SuggestableValueAttribute = Inedo.Otter.Web.Controls.SuggestibleValueAttribute;
#elif BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Configurations;
using SuggestableValueAttribute = Inedo.BuildMaster.Web.Controls.SuggestibleValueAttribute;
#else
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Web;
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
        [SuggestableValue(typeof(PackageNameSuggestionProvider))]
        public string PackageName { get; set; }

        [Persistent]
        [ScriptAlias("Version")]
        [Description("The version number of the package to install. Leave blank for the latest version.")]
        [SuggestableValue(typeof(VersionSuggestionProvider))]
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
        [SuggestableValue(typeof(SpecialSourceSuggestionProvider))]
        public string Source { get; set; } = "https://chocolatey.org/api/v2";
    }
}
