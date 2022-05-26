using System;
using System.ComponentModel;
using System.Security;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Extensibility.Credentials;
using Inedo.Serialization;
using Inedo.Web;
using UsernamePasswordCredentials = Inedo.Extensions.Credentials.UsernamePasswordCredentials;

namespace Inedo.Extensions.Chocolatey.Configurations
{
    [Serializable]
    [DisplayName("Chocolatey Source")]
    public sealed class ChocolateySourceConfiguration : PersistedConfiguration, IExistential
    {
        [ConfigurationKey]
        [Required]
        [Persistent]
        [DisplayName("Source Name")]
        [ScriptAlias("Name")]
        public string Name { get; set; }

        [Required]
        [Persistent]
        [DisplayName("Endpoint URL")]
        [ScriptAlias("Url")]
        public string Url { get; set; }

        [Persistent]
        [DisplayName("Credential")]
        [ScriptAlias("Credential")]
        [IgnoreConfigurationDrift]
        public string CredentialName { get; set; }

        [Persistent]
        [DisplayName("User name")]
        [ScriptAlias("UserName")]
        public string UserName { get; set; }

        [Persistent(Encrypted = true)]
        [DisplayName("Password")]
        [ScriptAlias("Password")]
        public SecureString Password { get; set; }

        [Persistent]
        [DisplayName("Priority")]
        [ScriptAlias("Priority")]
        [Description("Lower priority numbers are more important. As a special case, 0 (the default) is the lowest priority.")]
        [DefaultValue(0)]
        public int Priority { get; set; } = 0;

        [Persistent]
        [DisplayName("Exists")]
        [ScriptAlias("Exists")]
        [DefaultValue(true)]
        public bool Exists { get; set; } = true;

        public bool Disabled { get; set; }

    }
}
