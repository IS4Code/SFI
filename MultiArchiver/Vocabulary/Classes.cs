namespace IS4.MultiArchiver.Vocabulary
{
    public enum Classes
    {
        [Uri(Vocabularies.Schema)]
        Photograph,
        [Uri(Vocabularies.Schema)]
        ImageObject,
        [Uri(Vocabularies.Schema)]
        AudioObject,
        [Uri(Vocabularies.Schema)]
        VideoObject,
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
        [Uri(Vocabularies.Nfo)]
        Executable,
        [Uri(Vocabularies.Nfo)]
        Audio,
        [Uri(Vocabularies.Nfo)]
        Image,
        [Uri(Vocabularies.Nfo)]
        Video,

        [Uri(Vocabularies.Xis)]
        Document,
        [Uri(Vocabularies.Xis)]
        Element,

        [Uri(Vocabularies.Sec)]
        Digest,

        [Uri(Vocabularies.At)]
        Root,
    }
}
