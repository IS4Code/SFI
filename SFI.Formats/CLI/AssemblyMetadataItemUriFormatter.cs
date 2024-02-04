using System;
using System.Reflection;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Provides support for formatting assemblies and types as URIs.
    /// </summary>
    public class AssemblyMetadataItemUriFormatter : IIndividualUriFormatter<Assembly>, IIndividualUriFormatter<AssemblyName>, IIndividualUriFormatter<Namespace>, IIndividualUriFormatter<Type>
    {
        public static readonly AssemblyMetadataItemUriFormatter Instance = new();

        AssemblyMetadataItemUriFormatter()
        {

        }

        Uri IUriFormatter<Assembly>.this[Assembly value] => new($"clr-namespace:;assembly={value.GetName().Name}");

        Uri IUriFormatter<AssemblyName>.this[AssemblyName value] => new($"clr-namespace:;assembly={value.Name}");

        Uri IUriFormatter<Namespace>.this[Namespace value] => new($"clr-namespace:{value.FullName};assembly={value.Assembly.GetName().Name}");

        Uri IUriFormatter<Type>.this[Type value] => new($"clr-namespace:{value.Namespace};assembly={value.Assembly.GetName().Name}#{value.Name}");
    }
}
