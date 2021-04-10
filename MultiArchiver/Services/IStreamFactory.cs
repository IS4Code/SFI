using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IS4.MultiArchiver.Services
{
    public interface IStreamFactory
    {
        bool IsThreadSafe { get; }
        Stream Open();
    }
}
