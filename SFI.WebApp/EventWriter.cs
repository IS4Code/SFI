using System;
using System.IO;
using System.Text;

namespace IS4.SFI.WebApp
{
    /// <summary>
    /// An implementation of <see cref="TextWriter"/> which reports the written
    /// strings via an event.
    /// </summary>
    public class EventWriter : StringWriter
    {
        /// <summary>
        /// This event is fired each time text is written to the writer.
        /// </summary>
        public event Action<StringBuilder>? Written;

        /// <inheritdoc/>
        public override void Flush()
        {
            base.Flush();
            var sb = GetStringBuilder();
            Written?.Invoke(sb);
            sb.Clear();
        }

        /// <inheritdoc/>
        public override void Write(char value)
        {
            base.Write(value);
            Flush();
        }

        /// <inheritdoc/>
        public override void Write(string? value)
        {
            base.Write(value);
            Flush();
        }

        /// <inheritdoc/>
        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
            Flush();
        }
    }
}
