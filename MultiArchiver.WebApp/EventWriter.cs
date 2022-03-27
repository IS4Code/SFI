using System;
using System.IO;
using System.Text;

namespace IS4.MultiArchiver.WebApp
{
    public class EventWriter : StringWriter
    {
        public event Action<StringBuilder> Written;

        public override void Flush()
        {
            base.Flush();
            var sb = GetStringBuilder();
            Written?.Invoke(sb);
            sb.Clear();
        }

        public override void Write(char value)
        {
            base.Write(value);
            Flush();
        }

        public override void Write(string value)
        {
            base.Write(value);
            Flush();
        }

        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
            Flush();
        }
    }
}
