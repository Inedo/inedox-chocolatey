using System;
using System.ComponentModel;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Serialization;

namespace Inedo.Extensions.Chocolatey.Configurations
{
    [Serializable]
    [DisplayName("Chocolatey")]
    public sealed class ChocolateyInstalledConfiguration : PersistedConfiguration
    {
        public override string ConfigurationKey => "Chocolatey-Installed";

        [Persistent]
        public bool Exists { get; set; }

        [Persistent]
        [DisplayName("Install Script Url")]
        [ScriptAlias("InstallScriptUrl")]
        [Description("URL for the Chocolatey install PowerShell script.  The default is https://community.chocolatey.org/install.ps1")]
        [DefaultValue("https://community.chocolatey.org/install.ps1")]
        [IgnoreConfigurationDrift]
        public string InstallScriptUrl { get; set; } = "https://community.chocolatey.org/install.ps1";
    }
}
