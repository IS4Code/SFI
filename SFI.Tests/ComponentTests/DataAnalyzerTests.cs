using IS4.SFI.Analyzers;
using IS4.SFI.Application.Tools;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// Tests for <see cref="DataAnalyzer"/>.
    /// </summary>
    [TestClass]
    public class DataAnalyzerTests : AnalyzerTests
    {
        const int minDataLength = 1024;

        /// <summary>
        /// The analyzer instance to use.
        /// </summary>
        public DataAnalyzer Analyzer { get; } = new DataAnalyzer(() => new UdeEncodingDetector())
        {
            MinDataLengthToStore = minDataLength
        };

        const string nonAsciiText = "Lákamí vůněhulás úmyval rohlivý jednovod";

        /// <summary>
        /// Tests that text and charset is correctly recognized.
        /// </summary>
        /// <param name="text">The text to encode and detect.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(nonAsciiText, "ascii")]
        [DataRow(nonAsciiText, "utf-8")]
        [DataRow(nonAsciiText, "utf-16")]
        [DataRow(nonAsciiText, "unicodeFFFE")]
        [DataRow(nonAsciiText, "windows-1252")]
        public async Task TextCharset(string text, string encoding)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var enc = Encoding.GetEncoding(encoding);
            var data = enc.GetPreamble();
            var bytes = enc.GetBytes(text);
            var offset = data.Length;
            Array.Resize(ref data, offset + bytes.Length);
            bytes.CopyTo(data, offset);

            await Analyzer.Analyze(data, Context, this);
            var dataObject = GetOutput<IDataObject>(out _);

            Assert.AreEqual(data.Length, dataObject.ActualLength);
            Assert.IsTrue(dataObject.IsComplete);
            Assert.IsFalse(dataObject.IsBinary);
            Assert.IsNotNull(dataObject.Encoding);
            Assert.AreEqual(enc.WebName, dataObject.Encoding.WebName);
            Assert.AreEqual(enc.GetString(bytes), dataObject.StringValue);
        }

        /// <summary>
        /// Tests that the value of <see cref="IDataObject.IsComplete"/>
        /// is affected by the length of the data.
        /// </summary>
        /// <param name="size">The size of the data.</param>
        /// <param name="complete">Whether the object should be complete.</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(minDataLength - 2, true)]
        [DataRow(minDataLength - 1, true)]
        [DataRow(minDataLength, true)]
        [DataRow(minDataLength + 1, false)]
        [DataRow(minDataLength + 2, false)]
        public async Task CompleteBasedOnSize(int size, bool complete)
        {
            var data = new byte[size];
            data.AsSpan().MemoryCast<int>().Fill(size);

            await Analyzer.Analyze(data, Context, this);
            var dataObject = GetOutput<IDataObject>(out _);

            Assert.AreEqual(data.Length, dataObject.ActualLength);
            Assert.AreEqual(complete, dataObject.IsComplete);
            Assert.AreEqual(Math.Min(size, minDataLength + 1), dataObject.ByteValue.Count);
            if(complete)
            {
                Assert.IsTrue(dataObject.ByteValue.AsSpan().SequenceEqual(data.AsSpan()));
            }
        }

        /// <summary>
        /// Tests that hashes are applied for incompletely-stored data.
        /// </summary>
        /// <param name="size">The size of the data.</param>
        /// <param name="shouldStore">Whether hashes should be stored.</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(minDataLength - 2, false)]
        [DataRow(minDataLength - 1, false)]
        [DataRow(minDataLength, false)]
        [DataRow(minDataLength + 1, true)]
        [DataRow(minDataLength + 2, true)]
        public async Task Hashes(int size, bool shouldStore)
        {
            Analyzer.HashAlgorithms.Add(BuiltInHash.SHA1!);
            Analyzer.HashAlgorithms.Add(BuiltInHash.SHA256!);
            Analyzer.HashAlgorithms.Add(BuiltInHash.SHA512!);

            var data = new byte[size];

            await Analyzer.Analyze(data, Context, this);
            var dataObject = GetOutput<IDataObject>(out _);

            Assert.AreEqual(data.Length, dataObject.ActualLength);
            Assert.IsTrue(dataObject.IsBinary);
            if(!shouldStore)
            {
                Assert.AreEqual(0, dataObject.Hashes.Count);
            }else{
                Assert.AreEqual(Analyzer.HashAlgorithms.Count, dataObject.Hashes.Count);
                foreach(var hash in Analyzer.HashAlgorithms)
                {
                    Assert.IsTrue(dataObject.Hashes.TryGetValue(hash, out var computed));
                    var expected = BitConverter.ToString(await hash.ComputeHash(data));
                    Assert.AreEqual(expected, BitConverter.ToString(computed!));
                }
            }
        }
    }
}
