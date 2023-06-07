using IS4.SFI.Application.Tools;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Windows.Forms;

namespace IS4.SFI.ConsoleApp
{
    internal class ClipboardStream : MappedStream
    {
        static readonly Encoding encoding = Encoding.UTF8;

        readonly TaskScheduler staTaskScheduler = new StaTaskScheduler(1);

        static void SetText(string text)
        {
            Clipboard.SetText(text, TextDataFormat.UnicodeText);
        }

        static string GetText()
        {
            var text = Clipboard.GetText(TextDataFormat.UnicodeText);
            if(String.IsNullOrEmpty(text))
            {
                return Clipboard.GetText(TextDataFormat.Text);
            }
            return text;
        }

        protected async override ValueTask Store(ArraySegment<byte> data)
        {
            var text = encoding.GetString(data);
            if(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                SetText(text);
            }else{
                await Task.Factory.StartNew(() => SetText(text), CancellationToken.None, 0, staTaskScheduler);
            }
        }

        protected async override ValueTask Load()
        {
            string text;
            if(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                text = GetText();
            }else{
                text = await Task.Factory.StartNew(GetText, CancellationToken.None, 0, staTaskScheduler);
            }
            using var writer = new StreamWriter(this, encoding: encoding, leaveOpen: true);
            writer.Write(text);
        }
    }
}
