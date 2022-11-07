using System;
using System.Collections.Generic;
using System.Text;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Stores the information obtained during data analysis.
    /// </summary>
    public interface IDataObject
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
        /// The byte content of the data, possibly only from its
        /// beginning if <see cref="IsComplete"/> is true.
        /// </summary>
        ArraySegment<byte> ByteValue { get; }

        /// <summary>
        /// The string content of the data, possibly only from its
        /// beginning if <see cref="IsComplete"/> is true.
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
        /// Whether any format was recognized from the data or not.
        /// </summary>
        bool Recognized { get; }

        /// <summary>
        /// Whether <see cref="ByteValue"/> and <see cref="StringValue"/>
        /// describe the whole data, or only its beginning.
        /// </summary>
        bool IsComplete { get; }
    }
}
