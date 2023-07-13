using IS4.SFI.Services;
using MetadataExtractor;
using MetadataExtractor.Formats.FileType;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents a format for images with metadata, producing instances of
    /// <see cref="IReadOnlyList{T}"/> of <see cref="MetadataExtractor.Directory"/>.
    /// </summary>
    [Description("Represents a format for images with metadata.")]
    public class ImageMetadataFormat : BinaryFileFormat<IReadOnlyList<MetadataExtractor.Directory>>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public ImageMetadataFormat() : base(DataTools.MaxBomLength, null, null)
        {

        }

        /// <inheritdoc/>
        public override string? GetExtension(IReadOnlyList<MetadataExtractor.Directory> metadata)
        {
            return GetFileTag(metadata, FileTypeDirectory.TagExpectedFileNameExtension) ?? GetTypeFromName(GetFileTag(metadata, FileTypeDirectory.TagDetectedFileTypeName)).Item2 ?? base.GetExtension(metadata);
        }

        /// <inheritdoc/>
        public override string? GetMediaType(IReadOnlyList<MetadataExtractor.Directory> metadata)
        {
            return GetFileTag(metadata, FileTypeDirectory.TagDetectedFileMimeType) ?? GetTypeFromName(GetFileTag(metadata, FileTypeDirectory.TagDetectedFileTypeName)).Item1 ?? base.GetMediaType(metadata);
        }

        private string? GetFileTag(IReadOnlyList<MetadataExtractor.Directory> metadata, int tag)
        {
            return metadata.OfType<FileTypeDirectory>().FirstOrDefault()?.GetString(tag);
        }

        private (string, string) GetTypeFromName(string? name)
        {
            if(name == "RIFF") return ("application/x-riff", "riff");
            return default;
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IReadOnlyList<MetadataExtractor.Directory>, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory(ImageMetadataReader.ReadMetadata(stream), args);
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return isBinary || DataTools.FindBom(header) == 0;
        }
    }
}
