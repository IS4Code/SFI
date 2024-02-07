using System;
using System.Reflection;
using System.Text;

namespace IS4.SFI.Services
{
    using static UriTools;

    /// <summary>
    /// Provides support for formatting assemblies, namespaces, types, and members as URIs.
    /// </summary>
    public class ClrNamespaceUriFormatter : IIndividualUriFormatter<Assembly>, IIndividualUriFormatter<AssemblyName>, IIndividualUriFormatter<Namespace>, IIndividualUriFormatter<MemberInfo>, IIndividualUriFormatter<ParameterInfo>
    {
        /// <summary>
        /// The instance of the formatter.
        /// </summary>
        public static readonly ClrNamespaceUriFormatter Instance = new();

        ClrNamespaceUriFormatter()
        {

        }

        Uri IUriFormatter<Assembly>.this[Assembly value] => new(Namespace("", value).ToString());

        Uri IUriFormatter<AssemblyName>.this[AssemblyName value] => new(Namespace("", value).ToString());

        Uri IUriFormatter<Namespace>.this[Namespace value] => new(Namespace(value.FullName, value.Assembly).ToString());

        Uri IUriFormatter<MemberInfo>.this[MemberInfo value] {
            get {
                return new(Member(value).ToString());
            }
        }

        Uri? IUriFormatter<ParameterInfo>.this[ParameterInfo value] {
            get {
                var sb = Member(value.Member);
                sb.Append('/');
                sb.Append(value.Position);
                return new(sb.ToString());
            }
        }

        static StringBuilder Member(MemberInfo member)
        {
            var type = member.DeclaringType ?? (member as Type);
            var sb = Namespace(type?.Namespace ?? "", type?.Assembly);
            sb.Append('#');
            return TextTools.FormatMemberId(member, sb, MemberIdFormatOptions.UriEscaping | MemberIdFormatOptions.IncludeDeclaringMembers);
        }

        /// <summary>
        /// An internal marker type name that can be used to distinguish reference system assemblies from real ones.
        /// </summary>
        public const string ReferenceAssemblyMarkerClass = "IS4.SFI.<ReferenceAssembly>";

        static StringBuilder Namespace(string ns, Assembly? asm)
        {
            if(asm != null && asm.GetType(ReferenceAssemblyMarkerClass, false) == null)
            {
                // Assembly not marked as reference-only
                return Namespace(ns, asm.GetName());
            }
            return Namespace(ns, (AssemblyName?)null);
        }

        static StringBuilder Namespace(string ns, AssemblyName? asm)
        {
            var sb = new StringBuilder("clr-namespace:");
            sb.Append(EscapePathString(ns));
            if(asm != null)
            {
                sb.Append(";assembly=");
                sb.Append(EscapePathString(asm.Name));
            }
            return sb;
        }
    }
}
