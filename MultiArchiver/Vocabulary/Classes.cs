namespace IS4.MultiArchiver.Vocabulary
{
    public enum Classes
    {
        [Uri(Vocabularies.Schema)]
        Photograph,
        [Uri(Vocabularies.Schema)]
        ImageObject,
        [Uri(Vocabularies.Schema)]
        MediaObject,

        [Uri(Vocabularies.Cnt)]
        ContentAsText,
        [Uri(Vocabularies.Cnt)]
        ContentAsBase64,
        [Uri(Vocabularies.Cnt)]
        ContentAsXML,
        [Uri(Vocabularies.Cnt)]
        DoctypeDecl,

        [Uri(Vocabularies.Nfo)]
        ArchiveItem,
        [Uri(Vocabularies.Nfo)]
        Archive,
        [Uri(Vocabularies.Nfo)]
        FileDataObject,
        [Uri(Vocabularies.Nfo)]
        FilesystemImage,
        [Uri(Vocabularies.Nfo)]
        Folder,
    }
}
