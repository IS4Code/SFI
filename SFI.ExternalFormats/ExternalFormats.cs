using System.Reflection;

namespace IS4.SFI
{
    /// <inheritdoc cref="BaseFormats"/>
    public static class ExternalFormats
    {
        /// <inheritdoc cref="BaseFormats.Assembly"/>
        public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    }
}
