using System;

namespace IS4.SFI.Media.Modules
{
    /// <summary>
    /// Stores the VS_VERSIONINFO structure.
    /// </summary>
    public class WinVersionInfo
    {
        /// <summary>
        /// The buffer storing the bytes of the structure.
        /// </summary>
        public ArraySegment<byte> Data { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="data">The value of <see cref="Data"/>.</param>
        public WinVersionInfo(ArraySegment<byte> data)
        {
            Data = data;
        }
    }
}
