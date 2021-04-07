using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using VDS.RDF;

namespace IS4.MultiArchiver.Analyzers
{
    public class FileAnalyzer : RdfBase, IEntityAnalyzer<FileInfo>
    {
        public FileAnalyzer(INodeFactory nodeFactory) : base(nodeFactory)
        {

        }

        public IUriNode Analyze(FileInfo entity, IRdfHandler handler, IEntityAnalyzer baseAnalyzer)
        {
            var pathUri = new Uri(entity.FullName);
            var fileUri = new Uri("file:///" + Uri.EscapeDataString(entity.Name), UriKind.Absolute);

            var node = handler.CreateUriNode(new Uri(Vocabularies.ArchiveId + Guid.NewGuid().ToString("D")));

            handler.HandleTriple(node, this[Properties.Type], this[Classes.FileDataObject]);

            handler.HandleTriple(node, this[Properties.Broader], this[fileUri]);
            //handler.HandleTriple(fileNode, this[Properties.Broader], this[pathUri]);
            //handler.HandleTriple(this[pathUri], this[Properties.Broader], this[fileUri]);

            handler.HandleTriple(node, this[Properties.FileName], this[entity.Name]);
            handler.HandleTriple(node, this[Properties.FileSize], this[entity.Length.ToString(), Datatypes.Integer]);
            handler.HandleTriple(node, this[Properties.FileCreated], this[entity.CreationTimeUtc]);
            handler.HandleTriple(node, this[Properties.FileLastModified], this[entity.LastWriteTimeUtc]);
            handler.HandleTriple(node, this[Properties.FileLastAccessed], this[entity.LastAccessTimeUtc]);

            using(var stream = entity.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                var content = baseAnalyzer.Analyze(stream, handler);
                if(content != null)
                {
                    handler.HandleTriple(content, this[Properties.IsStoredAs], node);
                }
            }

            return node;
        }
    }
}
