using IS4.SFI.Application.Tools;
using Microsoft.JSInterop;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.WebApp
{
    internal class ClipboardStream : MappedStream
    {
        static readonly Encoding encoding = Encoding.UTF8;

        readonly IJSInProcessRuntime runtime;

        public ClipboardStream(IJSInProcessRuntime runtime)
        {
            this.runtime = runtime;
        }

        protected override ValueTask Store(ArraySegment<byte> data)
        {
            var text = encoding.GetString(data);
            return runtime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }

        protected async override ValueTask Load()
        {
            var text = await runtime.InvokeAsync<string>("navigator.clipboard.readText");
            using var writer = new StreamWriter(this, encoding: encoding, leaveOpen: true);
            writer.Write(text);
        }
    }
}
