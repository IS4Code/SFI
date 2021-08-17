namespace IS4.MultiArchiver.Vocabulary
{
    public interface ITermUri
    {
        VocabularyUri Vocabulary { get; }
        string Term { get; }

        string Value { get; }
    }
}
