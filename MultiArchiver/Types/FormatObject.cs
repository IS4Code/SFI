using IS4.MultiArchiver.Services;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Types
{
    public class FormatObject : IDisposable
    {
        readonly IFileFormat format;
        Task<object> task;

        public object Value => task.Result;

        public string Extension => format is IFileLoader loader ? loader.GetExtension((IDisposable)Value) : format.Extension;
        public string MediaType => format is IFileLoader loader ? loader.GetMediaType((IDisposable)Value) : format.MediaType;

        public FormatObject(IFileFormat format, Task<object> task)
        {
            this.format = format;
            this.task = task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(task != null)
            {
                if(disposing && Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                task = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
