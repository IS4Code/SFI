using IS4.SFI.Tools;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace IS4.SFI.ConsoleApp
{
    internal class ClipboardStream : MemoryStream
    {
        public override void Close()
        {
            var array = this.GetData();

            var text = Encoding.UTF8.GetString(array);

            if(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                Clipboard.SetText(text, TextDataFormat.UnicodeText);
            }else{
                var thread = new Thread(() => Clipboard.SetText(text, TextDataFormat.UnicodeText));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }

            base.Close();
        }
    }
}
