using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Extensions.Chocolatey.SuggestionProviders;
using Inedo.Serialization;
using Inedo.Web;
using System;
using System.ComponentModel;

namespace Inedo.Extensions.Chocolatey.Configurations
{
    [Serializable]
    [DisplayName("Chocolatey")]
    public sealed class ChocolateyInstalledConfiguration : PersistedConfiguration
    {
        public override string ConfigurationKey => "Chocolatey-Installed";

        [Persistent]
        [ScriptAlias("Version")]
        [Description("The version number of the Chocolatey to install. Also accepts \"latest\" to always use the latest version. Leave blank to only ensure Chocolatey is installed but not which version.")]
        [SuggestableValue(typeof(ChocolateyVersionSuggestionProvider))]
        public string Version { get; set; }

        public string LatestVersion { get; set; }

        [Persistent]
        [ScriptAlias("Source")]
        [DefaultValue("https://chocolatey.org/api/v2")]
        [SuggestableValue(typeof(SpecialSourceSuggestionProvider))]
        [IgnoreConfigurationDrift]
        public string Source { get; set; } = "https://chocolatey.org/api/v2";
    }
}
