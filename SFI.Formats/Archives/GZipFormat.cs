﻿using IS4.SFI.Formats.Archives;
using IS4.SFI.Media;
using SharpCompress.Readers.GZip;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the gzip archive format.
    /// </summary>
    public class GZipFormat : SignatureFormat<IArchiveReader>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public GZipFormat() : base(new byte[] { 0x1F, 0x8B }, "application/gzip", "gz")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveReader, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var reader = GZipReader.Open(stream);
            return await resultFactory(new ArchiveReaderAdapter(reader), args);
        }
    }
}
