[Semantic File Inspector](https://github.com/IS4Code/SFI/) Base Library
==========

This is the core library of the
[Semantic File Inspector](https://github.com/IS4Code/SFI/), defining the common
interfaces used in the other projects, as well as core analyzers and formats
whose implementation does not depend on any external libraries outside of .NET.

It defines types from these namespaces:

### `IS4.SFI`

This namespace contains various utility classes such as `DataTools`,
`TextTools`, and `UriTools`, as well as classes containing extension methods
intended to be used from all other components.

### `IS4.SFI.Analyzers`

This namespace is intended to store all analyzers, in this project or others.
In the core project, the only defined analyzers are for objects whose
types are defined in .NET and are critical to the proper functioning of the
software, such as `FileAnalyzer`, `DataAnalyzer`, etc.

### `IS4.SFI.Formats`

This namespace is similar to the previous one but stores classes used for
defining and parsing formats. It contains common interfaces and related
components, while concrete formats are defined in their respective projects.

### `IS4.SFI.Services`

This namespace contains specialized interfaces to be used for communication
between formats and analyzers or other components in the solution, such as
`IFileNodeInfo`, `IFormatObject`, `ILinkedNode`, `IEncodingDetector`, and
similar, as well as their base implementations.

### `IS4.SFI.Tags`

Tags, in this context, are usually small objects intended to be applied to
existing objects via other means, providing extended description of them
beyond what their original classes support.

In this project, the tags that are defined are `IImageTag` and
`IImageResourceTag`, usually applied to images via the `Image.Tag` property,
providing information about the origin of the image or the allowed operations.

### `IS4.SFI.Tools`

This namespace hosts various specialized utility classes for general use and
for I/O and XML operations. It also exposes the collection of built-in
hash algorithms using the `BuiltInHash` class, or the `EncodedUri`, which
should be used instead of the base `Uri` class in all situations to control its
formatting.

### `IS4.SFI.Vocabulary`

This namespace provides datatypes used for defining RDF vocabularies, such as
`ClassUri`, `PropertyUri`, or `LanguageCode`, as well as storing the common
vocabulary terms in the static classes `Individuals`, `Classes`, `Properties`,
and `Datatypes`, to be used from code easily.