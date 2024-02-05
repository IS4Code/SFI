using System;
using System.Reflection;

namespace IS4.SFI.Services
{
    using static UriTools;

    /// <summary>
    /// Provides support for formatting assemblies, namespaces, types, and members as URIs.
    /// </summary>
    public class ClrNamespaceUriFormatter : IIndividualUriFormatter<Assembly>, IIndividualUriFormatter<AssemblyName>, IIndividualUriFormatter<Namespace>, IIndividualUriFormatter<MemberInfo>
    {
        /// <summary>
        /// The instance of the formatter.
        /// </summary>
        public static readonly ClrNamespaceUriFormatter Instance = new();

        ClrNamespaceUriFormatter()
        {

        }

        Uri IUriFormatter<Assembly>.this[Assembly value] => new(Namespace("", value.GetName().Name));

        Uri IUriFormatter<AssemblyName>.this[AssemblyName value] => new(Namespace("", value.Name));

        Uri IUriFormatter<Namespace>.this[Namespace value] => new(Namespace(value.FullName, value.Assembly.GetName().Name));

        Uri IUriFormatter<MemberInfo>.this[MemberInfo value] {
            get {
                if (value is Type { DeclaringType: null } type)
                {
                    return new($"{Namespace(type.Namespace, type.Assembly.GetName().Name)}#{EscapeFragmentString(type.Name)}");
                }
                type = value.DeclaringType;
                return new($"{Namespace(type.Namespace, type.Assembly.GetName().Name)}#{EscapeFragmentString(type.Name)}.{EscapeFragmentString(value.Name)}");
            }
        }

        static string Namespace(string ns, string asm)
        {
            return $"clr-namespace:{EscapePathString(ns)};assembly={EscapeQueryString(asm)}";
        }
    }
}
