using System;

namespace IS4.MultiArchiver.Services
{
    public interface IUriFormatter<T>
    {
        Uri FormatUri(T value);
    }
}
