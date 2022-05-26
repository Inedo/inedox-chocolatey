using System.ComponentModel;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Serialization;

namespace Inedo.Extensions.Chocolatey.Configurations
{
    [DisplayName("Chocolatey Feature")]
    public sealed class ChocolateyFeatureConfiguration : PersistedConfiguration, IExistential
    {
        [ConfigurationKey]
        [Persistent]
        [DisplayName("Feature name")]
        [ScriptAlias("Feature")]
        public string Feature { get; set; }

        [DisplayName("Enabled")]
        [ScriptAlias("Enabled")]
        public bool Exists { get; set; } = true;
    }
}
