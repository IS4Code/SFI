using IS4.MultiArchiver.Services;
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

        public ILinkedNode Analyze(FileInfo entity, ILinkedNodeFactory nodeFactory)
        {
            var pathUri = new Uri(entity.FullName);

            var node = nodeFactory.Root[Guid.NewGuid().ToString("D")];

            node.Set(Classes.FileDataObject);

            node.Set(Properties.Broader, Vocabularies.File, Uri.EscapeDataString(entity.Name));
            //handler.HandleTriple(fileNode, this[Properties.Broader], this[pathUri]);
            //handler.HandleTriple(this[pathUri], this[Properties.Broader], this[fileUri]);

            node.Set(Properties.FileName, entity.Name);
            node.Set(Properties.FileSize, entity.Length.ToString(), Datatypes.Integer);
            node.Set(Properties.FileCreated, entity.CreationTimeUtc);
            node.Set(Properties.FileLastModified, entity.LastWriteTimeUtc);
            node.Set(Properties.FileLastAccessed, entity.LastAccessTimeUtc);

            using(var stream = entity.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                var content = nodeFactory.Create(stream);
                if(content != null)
                {
                    content.Set(Properties.IsStoredAs, node);
                }
            }

            return node;
        }
    }
}
