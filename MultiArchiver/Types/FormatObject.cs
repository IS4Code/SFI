using IS4.MultiArchiver.Services;
using System;

namespace IS4.MultiArchiver.Types
{
    public class FormatObject : IDisposable
    {
        public IFileFormat Format { get; }
        public IDisposable Value { get; private set; }

        public FormatObject(IFileFormat format, IDisposable value)
        {
            Format = format;
            Value = value;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(Value != null)
            {
                if(disposing)
                {
                    Value.Dispose();
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
