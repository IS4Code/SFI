﻿using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class FileAnalyzer : IEntityAnalyzer<FileInfo>, IEntityAnalyzer<DirectoryInfo>, IEntityAnalyzer<IFileInfo>, IEntityAnalyzer<IDirectoryInfo>, IEntityAnalyzer<IFileNodeInfo>
    {
        public ICollection<IFileHashAlgorithm> HashAlgorithms { get; } = new List<IFileHashAlgorithm>();

        public FileAnalyzer()
        {

        }

        private ILinkedNode CreateNode(ILinkedNode parent, IFileNodeInfo info, ILinkedNodeFactory nodeFactory)
        {
            var name = Uri.EscapeDataString(info.Name ?? "");
            if(info.SubName is string subName)
            {
                name += ":" + Uri.EscapeDataString(subName);
            }
            return parent?[name] ?? nodeFactory.NewGuidNode();
        }

        private ILinkedNode AnalyzeFileNode(ILinkedNode parent, IFileNodeInfo info, ILinkedNodeFactory nodeFactory)
        {
            var node = CreateNode(parent, info, nodeFactory);

            node.SetClass(Classes.FileDataObject);

            if(info.Path != null)
            {
                LinkDirectories(node, info.Path, false, nodeFactory);
            }else if(info.Name != null)
            {
                LinkDirectories(node, info.Name, false, nodeFactory);
            }
            node.Set(Properties.PrefLabel, info.ToString());

            if(info.Name != null)
            {
                node.Set(Properties.FileName, info.Name);
            }

            if(info.CreationTime is DateTime dt1)
            {
                node.Set(Properties.FileCreated, dt1);
            }
            if(info.LastWriteTime is DateTime dt2)
            {
                node.Set(Properties.FileLastModified, dt2);
            }
            if(info.LastAccessTime is DateTime dt3)
            {
                node.Set(Properties.FileLastAccessed, dt3);
            }
            if(info.Revision is int rev)
            {
                node.Set(Properties.Version, rev);
            }

            return node;
        }

        public ILinkedNode Analyze(ILinkedNode parent, IFileInfo file, ILinkedNodeFactory nodeFactory)
        {
            var node = AnalyzeFileNode(parent, file, nodeFactory);
            if(node != null)
            {
                if(file.Length >= 0)
                {
                    node.Set(Properties.FileSize, file.Length);
                }

                if(file.IsEncrypted)
                {
                    node.Set(Properties.EncryptionStatus, Individuals.EncryptedStatus);
                }else{
                    var content = nodeFactory.Create<IStreamFactory>(node, file);
                    if(content != null)
                    {
                        content.Set(Properties.IsStoredAs, node);
                    }
                }

                foreach(var alg in HashAlgorithms)
                {
                    HashAlgorithm.AddHash(node, alg, alg.ComputeHash(file), nodeFactory);
                }

                if(file is IDirectoryInfo directory)
                {
                    AnalyzeDirectory(node, directory, nodeFactory);
                }
            }
            return node;
        }

        private void AnalyzeDirectory(ILinkedNode node, IDirectoryInfo directory, ILinkedNodeFactory nodeFactory)
        {
            var folder = AnalyzeContents(node, directory, nodeFactory);

            if(folder != null)
            {
                folder.Set(Properties.IsStoredAs, node);
            }

            foreach(var alg in HashAlgorithms)
            {
                HashAlgorithm.AddHash(node, alg, alg.ComputeHash(directory, false), nodeFactory);
            }
        }

        public ILinkedNode Analyze(ILinkedNode parent, IDirectoryInfo directory, ILinkedNodeFactory nodeFactory)
        {
            var node = AnalyzeFileNode(parent, directory, nodeFactory);
            if(node != null)
            {
                AnalyzeDirectory(node, directory, nodeFactory);
            }
            return node;
        }

        private ILinkedNode AnalyzeContents(ILinkedNode parent, IDirectoryInfo directory, ILinkedNodeFactory nodeFactory)
        {
            var folder = parent?[""] ?? nodeFactory.NewGuidNode();

            if(folder != null)
            {
                folder.SetClass(Classes.Folder);

                folder.Set(Properties.IsStoredAs, parent);

                folder.Set(Properties.PrefLabel, directory.ToString() + "/");

                LinkDirectories(folder, directory.Path, true, nodeFactory);

                foreach(var alg in HashAlgorithms)
                {
                    HashAlgorithm.AddHash(folder, alg, alg.ComputeHash(directory, true), nodeFactory);
                }

                foreach(var entry in directory.Entries)
                {
                    var node2 = Analyze(parent, entry, nodeFactory);
                    if(node2 != null)
                    {
                        node2.Set(Properties.BelongsToContainer, folder);
                    }
                }
            }

            return folder;
        }

        private void LinkDirectories(ILinkedNode initial, string path, bool directory, ILinkedNodeFactory nodeFactory)
        {
            if(path == null) return;
            var parts = path.Split('/');
            if(parts.Length == 0) return;
            for(int i = 0; i < parts.Length; i++)
            {
                var local = String.Join("/", parts.Skip(i).Select(Uri.EscapeDataString)) + (directory ? "/" : "");
                var file = nodeFactory.Create(Vocabularies.File, local);
                initial.Set(Properties.PathObject, file);
                initial = file;
            }

            if(!directory)
            {
                string ext;
                try{
                    ext = Path.GetExtension(parts[parts.Length - 1]);
                }catch(ArgumentException)
                {
                    ext = null;
                }
                if(!String.IsNullOrEmpty(ext))
                {
                    ext = ext.Substring(1).ToLowerInvariant();
                    initial.Set(Properties.ExtensionObject, Vocabularies.Uris, Uri.EscapeDataString(ext));
                }
            }
        }

        public ILinkedNode Analyze(ILinkedNode parent, FileInfo entity, ILinkedNodeFactory nodeFactory)
        {
            return Analyze(parent, new FileInfoWrapper(entity), nodeFactory);
        }

        public ILinkedNode Analyze(ILinkedNode parent, DirectoryInfo entity, ILinkedNodeFactory nodeFactory)
        {
            return Analyze(parent, new DirectoryInfoWrapper(entity), nodeFactory);
        }

        public ILinkedNode Analyze(ILinkedNode parent, IFileNodeInfo entity, ILinkedNodeFactory nodeFactory)
        {
            switch(entity)
            {
                case IFileInfo file: return Analyze(parent, file, nodeFactory);
                case IDirectoryInfo dir: return Analyze(parent, dir, nodeFactory);
                default: return CreateNode(parent, entity, nodeFactory);
            }
        }
    }
}
