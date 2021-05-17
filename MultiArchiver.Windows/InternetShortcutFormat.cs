using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Windows;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using static Vanara.PInvoke.Url;
using IPersistStream = IS4.MultiArchiver.Windows.IPersistStream;

namespace IS4.MultiArchiver.Formats
{
    public class InternetShortcutFormat : BinaryFileFormat<IUniformResourceLocator>
    {
        static readonly Type InternetShortcut = Type.GetTypeFromCLSID(new Guid(0xFBF23B40, 0xE3F0, 0x101B, 0x84, 0x88, 0x00, 0xAA, 0x00, 0x3E, 0x56, 0xF8));

        public TaskScheduler StaTaskScheduler = new StaTaskScheduler(1);

        public InternetShortcutFormat() : base(0, "text/x-uri", "url")
        {

        }

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return !isBinary;
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return !isBinary;
        }

        public override TResult Match<TResult>(Stream stream, Func<IUniformResourceLocator, TResult> resultFactory)
        {
            if(InternetShortcut == null) return null;

            return Task.Factory.StartNew(() => {
                var shortcut = (IUniformResourceLocator)Activator.CreateInstance(InternetShortcut);
                try{
                    if(((IPersistStream)shortcut).Load(new StreamWrapper(stream)) < 0) return null;
                    shortcut.GetUrl(out var str);
                    if(String.IsNullOrEmpty(str)) return null;
                    return resultFactory(shortcut);
                }finally{
                    Marshal.FinalReleaseComObject(shortcut);
                }
            }, CancellationToken.None, 0, StaTaskScheduler).Result;
        }
    }
}
