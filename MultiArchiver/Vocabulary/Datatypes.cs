namespace IS4.MultiArchiver.Vocabulary
{
    public enum Datatypes
    {
        [Uri(Vocabularies.Xsd)]
        AnyURI,
        [Uri(Vocabularies.Xsd)]
        HexBinary,
        [Uri(Vocabularies.Xsd)]
        Base64Binary,
        [Uri(Vocabularies.Xsd)]
        Integer,
        [Uri(Vocabularies.Xsd)]
        String,

        [Uri(Vocabularies.Owl)]
        Rational,

        [Uri(Vocabularies.Dt)]
        Byte,
    }
}
