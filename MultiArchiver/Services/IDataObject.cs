using System;
using System.Collections.Generic;
using System.Text;

namespace IS4.MultiArchiver.Services
{
    public interface IDataObject
    {
        bool IsBinary { get; }
        IStreamFactory Source { get; }
        IStreamFactory StreamFactory { get; }
        long ActualLength { get; }
        IReadOnlyDictionary<IDataHashAlgorithm, byte[]> Hashes { get; }
        ArraySegment<byte> ByteValue { get; }
        string StringValue { get; }
        string Charset { get; }
        Encoding Encoding { get; }
        bool Recognized { get; }
        bool IsComplete { get; }
    }
}
