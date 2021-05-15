using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Windows;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using static Vanara.PInvoke.Url;
using IPersistStream = IS4.MultiArchiver.Windows.IPersistStream;

namespace IS4.MultiArchiver.Formats
{
    public class InternetShortcutFormat : BinaryFileFormat<IUniformResourceLocator>
    {
        static readonly Type InternetShortcut = Type.GetTypeFromCLSID(new Guid(0xFBF23B40, 0xE3F0, 0x101B, 0x84, 0x88, 0x00, 0xAA, 0x00, 0x3E, 0x56, 0xF8));
        
        public InternetShortcutFormat() : base(0, "text/x-uri", "url")
        {

        }

        public override bool CheckHeader(Span<byte> header)
        {
            return true;
        }

        public override TResult Match<TResult>(Stream stream, Func<IUniformResourceLocator, TResult> resultFactory)
        {
            TResult Inner()
            {
                var shortcut = (IUniformResourceLocator)Activator.CreateInstance(InternetShortcut);
                try{
                    if(((IPersistStream)shortcut).Load(new StreamWrapper(stream)) < 0) return null;
                    shortcut.GetUrl(out var str);
                    if(String.IsNullOrEmpty(str)) return null;
                    return resultFactory(shortcut);
                }finally{
                    Marshal.FinalReleaseComObject(shortcut);
                }
            }

            if(Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                TResult result = null;
                Exception exc = null;
                var thread = new Thread(() => {
                    try{
                        result = Inner();
                    }catch(Exception e)
                    {
                        exc = e;
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
                thread.Join();
                if(exc != null)
                {
                    ExceptionDispatchInfo.Capture(exc).Throw();
                }
                return result;
            }

            return Inner();
        }
    }
}
