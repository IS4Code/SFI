﻿using IS4.SFI.Formats.Archives;
using SharpCompress.Readers.Rar;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the RAR archive format.
    /// </summary>
    [Description("Represents the RAR archive format.")]
    public class RarFormat : SignatureFormat<IArchiveReader>
    {
        /// <summary>
        /// Whether to read the input as a multi-volume archive by default.
        /// </summary>
        [Description("Whether to read the input as a multi-volume archive by default.")]
        public bool TryMultiVolumeExtraction { get; set; } = true;

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public RarFormat() : base("Rar!", "application/vnd.rar", "rar")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveReader, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var reader = TryMultiVolumeExtraction
                ? RarReader.Open(new[] { stream })
                : RarReader.Open(stream);
            return await resultFactory(new ArchiveReaderAdapter(reader), args);
        }
    }
}
