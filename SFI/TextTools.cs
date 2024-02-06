using IS4.SFI.Services;
using IS4.SFI.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace IS4.SFI
{
    /// <summary>
    /// Contains various utility methods for manipulating text or producing human-readable labels.
    /// </summary>
    public static class TextTools
    {
        static readonly string[] units = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

        /// <summary>
        /// Creates a human-friendly size string using standard 1024-based size units.
        /// </summary>
        /// <param name="value">The size to be formatted.</param>
        /// <param name="decimalPlaces">How many decimal places to include in the size.</param>
        /// <returns>The size formatted as "[-]{value} {unit_prefix}B"</returns>
        public static string SizeSuffix(long value, int decimalPlaces)
        {
            if(value < 0) return "-" + SizeSuffix(-value, decimalPlaces);
            if(value == 0) return String.Format(CultureInfo.InvariantCulture, $"{{0:0.{new string('#', decimalPlaces)}}} B", 0);

            int n = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (n * 10));
            if(Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                n += 1;
                adjustedSize /= 1024;
            }
            return String.Format(CultureInfo.InvariantCulture, $"{{0:0.{new string('#', decimalPlaces)}}} {{1}}", adjustedSize, units[n]);
        }

        /// <summary>
        /// The prefix used for implied media types.
        /// </summary>
        public static readonly string ImpliedMediaTypePrefix = "application/prs.implied-";

        /// <summary>
        /// Creates an implied media type from a namespace URI, <c>PUBLIC</c> identifier,
        /// and the root element name in an XML document.
        /// </summary>
        /// <param name="ns">The root namespace URI (may be <see langword="null"/>).</param>
        /// <param name="publicId">The <c>PUBLIC</c> identifier (may be <see langword="null"/>).</param>
        /// <param name="rootName">The name of the root element.</param>
        /// <returns>A MIME type in the form of
        /// <c>application/prs.implied-document+xml;root={<paramref name="rootName"/>};ns={<paramref name="ns"/>};public={<paramref name="publicId"/>}</c>.
        /// </returns>
        public static string GetImpliedMediaTypeFromXml(Uri? ns, string? publicId, string rootName)
        {
            var sb = new StringBuilder("application/prs.implied-document+xml;root=");
            sb.Append(FormatMimeParameter(rootName));
            if(ns != null)
            {
                sb.Append(";ns=");
                sb.Append(FormatMimeParameter(ns.AbsoluteUri));
            }
            if(publicId != null)
            {
                sb.Append(";public=");
                sb.Append(FormatMimeParameter(publicId));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Creates a dummy media type from a .NET type.
        /// </summary>
        /// <typeparam name="T">The type to use for the media type.</typeparam>
        /// <returns>A MIME type in the form of
        /// <c>application/octet-stream;type={name}</c>,
        /// where name is the result of <see cref="GetIdentifierFromType{T}"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDummyMediaTypeFromType<T>()
        {
            return DummyTypeNameCache<T>.Name;
        }

        /// <summary>
        /// Creates an (MIME-safe) identifier from a .NET type.
        /// </summary>
        /// <typeparam name="T">The type to use for the identifier.</typeparam>
        /// <returns>A concatenation of the name of the type and names of all
        /// its generic arguments.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetIdentifierFromType<T>()
        {
            return DummyTypeNameCache<T>.ShortName;
        }

        /// <inheritdoc cref="GetIdentifierFromType{T}"/>
        /// <param name="type">The type to use for the identifier.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetIdentifierFromType(Type type)
        {
            return DummyTypeNameCache<object>.GetTypeFriendlyName(type);
        }

        /// <summary>
        /// Matches a letter after which a hyphen could be placed, as either a lowercase letter followed
        /// by an uppercase letter, or an uppercase letter or digit followed by an uppercase letter and a lowercase letter.
        /// </summary>
        static readonly Regex hyphenCharacters = new(@"\p{Ll}(?=\p{Lu})|[\p{Lu}\p{N}](?=\p{Lu}\p{Ll})", RegexOptions.Compiled);

        /// <summary>
        /// Matches any namespace that is located within <see cref="System"/> or <see cref="SFI"/>.
        /// </summary>
        static readonly Regex friendNamespace = new($@"^(?:{nameof(System)}|{nameof(IS4)}\.{nameof(SFI)})(?:$|\.)", RegexOptions.Compiled);

        /// <summary>
        /// Matches a letter I followed a capital letter, denoting interfaces by convention.
        /// </summary>
        static readonly Regex interfaceLetter = new(@"^I(?=\p{Lu})", RegexOptions.Compiled);

        class DummyTypeNameCache<T>
        {
            public static readonly string ShortName = GetTypeFriendlyName(typeof(T));
            public static readonly string Name = "application/octet-stream;type=" + FormatMimeParameter(ShortName);

            public static string GetTypeFriendlyName(Type type)
            {
                // Strip leading I for interfaces
                string name = type.IsInterface ? interfaceLetter.Replace(type.Name, "") : type.Name;
                var components = new List<string>();
                var genArgs = type.GetGenericArguments();
                int ownArgsStart = 0;
                if(type.IsNested && !type.IsGenericParameter)
                {
                    // Always include the name of the declaring type
                    var declaringType = type.DeclaringType;
                    if(declaringType.IsGenericTypeDefinition && type.IsGenericType)
                    {
                        // Nested type inherits generic parameters from declaring type
                        var genParams = declaringType.GetGenericArguments();
                        if(genArgs.Length > 0)
                        {
                            ownArgsStart = Math.Min(genArgs.Length, genParams.Length);
                            Array.Copy(genArgs, 0, genParams, 0, ownArgsStart);
                            declaringType = declaringType.MakeGenericType(genParams);
                        }
                    }
                    components.Add(GetTypeFriendlyName(declaringType));
                }else{
                    if(!String.IsNullOrEmpty(type.Namespace))
                    {
                        // Get all similarly named visible types in the assembly
                        var similarTypes = GetExportedTypes(type.Assembly).Where(t => t.Name.Equals(type.Name, StringComparison.OrdinalIgnoreCase) && !t.Equals(type));
                        // Get the length of the namespace prefix shared by all these types
                        int prefix = similarTypes.Select(t => CommonPrefix(t.Namespace, type.Namespace)).DefaultIfEmpty(type.Namespace.Length).Max();
                        // Prepend the determining part of the namespace to the name
                        name = (prefix == type.Namespace.Length ? "" : type.Namespace.Substring(prefix)) + name;
                    }
                    // Produce components from the encoded name and its generic arguments
                    if(!friendNamespace.IsMatch(type.Namespace))
                    {
                        // If this is in an external assembly, distinguish it by the assembly name
                        components.Add(FormatComponentName(type.Assembly.GetName().Name));
                    }
                }
                // Strip the arity
                int index = name.IndexOf('`');
                if(index != -1) name = name.Substring(0, index);
                // Add the base name and generic arguments
                components.Add(FormatComponentName(name));
                components.AddRange(genArgs.Skip(ownArgsStart).Select(GetTypeFriendlyName));
                return String.Join(".", components);
            }
        }

        static readonly ConditionalWeakTable<Assembly, Type[]> exportedTypesCache = new();

        /// <summary>
        /// Retrieves the collection of exported types in an assembly, with caching.
        /// </summary>
        /// <param name="asm">The <see cref="Assembly"/> instance to use.</param>
        /// <returns>A collection of <see cref="Assembly.ExportedTypes"/>.</returns>
        static IEnumerable<Type> GetExportedTypes(Assembly asm)
        {
            return exportedTypesCache.GetValue(asm, a => {
                try{
                    return a.GetExportedTypes();
                }catch{
                    return Array.Empty<Type>();
                }
            });
        }

        /// <summary>
        /// Returns the length of the common prefix of <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        static int CommonPrefix(string a, string b)
        {
            int max = Math.Min(a.Length, b.Length);
            for(int i = 0; i < max; i++)
            {
                if(a[i] != b[i]) return i;
            }
            return max;
        }

        /// <summary>
        /// Produces a component name by hyphenating the name and converting to lowercase.
        /// </summary>
        /// <param name="name">The name to format.</param>
        /// <returns>The resulting formatted name.</returns>
        public static string FormatComponentName(string name)
        {
            name = hyphenCharacters.Replace(name, "$0-").ToLowerInvariant();
            return name;
        }

        /// <summary>
        /// Creates an implied media type from a file signature characters.
        /// </summary>
        /// <param name="signature">The signature of the file.</param>
        /// <returns>A MIME type in the form of
        /// <c>application/prs.implied-structure;signature={<paramref name="signature"/>}</c>.
        /// </returns>
        public static string GetImpliedMediaTypeFromSignature(string signature)
        {
            return "application/prs.implied-structure;signature=" + FormatMimeParameter(signature);
        }

        /// <summary>
        /// Creates an implied media type from an interpreter command.
        /// </summary>
        /// <param name="interpreter">The interpreter command.</param>
        /// <returns>A MIME type in the form of
        /// <c>application/prs.implied-executable;interpreter={<paramref name="interpreter"/>}</c>.
        /// </returns>
        public static string GetImpliedMediaTypeFromInterpreter(string interpreter)
        {
            return "application/prs.implied-executable;interpreter=" + FormatMimeParameter(interpreter);
        }

        /// <summary>
        /// Creates an implied media type from a JSON object using the types of its root members.
        /// </summary>
        /// <param name="rootMemberTypes">
        /// A map from the root member names to their types. Only values "object", "array", "number", "string", "boolean", or "null" are permitted.
        /// </param>
        /// <returns>A MIME type in the form of
        /// <c>application/prs.implied-object+json;{name}={type}…</c>.
        /// </returns>
        /// <remarks>
        /// <para>For efficient handling, <paramref name="rootMemberTypes"/> should
        /// be an instance of <see cref="SortedDictionary{TKey, TValue}"/>
        /// or <see cref="ImmutableSortedDictionary{TKey, TValue}"/>
        /// using <see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
        /// <para>The "null" type is ignored when producing the MIME type.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <paramref name="rootMemberTypes"/> contains duplicate members with differing types,
        /// or contains invalid types.
        /// </exception>
        public static string GetImpliedMediaTypeFromJson(IReadOnlyDictionary<string, string> rootMemberTypes)
        {
            var normalized = rootMemberTypes.Select(pair => new KeyValuePair<string, string>(pair.Key.ToLowerInvariant(), pair.Value.ToLowerInvariant()));
            return GetImpliedMediaTypeFromMembers("application/prs.implied-object+json", normalized, IsSortedDictionary(rootMemberTypes), allowedJsonTypes, nameof(rootMemberTypes));
        }

        /// <summary>
        /// Creates an implied media type from a JSON object sequence using the types of the root members.
        /// </summary>
        /// <param name="rootMemberTypes">
        /// A map from the root member names to their types. Only values "object", "array", "number", "string", "boolean", or "null" are permitted.
        /// </param>
        /// <returns>A MIME type in the form of
        /// <c>application/prs.implied-object+json-seq;{name}={type}…</c>.
        /// </returns>
        /// <remarks>
        /// See <see cref="GetImpliedMediaTypeFromJson(IReadOnlyDictionary{string, string})"/> for more details.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <paramref name="rootMemberTypes"/> contains duplicate members with differing types,
        /// or contains invalid types.
        /// </exception>
        public static string GetImpliedMediaTypeFromJsonSequence(IReadOnlyDictionary<string, string> rootMemberTypes)
        {
            var normalized = rootMemberTypes.Select(pair => new KeyValuePair<string, string>(pair.Key.ToLowerInvariant(), pair.Value.ToLowerInvariant()));
            return GetImpliedMediaTypeFromMembers("application/prs.implied-object+json-seq", normalized, IsSortedDictionary(rootMemberTypes), allowedJsonTypes, nameof(rootMemberTypes));
        }

        /// <summary>
        /// Creates an implied media type from a YAML document using the tags of its top-level mappings.
        /// </summary>
        /// <param name="topLevelTags">
        /// A map from the top-level keys to their tags.
        /// </param>
        /// <returns>A MIME type in the form of
        /// <c>application/prs.implied-object+yaml;{key}={tag}…</c>.
        /// </returns>
        /// <remarks>
        /// See <see cref="GetImpliedMediaTypeFromJson(IReadOnlyDictionary{string, string})"/> for more details.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <paramref name="topLevelTags"/> contains duplicate entries with differing tags.
        /// </exception>
        public static string GetImpliedMediaTypeFromYaml(IReadOnlyDictionary<string, Uri> topLevelTags)
        {
            var normalized = topLevelTags.Select(
                pair => new KeyValuePair<string, string>
                (
                    pair.Key.ToLowerInvariant(),
                    ShortenYamlTag(pair.Value.AbsoluteUri)
                )
            );
            return GetImpliedMediaTypeFromMembers("application/prs.implied-object+yaml", normalized, IsSortedDictionary(topLevelTags), null, nameof(topLevelTags));
        }

        static string ShortenYamlTag(string tagUri)
        {
            const string standardTagPrefix = "tag:yaml.org,2002:";

            if(tagUri.StartsWith(standardTagPrefix))
            {
                var shortened = tagUri.Substring(standardTagPrefix.Length);
                if(!Uri.IsWellFormedUriString(shortened, UriKind.Absolute))
                {
                    return shortened;
                }
            }
            return tagUri;
        }

        static readonly StringComparer memberNameComparer = StringComparer.OrdinalIgnoreCase;

        static bool IsSortedDictionary<TValue>(IReadOnlyDictionary<string, TValue> dictionary)
        {
            switch(dictionary)
            {
                case SortedDictionary<string, TValue> sorted:
                    return memberNameComparer.Equals(sorted.Comparer);
                case ImmutableSortedDictionary<string, TValue> immutableSorted:
                    return memberNameComparer.Equals(immutableSorted.KeyComparer);
            }
            return false;
        }

        static readonly HashSet<string> allowedJsonTypes = new(StringComparer.Ordinal)
        {
            "object", "array", "number", "string", "boolean"
        };

        static string GetImpliedMediaTypeFromMembers(string prefix, IEnumerable<KeyValuePair<string, string>> types, bool isSorted, HashSet<string>? allowedTypes, string typesArgumentName)
        {
            types = types.Where(
                pair => {
                    var name = pair.Key;
                    if(name.Length == 0)
                    {
                        // must be at least one character
                        return false;
                    }
                    // must be a valid ASCII NCName
                    if(!XmlConvert.IsStartNCNameChar(name[0]))
                    {
                        return false;
                    }
                    if(name.Any(c => c > '\x7F' || !XmlConvert.IsNCNameChar(c)))
                    {
                        return false;
                    }
                    return true;
                }
            );
            // Sort if not already sorted
            if(!isSorted)
            {
                types = types.OrderBy(pair => pair.Key, memberNameComparer);
            }
            var sb = new StringBuilder(prefix);
            string? lastMemberName = null;
            string? lastMemberType = null;
            foreach(var (name, type) in types)
            {
                if(lastMemberName == name)
                {
                    // duplicate member
                    // only the previous one is needed to check since the sequence is sorted
                    if(lastMemberType != type)
                    {
                        throw new ArgumentException($"There are multiple equivalent '{name}' members but with different types ({lastMemberType} and {type}).", typesArgumentName);
                    }
                    continue;
                }
                (lastMemberName, lastMemberType) = (name, type);
                if(type == "null")
                {
                    // null member type is not useful to output, but is needed for duplicate checking
                    continue;
                }
                if(allowedTypes != null && !allowedTypes.Contains(type))
                {
                    throw new ArgumentException($"'{type}' is not a valid JSON value type name.", typesArgumentName);
                }
                sb.Append(';');
                sb.Append(name);
                sb.Append('=');
                sb.Append(allowedTypes != null ? type : FormatMimeParameter(type));
            }
            return sb.ToString();
        }

        static readonly Regex invalidTokenCharacters = new(@"[][()<>@,;:\\""/?=\x00-\x20\x7F-\uFFFF]", RegexOptions.Compiled);
        static readonly Regex encodedWord = new(@"^=\?[^?]*\?[^?]*\?[^?]*\?=$", RegexOptions.Compiled);
        static readonly Regex escapedQuotedCharacters = new(@"[""\\\r]", RegexOptions.Compiled);

        static readonly Encoding mimeParamEncoding = Encoding.UTF8;

        /// <summary>
        /// Formats the value of a MIME parameter.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>Either <paramref name="value"/> unchanged, or escaped and wrapped in quotes.</returns>
        public static string FormatMimeParameter(string value)
        {
            if(!invalidTokenCharacters.IsMatch(value))
            {
                // The value is okay as a standalone token.
                return value;
            }
            if(!value.Any(c => c > '\x7F') && !encodedWord.IsMatch(value))
            {
                // The value does not need encoding.
                return "\"" + escapedQuotedCharacters.Replace(value, @"\$0") + "\"";
            }
            // Produce encoded-word from the value (might not be according to RFC 2047,
            // but corresponds to what the ContentType class does).
            // TODO: Support for RFC 2231?
            var bytes = mimeParamEncoding.GetBytes(value);
            string base64Encoded = Convert.ToBase64String(bytes);
            var qEncoded = new StringBuilder();
            qEncoded.Append("\"=?");
            var encodingName = mimeParamEncoding.WebName.ToLowerInvariant();
            qEncoded.Append(encodingName);
            qEncoded.Append("?Q?");
            var prefixLength = qEncoded.Length;
            foreach(var b in bytes)
            {
                switch(b)
                {
                    case (byte)' ':
                        qEncoded.Append('_');
                        break;
                    case (byte)'\\':
                    case (byte)'"':
                        qEncoded.Append('\\');
                        qEncoded.Append((char)b);
                        break;
                    case < 0x20 or > 0x7F or (byte)'=' or (byte)'?':
                        qEncoded.Append('=');
                        qEncoded.Append(b.ToString("X2"));
                        break;
                    default:
                        qEncoded.Append((char)b);
                        break;
                }
                if(qEncoded.Length - prefixLength > base64Encoded.Length)
                {
                    // Q-encoded is longer, use B
                    return $"\"=?{encodingName}?B?{base64Encoded}?=\"";
                }
            }
            qEncoded.Append("?=\"");
            return qEncoded.ToString();
        }

        static readonly Regex mimeNameRegex = new(@"^[^/;]+/(?:vnd\.|prs\.|x-|octet-stream;type=|)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Obtains the core name from a media type.
        /// </summary>
        /// <param name="type">The media type.</param>
        /// <returns>The subtype part, without "vnd.", "prs.", or "x-".</returns>
        public static string GetMimeTypeSimpleName(string type)
        {
            return mimeNameRegex.Replace(type, "");
        }

        /// <summary>
        /// Returns a user-friendly string representation of an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="entity">The object to retrieve the name from.</param>
        /// <returns>
        /// <para>
        /// If <paramref name="entity"/> is an instance of <see cref="Type"/>,
        /// returns its name expressed in a C#-like syntax.
        /// </para>
        /// <para>
        /// If <paramref name="entity"/> implements <see cref="IFormattable"/>,
        /// <see cref="IFormattable.ToString(string, IFormatProvider)"/> is used,
        /// using an invariant culture.
        /// </para>
        /// <para>
        /// If <typeparamref name="T"/> explicitly provides an instance of
        /// <see cref="TypeConverter"/> that defines <see cref="TypeConverter.ConvertToInvariantString(object)"/>,
        /// that method is preferred for the result, otherwise <see cref="Object.ToString"/> is called.
        /// If the returned string is empty or a result of the default implementation of <see cref="Object.ToString"/>,
        /// calls <see cref="GetUserFriendlyName{T}(T)"/> on the type of <paramref name="entity"/> or the type
        /// that was ultimately used by the <see cref="Object.ToString"/> implementation.
        /// </para>
        /// </returns>
        public static string GetUserFriendlyName<T>(T entity)
        {
            if(entity == null) return "<null>";

            if(entity is Type type)
            {
                if(!type.IsGenericType) return type.Name;
                var sb = new StringBuilder();
                var typeName = type.Name;
                var index = typeName.IndexOf('`');
                if(index != -1)
                {
                    sb.Append(type.Name, 0, index);
                }else{
                    sb.Append(type.Name);
                }
                sb.Append("<");
                bool first = true;
                foreach(var typeArg in type.GetGenericArguments())
                {
                    if(first)
                    {
                        first = false;
                    }else{
                        sb.Append(", ");
                    }
                    sb.Append(GetUserFriendlyName(typeArg));
                }
                sb.Append(">");
                return sb.ToString();
            }

            if(entity is IFormattable formattable)
            {
                // trust IFormattable types to always provide a useful result
                return NormalizeUserFriendlyString(formattable.ToString(null, CultureInfo.InvariantCulture));
            }
            string name;
            type = entity.GetType();
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if(TypeConverterHasDefinedStringConversion(converter))
            {
                // the converter is viable to convert to string
                var convertedName = converter.ConvertToInvariantString(entity);
                if(GetTypeFromName<T>(type, convertedName) is not Type existingTypeFromConverter)
                {
                    // not the name of any type, so the converter or instance did actually provide its own string conversion
                    return NormalizeUserFriendlyString(convertedName);
                }
                name = entity.ToString();
                if(name == convertedName)
                {
                    // the converter just used the ToString on the entity, and we already know it corresponds to a type
                    return GetUserFriendlyName(existingTypeFromConverter);
                }
            }else{
                name = entity.ToString();
            }

            // it is not viable to check whether ToString is overridden,
            // since wrapper types may just redirect it to another instance,
            // in which case the type would have to be found anyway
            if(GetTypeFromName<T>(type, name) is Type existingType)
            {
                // it is the name of a type, so use that type instead
                return GetUserFriendlyName(existingType);
            }
            return NormalizeUserFriendlyString(name);
        }

        static readonly Type stringType = typeof(string);
        static readonly Type typeConverterType = typeof(TypeConverter);
        static readonly MethodInfo convertToMethod = ((MethodCallExpression)((Expression<Func<object>>)(
            () => new TypeConverter().ConvertTo(null, default(CultureInfo), null, default(Type))
        )).Body).Method;
        static readonly ConditionalWeakTable<Type, object> converterTypeCache = new();

        static readonly Regex whitespaceRegex = new(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Makes a string suitable for text writing and logging.
        /// </summary>
        static string NormalizeUserFriendlyString(string str)
        {
            return whitespaceRegex.Replace(str, " ");
        }

        /// <summary>
        /// Checks that a type converter has a defined conversion to string.
        /// </summary>
        static bool TypeConverterHasDefinedStringConversion(TypeConverter converter)
        {
            var type = converter.GetType();
            if(typeConverterType.Equals(type))
            {
                // the default conversion to string is not desirable
                return false;
            }
            if(!converter.CanConvertTo(stringType))
            {
                // this converter rejects string (unlike the default implementation)
                return false;
            }
            return (bool)converterTypeCache.GetValue(type, t => {
                var methods = t.GetMethods().Where(m => m.Name == nameof(TypeConverter.ConvertTo));
                if(methods.FirstOrDefault(m => convertToMethod.Equals(m.GetBaseDefinition())) is MethodInfo m)
                {
                    // check that the ConvertTo method is overridden (somewhere along the hierarchy to base)
                    if(!typeConverterType.Equals(m.DeclaringType))
                    {
                        // the method is declared in the custom type, so it should be consulted
                        return true;
                    }
                }
                return false;
            });
        }

        /// <summary>
        /// Checks if <paramref name="name"/> corresponds to an existing type (or is empty).
        /// </summary>
        static Type? GetTypeFromName<T>(Type hintType, string name)
        {
            if(String.IsNullOrWhiteSpace(name) || name == hintType.ToString())
            {
                // no useful name, or ToString is not overriden
                if(hintType.IsCOMObject)
                {
                    // use the provided COM interface instead
                    hintType = typeof(T);
                }
                return hintType;
            }

            try{
                hintType = AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetType(name, false)).FirstOrDefault(t => t != null);
                return hintType;
            }catch{
                // name is not a valid type name
                return null;
            }
        }

        /// <summary>
        /// Matches any sequence of '*', a single occurence of '?', or a sequence of any other characters.
        /// </summary>
        static readonly Regex wildcardRegex = new(@"(\*+|\?)|[^*?]+", RegexOptions.Compiled);

        /// <summary>
        /// Creates an instance of <see cref="Regex"/> from a wildcard pattern.
        /// </summary>
        /// <param name="pattern">The pattern, using * and ? as special characters.</param>
        /// <returns>
        /// A regular expression matching the whole string, where each occurence of '*'
        /// in <paramref name="pattern"/> is replaced by ".*", each occurence of '?'
        /// is replaced by ".", and the remaining portions are escaped with <see cref="Regex.Escape(string)"/>.
        /// </returns>
        public static Regex ConvertWildcardToRegex(string pattern)
        {
            static string Replacer(Match match)
            {
                if(match.Groups[1].Success)
                {
                    switch(match.Groups[1].Value[0])
                    {
                        case '*':
                            return ".*";
                        case '?':
                            return ".";
                    }
                }
                return Regex.Escape(match.Value);
            }

            return new Regex($"^{wildcardRegex.Replace(pattern, Replacer)}$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        static readonly char[] wildcardCharacters = { '*', '?' };

        /// <summary>
        /// Checks if <paramref name="text"/> is a wildcard.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>Whether <paramref name="text"/> contains any occurrences of <c>*</c> and <c>?</c>.</returns>
        public static bool ContainsWildcardCharacters(string text)
        {
            return text.IndexOfAny(wildcardCharacters) != -1;
        }

        /// <summary>
        /// Substitutes <paramref name="variables"/> in <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The pattern to replace variables in.</param>
        /// <param name="variables">A collection of key-value pairs to replace in <paramref name="text"/>.</param>
        /// <returns>The text with substituted variables.</returns>
        /// <remarks>
        /// The current implementation uses <see cref="Regex.Replace(string, string, string)"/> to perform the replacement.
        /// </remarks>
        public static string SubstituteVariables(string text, IEnumerable<KeyValuePair<string, object?>> variables)
        {
			// "value\0" for each pair
			var source = new StringBuilder();
			// "(?<key>.{value-length})\0" for each pair
			var pattern = new StringBuilder();

			pattern.Append("^");
			foreach(var (key, value) in variables)
            {
                var valueStr =
                    (value is IFormattable fmt ? fmt.ToString(null, CultureInfo.InvariantCulture) :
                    value?.ToString()) ?? "";
				source.Append(valueStr);
				source.Append('\0');
				pattern.Append($"(?<{key}>.{{{valueStr.Length}}})\0");
			}
			pattern.Append("$");

			// Perform substitution using Regex
			return Regex.Replace(source.ToString(), pattern.ToString(), text, RegexOptions.Singleline);
        }

        /// <inheritdoc cref="FormatMemberId(MemberInfo, StringBuilder, bool, bool, bool)"/>
        public static string FormatMemberId(MemberInfo member, bool inUri = false, bool includeNamespace = true, bool includeDeclaringType = true)
        {
            var sb = new StringBuilder();
            FormatMemberId(member, sb, inUri, includeNamespace, includeDeclaringType);
            return sb.ToString();
        }

        /// <summary>
        /// Produces an identifier from <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="member">The member to get the identifier from.</param>
        /// <param name="stringBuilder">The buffer to write the result to.</param>
        /// <param name="inUri">Whether to create a URI-safe identifier.</param>
        /// <param name="includeNamespace">Whether to include the namespace of the declaring type.</param>
        /// <param name="includeDeclaringType">Whether to include the declaring type or method in the identifier.</param>
        /// <returns>The formatted identifier.</returns>
        public static StringBuilder FormatMemberId(MemberInfo member, StringBuilder stringBuilder, bool inUri = false, bool includeNamespace = true, bool includeDeclaringType = true)
        {
            var context = new MemberIdFormatContext(stringBuilder, inUri);
            context.Member(member, includeNamespace, includeDeclaringType);
            return stringBuilder;
        }

        struct MemberIdFormatContext
        {
            readonly StringBuilder sb;
            readonly bool inUri;

            public MemberIdFormatContext(StringBuilder sb, bool inUri)
            {
                this.sb = sb;
                this.inUri = inUri;
            }

            public void Member(MemberInfo member, bool includeNamespace, bool includeDeclaringType)
            {
                if(member is Type type)
                {
                    if(!includeDeclaringType)
                    {
                        if(type.IsGenericParameter)
                        {
                            // Just the position requested
                            if(type.DeclaringMethod != null)
                            {
                                Syntax("``");
                            }else{
                                Syntax("`");
                            }
                            sb.Append(type.GenericParameterPosition);
                            return;
                        }
                        // Just name requested (make sure the type is not constructed)
                        Literal(includeNamespace ? (type.FullName ?? type.Name) : type.Name);
                        return;
                    }
                    Type(type, includeNamespace);
                    return;
                }
                if(includeDeclaringType)
                {
                    Type(member.DeclaringType, includeNamespace);
                    Syntax(".");
                }
                if(member is not MethodBase method)
                {
                    Literal(member.Name);
                    return;
                }
                if(method.Equals(method.DeclaringType.TypeInitializer))
                {
                    Syntax("#cctor");
                }else if(method.IsConstructor)
                {
                    Syntax("#ctor");
                }else{
                    // Escapes explicit implementations
                    Literal(method.Name.Replace('.', '#'));
                }
                if(method.IsGenericMethodDefinition)
                {
                    Syntax("``");
                    sb.Append(method.GetGenericArguments().Length);
                }else if(method.IsGenericMethod)
                {
                    Syntax("{");
                    var genArgs = method.GetGenericArguments();
                    for(int i = 0; i < genArgs.Length; i++)
                    {
                        if(i > 0)
                        {
                            Syntax(",");
                        }
                        Type(genArgs[i], true);
                    }
                    Syntax("}");
                }
                var parms = method.GetParameters();
                if(parms.Length == 0)
                {
                    return;
                }
                Syntax("(");
                for(int i = 0; i < parms.Length; i++)
                {
                    if(i > 0)
                    {
                        Syntax(",");
                    }
                    Type(parms[i].ParameterType, true, genericContext: (method.DeclaringType, method));
                }
                Syntax(")");
            }

            void Type(Type type, bool includeNamespace, bool constructed = false, (Type? type, MethodBase? method) genericContext = default)
            {
                if(type.IsArray)
                {
                    var elemType = type.GetElementType();
                    Type(elemType, includeNamespace, genericContext: genericContext);
                    var rank = type.GetArrayRank();
                    if(rank <= 1 && type.Equals(elemType.MakeArrayType()))
                    {
                        //SZ array
                        Syntax("[]");
                        return;
                    }
                    Syntax("[0:");
                    for(int i = 1; i < rank; i++)
                    {
                        Syntax(",0:");
                    }
                    Syntax("]");
                    return;
                }
                if(type.IsPointer)
                {
                    var elemType = type.GetElementType();
                    Type(elemType, includeNamespace, genericContext: genericContext);
                    Syntax("*");
                    return;
                }
                if(type.IsByRef)
                {
                    var elemType = type.GetElementType();
                    Type(elemType, includeNamespace, genericContext: genericContext);
                    Syntax("@");
                    return;
                }
                if(type.IsGenericParameter)
                {
                    if(type.DeclaringMethod is { } genDeclaringMethod)
                    {
                        // On method
                        if(!genDeclaringMethod.Equals(genericContext.method))
                        {
                            // Not the current one
                            Member(genDeclaringMethod, true, true);
                            Syntax("/");
                        }
                        Syntax("`");
                    }else if(type.DeclaringType is { } genDeclaringType)
                    {
                        // On type
                        if(!genDeclaringType.Equals(genericContext.type))
                        {
                            // Not the current one
                            Type(genDeclaringType, true);
                            Syntax("/");
                        }
                    }
                    Syntax("`");
                    sb.Append(type.GenericParameterPosition);
                    return;
                }
                if(type.IsConstructedGenericType)
                {
                    var elemType = type.GetGenericTypeDefinition();
                    Type(elemType, includeNamespace, constructed: true);
                    Syntax("{");
                    var genArgs = type.GetGenericArguments();
                    for(int i = 0; i < genArgs.Length; i++)
                    {
                        if(i > 0)
                        {
                            Syntax(",");
                        }
                        Type(genArgs[i], true, genericContext: genericContext);
                    }
                    Syntax("}");
                    return;
                }
                if(type.DeclaringType is { } declaringType)
                {
                    Type(declaringType, includeNamespace);
                    Syntax(".");
                }else if(includeNamespace)
                {
                    var ns = type.Namespace;
                    if(!String.IsNullOrEmpty(ns))
                    {
                        Literal(ns);
                        Syntax(".");
                    }
                }
                var name = type.Name;
                if(constructed)
                {
                    // remove arity
                    int arityStart = name.LastIndexOf('`');
                    if(arityStart != -1)
                    {
                        name = name.Substring(0, arityStart);
                    }
                }
                Literal(name);
            }

            void Syntax(string text)
            {
                if(inUri)
                {
                    sb.Append(UriTools.EscapePathString(text));
                }else{
                    sb.Append(text);
                }
            }

            void Literal(string text)
            {
                if(inUri)
                {
                    sb.Append(Uri.EscapeDataString(text));
                }else{
                    sb.Append(text);
                }
            }
        }
    }
}
