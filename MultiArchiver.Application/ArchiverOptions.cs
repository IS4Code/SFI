using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver
{
    public class ArchiverOptions
    {
        public bool DirectOutput { get; set; }
        public bool CompressedOutput { get; set; }
        public bool HideMetadata { get; set; }
        public bool PrettyPrint { get; set; }
        public IEnumerable<IFileInfo> Queries { get; set; } = Array.Empty<IFileInfo>();
        public string Root { get; set; } = "urn:uuid:";
        public string NewLine { get; set; } = Environment.NewLine;
        public Uri Node { get; set; }
    }
}
