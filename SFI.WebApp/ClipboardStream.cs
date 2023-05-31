using IS4.SFI.Tools;
using Microsoft.JSInterop;
using System.IO;
using System.Text;

namespace IS4.SFI.WebApp
{
    internal class ClipboardStream : MemoryStream
    {
        readonly IJSInProcessRuntime runtime;

        public ClipboardStream(IJSInProcessRuntime runtime)
        {
            this.runtime = runtime;
        }

        public override void Close()
        {
            var array = this.GetData();

            var text = Encoding.UTF8.GetString(array);

            runtime.InvokeVoid("navigator.clipboard.writeText", text);

            base.Close();
        }
    }
}
