using IS4.SFI.Tools;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents a general module with executable code.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// The type of the module, as one of the values in <see cref="ModuleType"/>.
        /// </summary>
        ModuleType Type { get; }

        /// <summary>
        /// Opens the list of resources in the module and returns them
        /// as a collection of <see cref="IModuleResource"/>.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IModuleResource> ReadResources();

        /// <summary>
        /// The cryptographic signature of the module.
        /// </summary>
        IModuleSignature? Signature { get; }
    }

    /// <summary>
    /// The type of a module.
    /// </summary>
    public enum ModuleType
    {
        /// <summary>
        /// The type is not known/recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// The module is an executable, i.e. it has an entry point that can be
        /// run on its own.
        /// </summary>
        Executable,

        /// <summary>
        /// The module is a library, containing a collection of externally
        /// available functions or resources.
        /// </summary>
        Library,

        /// <summary>
        /// The module is a system file, driver etc.
        /// </summary>
        System
    }

    /// <summary>
    /// Represents a resource in <see cref="IModule"/>.
    /// </summary>
    public interface IModuleResource
    {
        /// <summary>
        /// The type of the resource, usually as <see cref="System.String"/>
        /// or <see cref="System.Enum"/>.
        /// </summary>
        object Type { get; }

        /// <summary>
        /// The name of the resource, usually as <see cref="System.String"/>
        /// or <see cref="System.UInt32"/>.
        /// </summary>
        object Name { get; }

        /// <summary>
        /// The length of the resource in bytes.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Reads the whole resource into an array buffer.
        /// </summary>
        /// <param name="buffer">The buffer to receive the resource data.</param>
        /// <param name="offset">The offset where to read the data.</param>
        /// <param name="length">The maximum length of data to read.</param>
        /// <returns>The number of bytes read from the resource.</returns>
        int Read(byte[] buffer, int offset, int length);
    }

    /// <summary>
    /// Represents a cryptographically-signed part of a module.
    /// </summary>
    public interface IModuleSignature
    {
        /// <summary>
        /// The hash present in the signature.
        /// </summary>
        byte[] Hash { get; }

        /// <summary>
        /// The built-in hash algorithm used to produce <see cref="Hash"/>.
        /// </summary>
        BuiltInHash HashAlgorithm { get; }

        /// <summary>
        /// Computes a hash from the module using the provided built-in hash algorithm.
        /// </summary>
        /// <param name="hash">An instance of <see cref="BuiltInHash"/> to use.</param>
        /// <returns>The result of the hashing.</returns>
        byte[] ComputeHash(BuiltInHash hash);

        /// <summary>
        /// The signing X.509 certificate.
        /// </summary>
        X509Certificate2 Certificate { get; }
    }
}
