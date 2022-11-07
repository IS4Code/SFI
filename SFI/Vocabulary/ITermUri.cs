namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Stores information about an RDF term from some vocabulary.
    /// </summary>
    public interface ITermUri
    {
        /// <summary>
        /// The vocabulary where the term resides.
        /// </summary>
        VocabularyUri Vocabulary { get; }

        /// <summary>
        /// The local name of the term inside the vocabulary.
        /// </summary>
        string Term { get; }

        /// <summary>
        /// The full value of the term, concatenated from <see cref="VocabularyUri.Value"/>
        /// and <see cref="Term"/>.
        /// </summary>
        string Value { get; }
    }
}
