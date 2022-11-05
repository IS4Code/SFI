using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Windows.ComTypes;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using static Vanara.PInvoke.Shell32;
using IPersistStream = IS4.MultiArchiver.Windows.ComTypes.IPersistStream;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the Windows Shell Link format, producing instances of <see cref="IShellLinkW"/>.
    /// </summary>
    public class ShellLinkFormat : BinaryFileFormat<IShellLinkW>
    {
        const int headerLength = 76;
        static readonly byte[] headerGuid = { 0x01, 0x14, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46 };
        static readonly Type ShellLinkW = Type.GetTypeFromCLSID(new Guid(0x00021401, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46));

        public TaskScheduler StaTaskScheduler = new StaTaskScheduler(1);

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public ShellLinkFormat() : base(headerLength + 1, "application/x-ms-shortcut", "lnk")
        {
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            if(ShellLinkW == null) return false;
            if(header.Length <= headerLength) return false;
            var length = header.MemoryCast<int>()[0];
            if(length < headerLength) return false;
            var id = header.Slice(4, 16);
            if(!id.SequenceEqual(headerGuid.AsSpan())) return false;
            return true;
        }

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IShellLinkW, TResult, TArgs> resultFactory, TArgs args)
        {
            if(ShellLinkW == null) return default;

            var result = await Task.Factory.StartNew(() => {
                var link = (IShellLinkW)Activator.CreateInstance(ShellLinkW);
                try{
                    if(((IPersistStream)link).Load(new StreamWrapper(stream)) < 0) return default;
                    return resultFactory(link, args);
                }finally{
                    Marshal.FinalReleaseComObject(link);
                }
            }, CancellationToken.None, 0, StaTaskScheduler);
            if(result == null) return default;
            return await result;
        }
    }
}
