using IS4.MultiArchiver.Services;
using System;

namespace IS4.MultiArchiver.Types
{
    public class FormatObject : IDisposable
    {
        readonly IFileFormat format;
        public object Value { get; private set; }

        public string Extension => format is IFileLoader loader ? loader.GetExtension((IDisposable)Value) : format.Extension;
        public string MediaType => format is IFileLoader loader ? loader.GetMediaType((IDisposable)Value) : format.MediaType;

        public FormatObject(IFileFormat format, object value)
        {
            this.format = format;
            Value = value;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(Value != null)
            {
                if(disposing && Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                Value = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
