using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Services
{
    public interface IDataObject
    {
        bool IsBinary { get; }
        IStreamFactory Source { get; }
        IStreamFactory StreamFactory { get; }
        long ActualLength { get; }
        IReadOnlyDictionary<IDataHashAlgorithm, byte[]> Hashes { get; }
        ArraySegment<byte> Signature { get; }
        string Charset { get; }
        bool Recognized { get; }
    }
}
