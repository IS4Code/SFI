using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class FileAnalyzer : IEntityAnalyzer<FileInfo>
    {
        public FileAnalyzer()
        {

        }

        public ILinkedNode Analyze(FileInfo file, ILinkedNodeFactory nodeFactory)
        {
            var pathUri = new Uri(file.FullName);

            var node = nodeFactory.Root[Guid.NewGuid().ToString("D")];

            node.Set(Classes.FileDataObject);

            node.Set(Properties.Broader, Vocabularies.File, Uri.EscapeDataString(file.Name));
            //handler.HandleTriple(fileNode, this[Properties.Broader], this[pathUri]);
            //handler.HandleTriple(this[pathUri], this[Properties.Broader], this[fileUri]);

            node.Set(Properties.FileName, file.Name);
            node.Set(Properties.FileSize, file.Length.ToString(), Datatypes.Integer);
            node.Set(Properties.FileCreated, file.CreationTimeUtc);
            node.Set(Properties.FileLastModified, file.LastWriteTimeUtc);
            node.Set(Properties.FileLastAccessed, file.LastAccessTimeUtc);

            var content = nodeFactory.Create<Func<Stream>>(() => file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete));
            if(content != null)
            {
                content.Set(Properties.IsStoredAs, node);
            }

            return node;
        }
    }
}
