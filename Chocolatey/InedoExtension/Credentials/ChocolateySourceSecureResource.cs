using System;
using System.ComponentModel;
using Inedo.Documentation;
using Inedo.Extensibility.SecureResources;
using Inedo.Extensions.Credentials;
using Inedo.Serialization;


namespace Inedo.Extensions.Chocolatey.Credentials
{
    [DisplayName("Chocolatey Package Source")]
    [Description("Connect to a Chocolatey package feed")]
    public sealed class ChocolateySourceSecureResource : SecureResource<UsernamePasswordCredentials>
    {
        [Required]
        [Persistent]
        [DisplayName("Source URL")]
        public string SourceUrl { get; set; }

        public override RichDescription GetDescription()
        {
            var host = AH.CoalesceString(this.SourceUrl, "(unknown)");
            if (!string.IsNullOrWhiteSpace(this.SourceUrl) && Uri.TryCreate(this.SourceUrl, UriKind.Absolute, out var uri))
                host = uri.Host;

            return new RichDescription(host);
        }
    }
}
