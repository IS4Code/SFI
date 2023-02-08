using IS4.SFI.Tools.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// The tests for <see cref="BitmapDataStream"/>.
    /// </summary>
    [TestClass]
    public class BitmapDataStreamTests
    {
        /// <summary>
        /// The tests for <see cref="BitmapDataStream.Read(byte[], int, int)"/>,
        /// checking randomly-generated data and both positive and negative stride.
        /// </summary>
        [DataRow(0, false)]
        [DataRow(402927094, false)]
        [DataRow(452854933, false)]
        [DataRow(1955186690, false)]
        [DataRow(862248498, false)]
        [DataRow(2112741252, false)]
        [DataRow(1363995148, false)]
        [DataRow(2093213676, false)]
        [DataRow(503564212, false)]
        [DataRow(0, true)]
        [DataRow(402927094, true)]
        [DataRow(452854933, true)]
        [DataRow(1955186690, true)]
        [DataRow(862248498, true)]
        [DataRow(2112741252, true)]
        [DataRow(1363995148, true)]
        [DataRow(2093213676, true)]
        [DataRow(503564212, true)]
        [TestMethod]
        public void ReadTests(int seed, bool negativeStride)
        {
            var rnd = new Random(seed);

            var height = rnd.Next(1, 200);
            var width = rnd.Next(1, 200);
            var stride = (width + rnd.Next(0, 100)) * (negativeStride ? -1 : 1);

            var data = new byte[width * height];
            rnd.NextBytes(data);

            var ptr = Marshal.AllocHGlobal(Math.Abs(stride) * height);
            try
            {
                var scan0 = ptr + (negativeStride
                    ? -stride * (height - 1)
                    : 0);

                for(int i = 0; i < height; i++)
                {
                    Marshal.Copy(data, i * width, scan0 + i * stride, width);
                }

                using(var stream = new BitmapDataStream(scan0, stride, height, width))
                {
                    using var buffer = new MemoryStream();
                    stream.CopyTo(buffer);

                    CollectionAssert.AreEqual(data, buffer.ToArray());
                }
            } finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
