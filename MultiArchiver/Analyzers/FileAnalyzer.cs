using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class FileAnalyzer : IEntityAnalyzer<IFileInfo>, IEntityAnalyzer<FileInfo>
    {
        public FileAnalyzer()
        {

        }

        public ILinkedNode Analyze(IFileInfo file, ILinkedNodeFactory nodeFactory)
        {
            var name = Uri.EscapeUriString(file.Name);
            var node = file.Container?[name] ?? nodeFactory.Root[Guid.NewGuid().ToString("D")];

            node.SetClass(Classes.FileDataObject);

            node.Set(Properties.Broader, Vocabularies.File, name);
            //handler.HandleTriple(fileNode, this[Properties.Broader], this[pathUri]);
            //handler.HandleTriple(this[pathUri], this[Properties.Broader], this[fileUri]);

            node.Set(Properties.FileName, file.Name);
            if(file.Length is long len)
            {
                node.Set(Properties.FileSize, len);
            }
            if(file.CreationTime is DateTime dt1)
            {
                node.Set(Properties.FileCreated, dt1);
            }
            if(file.LastWriteTime is DateTime dt2)
            {
                node.Set(Properties.FileLastModified, dt2);
            }
            if(file.LastAccessTime is DateTime dt3)
            {
                node.Set(Properties.FileLastAccessed, dt3);
            }

            var content = nodeFactory.Create<Func<Stream>>(() => file.Open());
            if(content != null)
            {
                content.Set(Properties.IsStoredAs, node);
            }

            return node;
        }

        public ILinkedNode Analyze(FileInfo entity, ILinkedNodeFactory nodeFactory)
        {
            return Analyze(new FileInfoWrapper(entity), nodeFactory);
        }

        class FileInfoWrapper : IFileInfo
        {
            readonly FileInfo baseInfo;

            public FileInfoWrapper(FileInfo baseInfo)
            {
                this.baseInfo = baseInfo;
            }

            public string Name => baseInfo.Name;

            public string Path => baseInfo.FullName.Substring(System.IO.Path.GetPathRoot(baseInfo.FullName).Length);

            public long? Length => baseInfo.Length;

            public DateTime? CreationTime => baseInfo.CreationTimeUtc;

            public DateTime? LastWriteTime => baseInfo.LastWriteTimeUtc;

            public DateTime? LastAccessTime => baseInfo.LastAccessTimeUtc;

            public ILinkedNode Container => null;

            public Stream Open()
            {
                return baseInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            }
        }
    }
}
