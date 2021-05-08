using System;
using System.Collections.Generic;
using System.IO;
using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveReaderAnalyzer : BinaryFormatAnalyzer<IReader>
    {
        public ArchiveReaderAnalyzer() : base(Classes.Archive)
        {

        }

        public override bool Analyze(ILinkedNode parent, IReader reader, ILinkedNodeFactory nodeFactory)
        {
            var remainingDirectories = new HashSet<string>();
            var visitedDirectories = new HashSet<string>();

            while(reader.MoveToNextEntry())
            {
                var entry = reader.Entry;
                var info = entry.IsDirectory ? (IFileNodeInfo)new ArchiveDirectoryInfo(reader, entry) : new ArchiveFileInfo(reader, entry);
                var dir = GetDirectory(info.Path);
                if(dir != null && !visitedDirectories.Contains(dir))
                {
                    remainingDirectories.Add(dir);
                }
                var container = dir == null ? parent : parent[Uri.EscapeUriString(dir)];
                var node = nodeFactory.Create(container, info);
                node.Set(Properties.BelongsToContainer, dir == null ? parent : container[""]);
                if(entry.IsDirectory)
                {
                    visitedDirectories.Add(info.Path);
                }
            }

            if(remainingDirectories.Count > 0)
            {
                var queue = new Queue<string>(remainingDirectories);

                while(queue.Count > 0)
                {
                    var path = queue.Dequeue();
                    if(!visitedDirectories.Contains(path))
                    {
                        var dir = GetDirectory(path);
                        if(dir != null && !visitedDirectories.Contains(dir))
                        {
                            queue.Enqueue(dir);
                        }
                        var container = dir == null ? parent : parent[Uri.EscapeUriString(dir)];
                        var node = nodeFactory.Create(container, new BlankDirectoryInfo(reader, path));
                        node.Set(Properties.BelongsToContainer, dir == null ? parent : container[""]);
                        visitedDirectories.Add(path);
                    }
                }
            }
            return false;
        }

        static string GetDirectory(string path)
        {
            if(path == null) return null;
            var nameIndex = path.LastIndexOf('/');
            if(nameIndex != -1) return path.Substring(0, nameIndex);
            return null;
        }
        
        abstract class ArchiveEntryInfo : IFileNodeInfo
        {
            protected IReader Reader { get; }
            protected IEntry Entry { get; }

            public ArchiveEntryInfo(IReader reader, IEntry entry)
            {
                Reader = reader;
                Entry = entry;
            }

            public virtual string Name => Entry != null ? System.IO.Path.GetFileName(Path) : null;

            public virtual string Path => ArchiveAnalyzer.ExtractPathSimple(Entry);

            public DateTime? CreationTime => Entry?.CreatedTime;

            public DateTime? LastWriteTime => Entry?.LastModifiedTime;

            public DateTime? LastAccessTime => Entry?.LastAccessedTime;

            public int? Revision => null;

            object IPersistentKey.ReferenceKey => Reader;

            object IPersistentKey.DataKey => Path;
        }

        class ArchiveFileInfo : ArchiveEntryInfo, IFileInfo
        {
            readonly Lazy<ArraySegment<byte>> data;

            public ArchiveFileInfo(IReader reader, IEntry entry) : base(reader, entry)
            {
                data = new Lazy<ArraySegment<byte>>(() => {
                    using(var stream = new MemoryStream())
                    {
                        using(var input = Reader.OpenEntryStream())
                        {
                            input.CopyTo(stream);
                        }
                        if(!stream.TryGetBuffer(out var buffer))
                        {
                            buffer = new ArraySegment<byte>(stream.ToArray());
                        }
                        return buffer;
                    }
                });
            }

            public long Length => Entry.Size;

            public bool IsThreadSafe => true;

            public Stream Open()
            {
                var data = this.data.Value;
                return new MemoryStream(data.Array, data.Offset, data.Count, false);
            }
        }

        class ArchiveDirectoryInfo : ArchiveEntryInfo, IDirectoryInfo
        {
            public ArchiveDirectoryInfo(IReader reader, IEntry entry) : base(reader, entry)
            {

            }

            public IEnumerable<IFileNodeInfo> Entries => Array.Empty<IFileNodeInfo>();
        }

        class BlankDirectoryInfo : ArchiveDirectoryInfo
        {
            public BlankDirectoryInfo(IReader reader, string path) : base(reader, null)
            {
                Path = path;
            }

            public override string Path { get; }
        }
    }
}
