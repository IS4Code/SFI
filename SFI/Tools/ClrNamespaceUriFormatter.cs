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

        Uri IUriFormatter<Assembly>.this[Assembly value] => new(Namespace("", value.GetName()));

        Uri IUriFormatter<AssemblyName>.this[AssemblyName value] => new(Namespace("", value));

        Uri IUriFormatter<Namespace>.this[Namespace value] => new(Namespace(value.FullName, value.Assembly.GetName()));

        Uri IUriFormatter<MemberInfo>.this[MemberInfo value] {
            get {
                if (value is Type { DeclaringType: null } type)
                {
                    // Non-nested type
                    return new(Type(type));
                }
                if (value is Type { IsGenericParameter: true, DeclaringMethod: { } method })
                {
                    // Type parameter
                    return new($"{Member(method)}/{EscapeFragmentString(value.Name)}");
                }
                return new(Member(value));
            }
        }

        static string Namespace(string ns, AssemblyName asm)
        {
            return $"clr-namespace:{EscapePathString(ns)};assembly={EscapePathString(asm.Name)}";
        }

        static string Type(Type type)
        {
            if(type.DeclaringType != null)
            {
                return Member(type);
            }
            return $"{Namespace(type.Namespace ?? "", type.Assembly.GetName())}#{EscapeFragmentString(type.Name)}";
        }

        static string Member(MemberInfo member)
        {
            return $"{Type(member.DeclaringType)}.{EscapeFragmentString(member.Name)}";
        }
    }
}
