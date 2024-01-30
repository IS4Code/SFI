using IS4.SFI.Services;
using System;
using System.Reflection;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Provides support for formatting assemblies and types as URIs.
    /// </summary>
    public class AssemblyMetadataItemUriFormatter : IIndividualUriFormatter<AssemblyName>, IIndividualUriFormatter<Type>
    {
        public static readonly AssemblyMetadataItemUriFormatter Instance = new();

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public AssemblyMetadataItemUriFormatter()
        {

        }

        Uri IUriFormatter<AssemblyName>.this[AssemblyName value] => new($"clr-namespace:;assembly={value.Name}");

        Uri IUriFormatter<Type>.this[Type value] => new($"clr-namespace:{value.Namespace};assembly={value.Assembly.GetName().Name}#{value.Name}");
    }
}
