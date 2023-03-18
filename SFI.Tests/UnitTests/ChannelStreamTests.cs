using IS4.SFI.Tools.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// The tests for <see cref="ChannelStream{TSequence}"/>.
    /// </summary>
    [TestClass]
    public class ChannelStreamTests
    {
        /// <summary>
        /// The tests for <see cref="ChannelStream{TSequence}.Read(byte[], int, int)"/>,
        /// checking randomly-generated data.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(402927094)]
        [DataRow(452854933)]
        [DataRow(1955186690)]
        [DataRow(862248498)]
        [DataRow(2112741252)]
        [DataRow(1363995148)]
        [DataRow(2093213676)]
        [DataRow(503564212)]
        public async Task ReadTests(int seed)
        {
            var stream = ChannelArrayStream.Create(out var writer);

            var expectedStream = new MemoryStream();
            var rnd = new Random(seed);
            int count = rnd.Next(10, 50);
            for(int i = 0; i < count; i++)
            {
                int size = rnd.Next(100, 500);
                var bytes = new byte[size];
                rnd.NextBytes(bytes);

                int split = rnd.Next(0, size);

                await writer.WriteAsync(new ArraySegment<byte>(bytes, 0, split));
                await writer.WriteAsync(new ArraySegment<byte>(bytes, split, bytes.Length - split));

                expectedStream.Write(bytes, 0, bytes.Length);
            }

            writer.Complete();

            var actualStream = new MemoryStream();
            stream.CopyTo(actualStream);

            CollectionAssert.AreEqual(expectedStream.ToArray(), actualStream.ToArray());
        }
    }
}
