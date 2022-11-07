using IS4.SFI.Analyzers;
using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static IS4.SFI.Vocabulary.Properties;

namespace IS4.SFI.Tests.Analyzers
{
    /// <summary>
    /// Tests for <see cref="FileAnalyzer"/>.
    /// </summary>
    [TestClass]
    public class FileAnalyzerTests : AnalyzerTests
    {
        /// <summary>
        /// The analyzer instance to use.
        /// </summary>
        public FileAnalyzer Analyzer { get; } = new FileAnalyzer();

        /// <summary>
        /// Tests that properties of the file are stored.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FileProperties()
        {
            var date = DateTime.UtcNow;
            var file = new File(new ArraySegment<byte>(new byte[100]), null)
            {
                Name = "TestFile",
                Revision = 10,
                CreationTime = date,
                LastWriteTime = date.AddSeconds(10),
                LastAccessTime = date.AddSeconds(20)
            };

            var result = await Analyzer.Analyze((IFileNodeInfo)file, Context, this);

            Assert.AreSame(Node, result.Node);
            Assert.AreEqual(file.Name, Node[FileName]);
            Assert.AreEqual(file.Revision, Node[Properties.Version]);
            Assert.AreEqual(file.CreationTime, Node[FileCreated]);
            Assert.AreEqual(file.LastWriteTime, Node[FileLastModified]);
            Assert.AreEqual(file.LastAccessTime, Node[FileLastAccessed]);

            Assert.AreSame(file, GetOutput<IStreamFactory>(out _));
        }

        abstract class FileNode : IFileNodeInfo
        {
            public Directory? Parent { get; }

            public FileNode(Directory? parent)
            {
                Parent = parent;
            }

            public string? Name { get; set; }
            public string? SubName { get; set; }
            public string? Path => (Parent != null ? Parent.Path + "/" : "") + Name;
            public int? Revision { get; set; }
            public DateTime? CreationTime { get; set; }
            public DateTime? LastWriteTime { get; set; }
            public DateTime? LastAccessTime { get; set; }

            public FileKind Kind => FileKind.None;
            public object? ReferenceKey => this;
            public object? DataKey => null;

            public override string ToString()
            {
                return "/" + Path;
            }
        }

        class File : FileNode, IFileInfo
        {
            public bool IsEncrypted { get; set; }

            readonly ArraySegment<byte> data;

            public File(ArraySegment<byte> data, Directory? parent) : base(parent)
            {
                this.data = data;
            }

            public long Length => data.Count;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public Stream Open()
            {
                return new MemoryStream(data.Array!, data.Offset, data.Count);
            }
        }

        class Directory : FileNode, IDirectoryInfo
        {
            IEnumerable<IFileNodeInfo> IDirectoryInfo.Entries => Entries;

            public HashSet<FileNode> Entries { get; } = new HashSet<FileNode>();

            public Directory(Directory? parent) : base(parent)
            {

            }
        }
    }
}
