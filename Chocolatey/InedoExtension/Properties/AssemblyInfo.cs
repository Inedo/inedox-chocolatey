using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Inedo.Extensibility;

[assembly: AssemblyTitle("Chocolatey")]
[assembly: AssemblyDescription("Contains operations for working with Chocolatey.")]
[assembly: AssemblyProduct("any")]
[assembly: AssemblyCompany("Inedo, LLC")]
[assembly: AssemblyCopyright("Copyright © Inedo 2021")]
[assembly: AssemblyVersion("1.10.0")]
[assembly: AssemblyFileVersion("1.10.0")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]

[assembly: ScriptNamespace("Chocolatey")]
[assembly: AppliesTo(InedoProduct.BuildMaster | InedoProduct.Otter)]
