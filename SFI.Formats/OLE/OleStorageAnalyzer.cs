﻿using IS4.SFI.Services;
using OpenMcdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of OLE compound files as instances of <see cref="CompoundFile"/>.
    /// </summary>
    [Description("An analyzer of OLE compound files.")]
    public class OleStorageAnalyzer : MediaObjectAnalyzer<CompoundFile>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(CompoundFile file, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            IFileNodeInfo? Visitor(string path, CFItem item)
            {
                if(item.IsStream)
                {
                    return new FileEntry(path, (CFStream)item);
                }else if(item.IsStorage || item.IsRoot)
                {
                    var list = new List<IFileNodeInfo>();
                    var storage = (CFStorage)item;
                    storage.VisitEntries(item2 => {
                        var entry = Visitor(item.IsRoot ? path : path + item.Name + "/", item2);
                        if(entry == null) return;
                        list.Add(entry);
                    }, false);
                    return new DirectoryEntry(path, storage, list);
                }
                return null;
            }

            var node = GetNode(context);

            var info = Visitor("", file.RootStorage);
            
            switch(info)
            {
                case IDirectoryInfo dirInfo:
                    return await analyzers.Analyze(dirInfo, context.WithNode(node));
                case IFileInfo fileInfo:
                    return await analyzers.Analyze(fileInfo, context.WithNode(node));
                case null:
                    return default;
                default:
                    return await analyzers.Analyze(info, context.WithNode(node));
            }
        }

        abstract class ItemEntry<TItem> : IFileNodeInfo where TItem : CFItem
        {
            protected TItem Item { get; }

            public ItemEntry(string path, TItem item)
            {
                Item = item;
                Path = IsRoot ? path : path + item.Name;
            }

            public string? Name => IsRoot ? null : Item.Name;

            public string? SubName => null;

            public string? Path { get; }

            public int? Revision => null;

            public DateTime? CreationTime {
                get {
                    try{
                        return Item.CreationDate.ToFileTime() == 0 ? (DateTime?)null : Item.CreationDate.ToUniversalTime();
                    }catch{
                        return null;
                    }
                }
            }

            public DateTime? LastWriteTime {
                get {
                    try{
                        return Item.ModifyDate.ToFileTime() == 0 ? (DateTime?)null : Item.ModifyDate.ToUniversalTime();
                    }catch{
                        return null;
                    }
                }
            }

            public DateTime? LastAccessTime => null;

            public object? ReferenceKey => Item;

            public object? DataKey => null;

            public bool IsRoot => Item.IsRoot;

            public FileKind Kind => FileKind.Embedded;

            public abstract FileAttributes Attributes { get; }

            public override string ToString()
            {
                return IsRoot ? "" : "/" + Path;
            }
        }

        class DirectoryEntry : ItemEntry<CFStorage>, IDirectoryInfo
        {
            public IEnumerable<IFileNodeInfo> Entries { get; }

            public override FileAttributes Attributes => FileAttributes.Directory;

            public Environment.SpecialFolder? SpecialFolderType => null;

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

            public long Length => Item.Size;

            public override FileAttributes Attributes => FileAttributes.Normal;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public Stream Open()
            {
                return new MemoryStream(Item.GetData(), false);
            }
        }
    }
}
