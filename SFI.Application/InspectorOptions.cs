using IS4.SFI.RDF;
using IS4.SFI.Services;
using System;
using System.Collections.Generic;

namespace IS4.SFI.Application
{
    /// <summary>
    /// Additional options and configuration of <see cref="Inspector"/>.
    /// </summary>
    public class InspectorOptions
    {
        /// <summary>
        /// Whether to write the output directly or use an intermediate graph.
        /// </summary>
        public bool DirectOutput { get; set; }

        /// <summary>
        /// Whether to compress the output file with gzip.
        /// </summary>
        public bool CompressedOutput { get; set; }

        /// <summary>
        /// Whether to hide metadata properties, such as
        /// <see cref="Vocabulary.Properties.Visited"/>, from the output.
        /// </summary>
        public bool HideMetadata { get; set; }

        /// <summary>
        /// Whether to prettify the text output.
        /// </summary>
        public bool PrettyPrint { get; set; }

        /// <summary>
        /// Input SPARQL queries to be used to search in the description.
        /// </summary>
        public IEnumerable<IFileInfo> Queries { get; set; } = Array.Empty<IFileInfo>();

        /// <summary>
        /// Whether the output file should be used to write SPARQL results.
        /// </summary>
        public bool OutputIsSparqlResults { get; set; }

        /// <summary>
        /// The root of the URI hierarchy; by default <see cref="LinkedNodeHandler.BlankUriScheme"/>.
        /// </summary>
        public string Root { get; set; } = LinkedNodeHandler.BlankUriScheme + ":";

        /// <summary>
        /// The sequence of characters used for separating lines in text output.
        /// </summary>
        public string NewLine { get; set; } = Environment.NewLine;

        /// <summary>
        /// The URI of the described node.
        /// </summary>
        public Uri? Node { get; set; }

        /// <summary>
        /// Contains the desired output format, as a file extension.
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// Whether to shorten blank node IDs in the resulting graph.
        /// </summary>
        public bool SimplifyBlankNodes { get; set; }
    }
}
