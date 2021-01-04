using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inedo.Extensibility;
using Inedo.Extensibility.Configurations;
using Inedo.Serialization;

namespace Inedo.Extensions.Chocolatey.Configurations
{
    /// <summary>
    /// Provides additional metadata for installed Chocolatey packages.
    /// </summary>
    [Serializable]
    [SlimSerializable]
    [ScriptAlias("Chocolatey")]
    public sealed class ChocolateyPackageCollectionConfiguration : PackageConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChocolateyPackageCollectionConfiguration"/> class.
        /// </summary>
        public ChocolateyPackageCollectionConfiguration()
        {
        }
    }
}
