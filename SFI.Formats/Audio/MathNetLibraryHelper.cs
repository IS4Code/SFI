using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IS4.SFI.Formats.Audio
{
    internal static class MathNetLibraryHelper
    {
        public const string MKLPackageWildcard = "mathnet.numerics.mkl.*";
        public const string MKLPackageSuggestion = "mathnet.numerics.mkl.win";
        public const string MKLVersionSuggestion = "2.5.0";

        static string nugetPackages => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        public static IEnumerable<(string dir, Version version)> FindMKLInstallations()
        {
            var archWildcard = "*" + RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
            foreach(var mklFolder in Directory.EnumerateDirectories(nugetPackages, MKLPackageWildcard))
            {
                foreach(var mklVersionDir in Directory.EnumerateDirectories(mklFolder))
                {
                    if(!Version.TryParse(Path.GetFileName(mklVersionDir), out var version))
                    {
                        // Should be a version
                        continue;
                    }
                    // Try build/ and runtimes/ if they contain architecture names
                    var archDirs = new[]
                    {
                        Path.Combine(mklVersionDir, "build"),
                        Path.Combine(mklVersionDir, "runtimes")
                    }.Where(Directory.Exists).SelectMany(dir => Directory.EnumerateDirectories(dir, archWildcard));
                    foreach(var archDir in archDirs)
                    {
                        // Yield the architecture folder if there are files inside
                        if(Directory.EnumerateFiles(archDir).Any())
                        {
                            yield return (WrapQuote(archDir), version);
                        }
                        // Yield the native/ folder if there are files inside
                        var nativeDir = Path.Combine(archDir, "native");
                        if(Directory.Exists(nativeDir) && Directory.EnumerateFiles(nativeDir).Any())
                        {
                            yield return (WrapQuote(nativeDir), version);
                        }

                        static string WrapQuote(string dir)
                        {
                            if(dir.Contains(";"))
                            {
                                return $"\"{dir}\"";
                            }
                            return dir;
                        }
                    }
                }
            }
        }

        public static string FormatLoadExceptionMessage(Exception? exception)
        {
            if(exception == null)
            {
                return "";
            }
            var sb = new StringBuilder();
            while(exception != null)
            {
                var msg = exception.Message;
                sb.Append("- ");
                sb.AppendLine(msg);
                var inner = exception.InnerException;
                if(inner == exception)
                {
                    inner = null;
                }
                exception = inner;
            }
            return sb.ToString();
        }
    }
}
