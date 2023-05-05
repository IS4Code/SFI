using System;
using System.Collections.Generic;
using System.Text;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Stores the information obtained during data analysis.
    /// </summary>
    public interface IDataObject : IPersistentKey
    {
        /// <summary>
        /// The the data is binary, i.e. not textual.
        /// </summary>
        bool IsBinary { get; }

        /// <summary>
        /// The original entity that was used to read the data from.
        /// </summary>
        IStreamFactory Source { get; }

        /// <summary>
        /// A factory of seekable streams that can be used to read the data.
        /// </summary>
        IStreamFactory StreamFactory { get; }

        /// <summary>
        /// The actual number of bytes read from <see cref="Source"/>
        /// (may be different from <see cref="IStreamFactory.Length"/>).
        /// </summary>
        long ActualLength { get; }

        /// <summary>
        /// The mapping of hashes and their values as computed during
        /// analysis.
        /// </summary>
        IReadOnlyDictionary<IDataHashAlgorithm, byte[]> Hashes { get; }

        /// <summary>
        /// The collection of recognized format objects and the results of the analysis.
        /// </summary>
        IReadOnlyDictionary<IBinaryFormatObject, AnalysisResult> Formats { get; }

        /// <summary>
        /// Indicates whether the data is marked as not containing any structured data.
        /// </summary>
        bool IsPlain { get; }

        /// <summary>
        /// The byte content of the data, possibly only an
        /// initial portion if <see cref="IsComplete"/> is <see langword="false"/>.
        /// </summary>
        ArraySegment<byte> ByteValue { get; }

        /// <summary>
        /// The string content of the data, possibly only an
        /// initial portion if <see cref="IsComplete"/> is <see langword="false"/>.
        /// </summary>
        string? StringValue { get; }

        /// <summary>
        /// The recognized charset of the text.
        /// </summary>
        string? Charset { get; }

        /// <summary>
        /// The encoding used to produce <see cref="StringValue"/>.
        /// </summary>
        Encoding? Encoding { get; }

        /// <summary>
        /// Whether <see cref="ByteValue"/> and <see cref="StringValue"/>
        /// describe the whole data, or only its beginning.
        /// </summary>
        bool IsComplete { get; }
    }
}
