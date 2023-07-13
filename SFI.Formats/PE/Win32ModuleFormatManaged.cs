using IS4.SFI.Formats.Modules;
using IS4.SFI.Tools;
using PeNet;
using PeNet.Header.Authenticode;
using PeNet.Header.Pe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Windows PE module format, backed by <see cref="PeFile"/>.
    /// </summary>
    [Description("Represents the Windows PE module format.")]
    public class Win32ModuleFormatManaged : WinModuleFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public Win32ModuleFormatManaged() : base("PE\0\0", "application/vnd.microsoft.portable-executable", null)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IModule, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            if(PeFile.TryParse(stream, out var file) && file != null)
            {
                return await resultFactory(new Module(file, stream), args);
            }
            return default;
        }

        class Module : IModule
        {
            readonly PeFile file;
            readonly Stream stream;

            bool? verified;

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

            public IModuleSignature? Signature {
                get {
                    var sig = file.Authenticode;
                    if(sig != null && sig.SignedHash != null)
                    {
                        if(verified ?? (verified = VerifySignatureHash() && VerifyHash(sig)).GetValueOrDefault())
                        {
                            return new SignatureInfo(sig);
                        }
                    }
                    return null;
                }
            }
            
            bool VerifySignatureHash()
            {
                var cert = file.WinCertificate?.BCertificate.ToArray();
                if(cert == null) return false;
                var cms = new System.Security.Cryptography.Pkcs.SignedCms();
                cms.Decode(cert);
                try{
                    cms.CheckHash();
                }catch{
                    return false;
                }
                return true;
            }
            
            bool VerifyHash(AuthenticodeInfo sig)
            {
                var hash = BuiltInHash.FromLength(sig.SignedHash!.Length);
                if(hash == null) return false;
                var algorithm = hash.Algorithm;
                try{
                    var computed = sig.ComputeAuthenticodeHashFromPeFile(algorithm);
                    return computed != null && computed.SequenceEqual(sig.SignedHash);
                }finally{
                    algorithm.Initialize();
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
                return file.ImageResourceDirectory.DirectoryEntries.Where(e => e != null && e.DataIsDirectory).SelectMany(e => Resource.Directory(this, e!, rsrc));
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

                public object Name => entry.IsNamedEntry ? entry.NameResolved! : entry.ID;

                public int Length => unchecked((int)data.Size1);

                public int Read(byte[] buffer, int offset, int length)
                {
                    length = Math.Min(length, Length);
                    long position = data.OffsetToData - rsrc.VirtualAddress + rsrc.PointerToRawData;
                    if(position > module.stream.Length)
                    {
                        return 0;
                    }
                    module.stream.Position = position;
                    return module.stream.Read(buffer, offset, length);
                }

                public static IEnumerable<Resource> Directory(Module module, ImageResourceDirectoryEntry entry, ImageSectionHeader rsrc)
                {
                    object type = entry.IsNamedEntry ? entry.NameResolved! : (Win32ResourceType)entry.ID;
                    var resources = entry.ResourceDirectory;
                    if(resources == null) return Array.Empty<Resource>();
                    return resources.DirectoryEntries.Where(e => e != null && e.DataIsDirectory).SelectMany(e => DirectoryEntry(module, type, e!, rsrc));
                }

                public static IEnumerable<Resource> DirectoryEntry(Module module, object type, ImageResourceDirectoryEntry entry, ImageSectionHeader rsrc)
                {
                    var resources = entry.ResourceDirectory;
                    if(resources == null) return Array.Empty<Resource>();
                    return resources.DirectoryEntries.Where(e => e != null && !e.DataIsDirectory).Select(e => new Resource(module, type, entry, e!.ResourceDataEntry!, rsrc));
                }
            }

            class SignatureInfo : IModuleSignature
            {
                readonly AuthenticodeInfo info;

                public SignatureInfo(AuthenticodeInfo info)
                {
                    this.info = info;
                }

                public BuiltInHash HashAlgorithm => Services.HashAlgorithm.FromLength(info.SignedHash!.Length)!;

                public byte[] Hash => info.SignedHash!;

                public X509Certificate2 Certificate => info.SigningCertificate!;

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
