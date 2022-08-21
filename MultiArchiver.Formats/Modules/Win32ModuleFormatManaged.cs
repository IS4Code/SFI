using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Media.Modules;
using IS4.MultiArchiver.Tools;
using PeNet;
using PeNet.Header.Authenticode;
using PeNet.Header.Pe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the Windows PE module format, backed by <see cref="PeFile"/>.
    /// </summary>
    public class Win32ModuleFormatManaged : WinModuleFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public Win32ModuleFormatManaged() : base("PE\0\0", "application/vnd.microsoft.portable-executable", null)
        {

        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IModule, TResult, TArgs> resultFactory, TArgs args)
        {
            if(PeFile.TryParse(stream, out var file))
            {
                return await resultFactory(new Module(file, stream), args);
            }
            return default;
        }

        class Module : IModule
        {
            readonly PeFile file;
            readonly Stream stream;

            public ModuleType Type =>
                file.IsDriver ? ModuleType.System :
                file.IsDll ? ModuleType.Library :
                file.IsExe ? ModuleType.Executable :
                ModuleType.Unknown;

            public Module(PeFile file, Stream stream)
            {
                this.file = file;
                this.stream = stream;
            }

            public IModuleSignature Signature {
                get {
                    var sig = file.Authenticode;
                    if(sig != null && sig.IsAuthenticodeValid)
                    {
                        return new SignatureInfo(sig);
                    }
                    return null;
                }
            }

            public override string ToString()
            {
                return file.ToString();
            }

            public IEnumerable<IModuleResource> ReadResources()
            {
                var rsrc = file.ImageSectionHeaders?.FirstOrDefault(h => h.Name == ".rsrc");
                if(rsrc == null || file.ImageResourceDirectory == null)
                {
                    return Array.Empty<IModuleResource>();
                }
                return file.ImageResourceDirectory.DirectoryEntries.Where(e => e.DataIsDirectory).SelectMany(e => Resource.Directory(this, e, rsrc));
            }

            class Resource : IModuleResource
            {
                readonly Module module;
                readonly ImageResourceDirectoryEntry entry;
                readonly ImageResourceDataEntry data;
                readonly ImageSectionHeader rsrc;

                public Resource(Module module, object type, ImageResourceDirectoryEntry entry, ImageResourceDataEntry data, ImageSectionHeader rsrc)
                {
                    this.module = module;
                    Type = type;
                    this.entry = entry;
                    this.data = data;
                    this.rsrc = rsrc;
                }

                public object Type { get; }

                public object Name => entry.IsNamedEntry ? (object)entry.NameResolved : entry.ID;

                public int Length => unchecked((int)data.Size1);

                public int Read(byte[] buffer, int offset, int length)
                {
                    length = Math.Min(length, Length);
                    module.stream.Position = data.OffsetToData - rsrc.VirtualAddress + rsrc.PointerToRawData;
                    return module.stream.Read(buffer, offset, length);
                }

                public static IEnumerable<Resource> Directory(Module module, ImageResourceDirectoryEntry entry, ImageSectionHeader rsrc)
                {
                    object type = entry.IsNamedEntry ? (object)entry.NameResolved : (Win32ResourceType)entry.ID;
                    return entry.ResourceDirectory.DirectoryEntries.Where(e => e.DataIsDirectory).SelectMany(e => DirectoryEntry(module, type, e, rsrc));
                }

                public static IEnumerable<Resource> DirectoryEntry(Module module, object type, ImageResourceDirectoryEntry entry, ImageSectionHeader rsrc)
                {
                    return entry.ResourceDirectory.DirectoryEntries.Where(e => !e.DataIsDirectory).Select(e => new Resource(module, type, entry, e.ResourceDataEntry, rsrc));
                }
            }

            class SignatureInfo : IModuleSignature
            {
                readonly AuthenticodeInfo info;

                public SignatureInfo(AuthenticodeInfo info)
                {
                    this.info = info;
                }

                public BuiltInHash HashAlgorithm => Services.HashAlgorithm.FromLength(info.SignedHash.Length);

                public byte[] Hash => info.SignedHash;

                public X509Certificate2 Certificate => info.SigningCertificate;

                public byte[] ComputeHash(BuiltInHash hash)
                {
                    var algorithm = hash.Algorithm;
                    algorithm.Initialize();
                    info.ComputeAuthenticodeHashFromPeFile(algorithm);
                    return algorithm.Hash;
                }
            }
        }
    }
}
