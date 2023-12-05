using BisUtils.Core.Serialization;
using BisUtils.PBO;
using BisUtils.PBO.Entries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the PBO archive format, as an instance of <see cref="PboFile"/>.
    /// </summary>
    [Description("Represents the PBO archive format used in Bohemia Interactive games.")]
    public class PboFormat : SignatureFormat<PboFile>
    {
        /// <summary>
        /// Contains the encoding used for reading strings.
        /// </summary>
        [Description("Contains the encoding used for reading strings.")]
        public Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public PboFormat() : base("\0sreV", "application/x-pbo", "pbo")
        {

        }

        public override async ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<PboFile, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            if(OpenPboStream(stream) is { } streamPbo)
            {
                try{
                    return await resultFactory(streamPbo, args);
                }finally{
                    streamPbo.Dispose();
                }
            }
            using var tmpPath = FileTools.GetTemporaryFile("pbo");
            using(var file = new FileStream(tmpPath, FileMode.CreateNew))
            {
                stream.CopyTo(file);
            }
            using var pbo = new PboFile(tmpPath);
            return await resultFactory(pbo, args);
        }

        static Type pboFileType = typeof(PboFile);
        static ConstructorInfo? baseCtor = typeof(BisSynchronizable).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(Stream) });
        static PropertyInfo? pboEntries = typeof(PboFile).GetProperty("PBOEntries", BindingFlags.NonPublic | BindingFlags.Instance);

        [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicProperties, typeof(PboFile))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicConstructors, typeof(BisSynchronizable))]
        PboFile? OpenPboStream(Stream stream)
        {
            if(baseCtor == null || pboEntries == null)
            {
                return null;
            }
            var pboFile = (PboFile)FormatterServices.GetUninitializedObject(pboFileType);
            baseCtor.Invoke(pboFile, new object[] { stream });
            pboEntries.SetValue(pboFile, new List<PboEntry>());
            pboFile.ReadBinary(new BinaryReader(stream, DefaultEncoding, leaveOpen: true));
            return pboFile;
        }
    }
}
