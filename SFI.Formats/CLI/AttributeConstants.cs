using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IS4.SFI.Analyzers
{
    internal static class AttributeConstants
    {
        public static readonly string AttributeType = typeof(Attribute).FullName;
        public static readonly string AttributeUsageAttributeType = typeof(AttributeUsageAttribute).FullName;
        public static readonly string GuidAttributeType = typeof(GuidAttribute).FullName;
        public static readonly string DecimalConstantAttributeType = typeof(DecimalConstantAttribute).FullName;
        public static readonly string DateTimeConstantAttributeType = typeof(DateTimeConstantAttribute).FullName;
        public static readonly string ObsoleteAttributeType = typeof(ObsoleteAttribute).FullName;
        public static readonly string UnmanagedCallersOnlyAttributeType = "System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute";
        public static readonly string AssemblyMetadataAttribute = typeof(AssemblyMetadataAttribute).FullName;

        public static readonly string IsVolatileModifierType = typeof(IsVolatile).FullName;

        public static readonly IReadOnlyDictionary<string, (PropertyUri Uri, bool UseLanguage)> AttributeProperties = new Dictionary<string, (PropertyUri, bool)>()
        {
            //{ typeof(AssemblyProductAttribute).FullName, (Properties.) },
            { typeof(AssemblyCompanyAttribute).FullName, (Properties.Creator, true) },
            { typeof(AssemblyCopyrightAttribute).FullName, (Properties.CopyrightNotice, true) },
            { typeof(AssemblyInformationalVersionAttribute).FullName, (Properties.SoftwareVersion, false) },
            { typeof(AssemblyFileVersionAttribute).FullName, (Properties.Version, false) },
            { typeof(AssemblyTitleAttribute).FullName, (Properties.Title, true) },
            { typeof(AssemblyDescriptionAttribute).FullName, (Properties.Description, true) },

            { typeof(CategoryAttribute).FullName, (Properties.Category, true) },
            { typeof(DefaultValueAttribute).FullName, (Properties.DefaultValue, false) },
            { typeof(DisplayNameAttribute).FullName, (Properties.Label, true) },
            { typeof(DescriptionAttribute).FullName, (Properties.Description, true) },
        };
    }
}
