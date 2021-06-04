using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using OpenMcdf;
using System;
using System.Collections.Generic;
using System.IO;

namespace IS4.MultiArchiver.Analyzers
{
    public class OleStorageAnalyzer : BinaryFormatAnalyzer<CompoundFile>
    {
        public override string Analyze(ILinkedNode node, CompoundFile file, ILinkedNodeFactory nodeFactory)
        {
            IFileNodeInfo Visitor(string path, CFItem item)
            {
                if(item.IsStream)
                {
                    return new FileEntry(path, (CFStream)item);
                }else if(item.IsStorage)
                {
                    var list = new List<IFileNodeInfo>();
                    var storage = ((CFStorage)item);
                    storage.VisitEntries(item2 => list.Add(Visitor(path + item.Name + "/", item2)), false);
                    return new DirectoryEntry(path, storage, list);
                }
                return null;
            }

            file.RootStorage.VisitEntries(item => {
                var info = Visitor(null, item);
                var fileNode = nodeFactory.Create(node, info);
                if(fileNode != null)
                {
                    node.Set(Properties.HasMediaStream, fileNode);
                }
            }, false);

            node.Set(Properties.Modified, file.RootStorage.ModifyDate);

            return null;
        }

        abstract class ItemEntry<TItem> : IFileNodeInfo where TItem : CFItem
        {
            protected TItem Item { get; }

            public ItemEntry(string path, TItem item)
            {
                Item = item;
                Path = path + item.Name;
            }

            public string Name => Item.Name;

            public string SubName => null;

            public string Path { get; }

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => Item.ModifyDate;

            public DateTime? LastAccessTime => null;

            public object ReferenceKey => Item;

            public object DataKey => null;

            public override string ToString()
            {
                return "/" + Path;
            }
        }

        class DirectoryEntry : ItemEntry<CFStorage>, IDirectoryInfo
        {
            public IEnumerable<IFileNodeInfo> Entries { get; }

            public DirectoryEntry(string path, CFStorage item, IEnumerable<IFileNodeInfo> entries) : base(path, item)
            {
                Entries = entries;
            }
        }

        class FileEntry : ItemEntry<CFStream>, IFileInfo
        {
            public FileEntry(string path, CFStream item) : base(path, item)
            {

            }

            public bool IsEncrypted => false;

            public long Length => Item.Size;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public Stream Open()
            {
                return new MemoryStream(Item.GetData(), false);
            }
        }
    }
}
