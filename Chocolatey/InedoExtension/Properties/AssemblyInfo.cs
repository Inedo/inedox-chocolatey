using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Inedo.Extensibility;

[assembly: AssemblyTitle("Chocolatey")]
[assembly: AssemblyDescription("Contains operations for working with Chocolatey.")]
[assembly: AssemblyProduct("any")]
[assembly: AssemblyCompany("Inedo, LLC")]
[assembly: AssemblyCopyright("Copyright © Inedo 2024")]
[assembly: AssemblyVersion("3.1.2")]
[assembly: AssemblyFileVersion("3.1.0")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]

[assembly: ScriptNamespace("Chocolatey")]
[assembly: AppliesTo(InedoProduct.BuildMaster | InedoProduct.Otter)]
