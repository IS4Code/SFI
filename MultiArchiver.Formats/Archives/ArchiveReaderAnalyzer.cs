using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var directories = new Dictionary<string, ArchiveDirectoryInfo>();

            while(reader.MoveToNextEntry())
            {
                var entry = reader.Entry;
                IFileNodeInfo info;
                string dir;
                if(entry.IsDirectory)
                {
                    info = new ArchiveDirectoryInfo(reader, entry);
                    if(!directories.TryGetValue(info.Path, out var dirInfo))
                    {
                        directories[info.Path] = (ArchiveDirectoryInfo)info;
                    }else{
                        dirInfo.Entry = entry;
                    }
                    dir = GetDirectory(info.Path);
                }else{
                    info = new ArchiveFileInfo(reader, entry);
                    dir = GetDirectory(info.Path);
                    var container = GetContainer(parent, dir);
                    var node = nodeFactory.Create(container, info);
                    if(dir == null) node?.Set(Properties.BelongsToContainer, parent);
                    //node?.Set(Properties.BelongsToContainer, dir == null ? parent : container[""]);
                }
                if(dir != null)
                {
                    if(!directories.TryGetValue(dir, out var dirInfo))
                    {
                        dirInfo = directories[dir] = new ArchiveDirectoryInfo(reader, null);
                    }
                    dirInfo.Entries.Add(entry.IsDirectory ? info : new ArchiveEntryInfo(reader, entry));
                    dir = GetDirectory(dir);
                    while(dir != null)
                    {
                        if(directories.TryGetValue(dir, out var parentDirInfo))
                        {
                            break;
                        }
                        parentDirInfo = directories[dir] = new ArchiveDirectoryInfo(reader, null);
                        parentDirInfo.Entries.Add(dirInfo);
                        dir = GetDirectory(dir);
                        dirInfo = parentDirInfo;
                    }
                }
            }

            foreach(var pair in directories)
            {
                var info = pair.Value;
                var dir = GetDirectory(info.Path);
                var container = GetContainer(parent, dir);
                var node = nodeFactory.Create(container, info);
                if(dir == null) node?.Set(Properties.BelongsToContainer, parent);
            }

            return false;
        }

        static ILinkedNode GetContainer(ILinkedNode parent, string dir)
        {
            return dir == null ? parent : parent[String.Join("/", dir.Split('/').Select(Uri.EscapeDataString))];
        }

        static string GetDirectory(string path)
        {
            if(path == null) return null;
            var nameIndex = path.LastIndexOf('/');
            if(nameIndex != -1) return path.Substring(0, nameIndex);
            return null;
        }
        
        class ArchiveEntryInfo : IFileNodeInfo
        {
            protected IReader Reader { get; }
            public IEntry Entry { get; set; }

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
            public List<IFileNodeInfo> Entries { get; } = new List<IFileNodeInfo>();

            public ArchiveDirectoryInfo(IReader reader, IEntry entry) : base(reader, entry)
            {

            }

            IEnumerable<IFileNodeInfo> IDirectoryInfo.Entries => Entries;
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
