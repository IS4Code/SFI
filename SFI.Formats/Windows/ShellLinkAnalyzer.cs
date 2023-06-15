using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Text;
using System.Threading.Tasks;
using static Vanara.PInvoke.Kernel32;
using static Vanara.PInvoke.Shell32;
using static Vanara.PInvoke.ShlwApi;
using static Vanara.PInvoke.ComCtl32;
using System.Text.RegularExpressions;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of Windows Shell Links, as instances of <see cref="IShellLinkW"/>.
    /// </summary>
    public class ShellLinkAnalyzer : MediaObjectAnalyzer<IShellLinkW>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IShellLinkW link, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            var sb = new StringBuilder(MAX_PATH, MAX_PATH);
            link.GetPath(sb, sb.Capacity, out _, SLGP.SLGP_RAWPATH);
            var path = sb.ToString();
            if(!String.IsNullOrEmpty(path))
            {
                var uri = FileUriFromPath(path);
                if(uri != null)
                {
                    node.Set(Properties.Links, UriFormatter.Instance, uri);
                }
            }

            sb = new StringBuilder(INFOTIPSIZE, INFOTIPSIZE);
            link.GetDescription(sb, sb.Capacity);
            if(IsDefined(sb.ToString(), out var desc))
            {
                node.Set(Properties.Description, desc);
            }

            return new AnalysisResult(node);
        }

        static readonly Regex variableBased = new(@"^(file:(?://.*?/)?)%25(\w+)%25", RegexOptions.Compiled);
        static readonly Regex noPrefix = new(@"^file:([^/])", RegexOptions.Compiled);

        static Uri? FileUriFromPath(string path)
        {
            // Compute length
            uint len = 1;
            var buffer = new StringBuilder(1);
            UrlCreateFromPath(path, buffer, ref len, 0);
            if(len == 1) return null;

            buffer.EnsureCapacity(unchecked((int)len));
            UrlCreateFromPath(path, buffer, ref len, 0);
            var result = buffer.ToString();
            if(String.IsNullOrWhiteSpace(result)) return null;

            // Replace %VAR% at the beginning with VAR: (to simulate "drive")
            result = variableBased.Replace(result, m => $"{m.Groups[1].Value}{m.Groups[2].Value.ToUpperInvariant()}:");

            // Add /// (is missing for relative paths)
            result = noPrefix.Replace(result, "file:///$1");

            return new Uri(Uri.EscapeUriString(result), UriKind.Absolute);
        }
    }
}
