using System;

namespace IS4.MultiArchiver
{
    public class ArchiverOptions
    {
        public bool DirectOutput { get; set; }
        public bool CompressedOutput { get; set; }
        public bool HideMetadata { get; set; }
        public bool PrettyPrint { get; set; }
        public string Root { get; set; } = "urn:uuid:";
        public string NewLine { get; set; } = Environment.NewLine;
        public Uri Node { get; set; }
    }
}
