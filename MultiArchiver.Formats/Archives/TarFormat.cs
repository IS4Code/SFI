using IS4.MultiArchiver.Services;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace IS4.MultiArchiver.Formats
{
    public class TarFormat : BinaryFileFormat<IArchive>
    {
        public TarFormat() : base(headerLength, "application/x-tar", "tar")
        {

        }

        const int headerLength = 512;

        static readonly byte[] leadingDigits = Enumerable.Range('1', 9).Select(i => (byte)i).ToArray();

        static readonly byte[] terminator = { 0, (byte)' ' };

        public override bool Match(Span<byte> header)
        {
            if(header.Length < headerLength) return false;
            if(header.Slice(154, 2).IndexOfAny(terminator) != 0)
            {
                // Checksum not terminated
                return false;
            }
            // Locate the checksum
            var checksumSpan = header.Slice(148, 6);
            var finalDigit = checksumSpan[5];
            if(finalDigit < '0' || finalDigit > '7') return false;
            // Find the leading digit
            int start = checksumSpan.IndexOfAny(leadingDigits);
            for(int i = 0; i < start; i++)
            {
                var digit = checksumSpan[i];
                if(digit != 0 && digit != ' ' && digit != '0')
                {
                    // Unexpected padding character
                    return false;
                }
            }
            // Compute the stored value
            int targetChecksum = 0;
            for(int i = start; i < checksumSpan.Length; i++)
            {
                var digit = checksumSpan[i];
                if(digit < '0' || digit > '7')
                {
                    // Not an octal digit
                    return false;
                }
                targetChecksum = targetChecksum * 8 + (digit - '0');
            }
            if(targetChecksum > headerLength * Byte.MaxValue)
            {
                // Not a valid header checksum
                return false;
            }
            // Compute both unsigned and signed checksum
            int unsignedChecksum = 0;
            int signedChecksum = 0;
            var sheader = MemoryMarshal.Cast<byte, sbyte>(header);
            for(int i = 0; i < headerLength; i++)
            {
                if(i >= 148 && i <= 155)
                {
                    // Inside the checksum span
                    unsignedChecksum += ' ';
                    signedChecksum += ' ';
                }else{
                    unsignedChecksum += header[i];
                    signedChecksum += sheader[i];
                }
                if(i % (headerLength / 8) == 0)
                {
                    // Check if not too far from the target checksum
                    int unsignedDist = targetChecksum - unsignedChecksum;
                    int signedDist = targetChecksum - signedChecksum;
                    int remaining = headerLength - i - 1;
                    if(
                        (unsignedDist < 0 || unsignedDist > Byte.MaxValue * remaining) &&
                        (signedDist < SByte.MinValue * remaining || signedDist > SByte.MaxValue * remaining)
                    ) return false;
                }
            }
            return true;
        }

        public override TResult Match<TResult>(Stream stream, Func<IArchive, TResult> resultFactory)
        {
            if(TarArchive.IsTarFile(stream))
            {
                using(var archive = TarArchive.Open(stream))
                {
                    if(archive == null)
                    {
                        return null;
                    }
                    if(archive.TotalSize <= 0 || archive.TotalUncompressSize <= 0 || archive.TotalUncompressSize > 1024 * (long)Int32.MaxValue)
                    {
                        return null;
                    }
                    /*if(archive.TotalUncompressSize != archive.Entries.Sum(e => e.Size))
                    {
                        return null;
                    }
                    if(archive.Entries.Any(e => e.Key.Contains('\0')))
                    {
                        return null;
                    }*/
                    return resultFactory(archive);
                }
            }
            return null;
        }
    }
}
