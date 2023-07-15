using IS4.SFI.Application.Tools;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IS4.SFI.ConsoleApp
{
    internal class ClipboardStream : MappedStream
    {
        static readonly Encoding encoding = Encoding.UTF8;

        protected async override ValueTask Store(ArraySegment<byte> data)
        {
            var text = encoding.GetString(data);
            await StaThread.InvokeAsync(() => Clipboard.SetText(text, TextDataFormat.UnicodeText));
        }

        protected async override ValueTask Load()
        {
            var text = await StaThread.InvokeAsync(() => {
                var text = Clipboard.GetText(TextDataFormat.UnicodeText);
                if(String.IsNullOrEmpty(text))
                {
                    return Clipboard.GetText(TextDataFormat.Text);
                }
                return text;
            });
            using var writer = new StreamWriter(this, encoding: encoding, leaveOpen: true);
            writer.Write(text);
        }
    }
}
