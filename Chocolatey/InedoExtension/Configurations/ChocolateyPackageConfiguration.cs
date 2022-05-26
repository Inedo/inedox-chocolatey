using System;
using System.ComponentModel;
using System.Security;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Extensions.Chocolatey.Credentials;
using Inedo.Extensions.Chocolatey.SuggestionProviders;
using Inedo.Serialization;
using Inedo.Web;

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

        [DisplayName("From resource")]
        [ScriptAlias("From")]
        [ScriptAlias("Credentials")]
        [SuggestableValue(typeof(SecureResourceSuggestionProvider<ChocolateySourceSecureResource>))]
        [IgnoreConfigurationDrift]
        [Category("Connection/Identity")]
        public string ResourceName { get; set; }

        [Persistent]
        [ScriptAlias("Source")]
        [Description("The value to specify for --source parameter in choco install.  This can be a special source or a URL to a Chocolatey repository.")]
        [SuggestableValue(typeof(SpecialSourceSuggestionProvider))]
        [IgnoreConfigurationDrift]
        [Category("Connection/Identity")]
        public string Source { get; set; }

        [Persistent]
        [IgnoreConfigurationDrift]
        [DisplayName("User name")]
        [ScriptAlias("UserName")]
        [Category("Connection/Identity")]
        public string UserName { get; set; }

        [Persistent(Encrypted = true)]
        [IgnoreConfigurationDrift]
        [DisplayName("Password")]
        [ScriptAlias("Password")]
        [Category("Connection/Identity")]
        public SecureString Password { get; set; }

        [Persistent]
        [IgnoreConfigurationDrift]
        [ScriptAlias("AdditionalInstallArguments")]
        [DisplayName("Additional install arguments")]
        [Description("Arguments supplied here are passed directly to choco when a package is installed or upgraded.")]
        [Category("Advanced")]
        public string AdditionalInstallArguments { get; set; }
    }
}
