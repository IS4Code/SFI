using System;

namespace IS4.MultiArchiver.Media.Modules
{
    public class WinVersionInfo
    {
        public ArraySegment<byte> Data { get; }

        public WinVersionInfo(ArraySegment<byte> data)
        {
            Data = data;
        }
    }
}
