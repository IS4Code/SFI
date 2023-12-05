<img src="icon.png" height="32"> Semantic File Inspector
==========

The *Semantic File Inspector* is a .NET project that provides a software suite and a tool to describe any file or piece of data, including its formats and contents, using [RDF (Resource Description Framework)](https://www.w3.org/TR/rdf-primer/).

The tool is accessible through several options:

- Online at [sfi.is4.site](https://sfi.is4.site/) (requires WebAssembly support)
- Installable via <code>[dotnet tool install](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install) IS4.SFI.ConsoleApp --global</code> (run as `sfi`).
- Downloadable in the [releases](//github.com/IS4Code/SFI/releases) for your platform.

Supported formats:

* Applications: com, dll, exe, swf.
* Archives: 7z, cab, gz, iso, lha, mpq, pbo, rar, sz, tar, udf, wad, zip.
* Audio: aa, aax, aac, aiff, ape, dsf, flac, m4a, m4b, m4p, mp3, mpc, mpp, ogg, oga, wav, wma, wv, webm.
* Certificates: der, p7c, pfx.
* Data: dfm, ole, rdf, warc, xml.
* Documents: doc, docx, html, pdf, xls, xlsx.
* Images: bmp, gif, ico, jpg, pbm, pgm, ppm, pnm, pcx, png, psd, tiff, tga, dng, svg, webp.
* Video: asf, avci, avi, mkv, mov, mp4, mpeg, ogv, wmv.

## Documentation

For detailed information regarding the command-line API, configuration options, and plugin integration, please see the [wiki](//github.com/IS4Code/SFI/wiki), offering comprehensive documentation covering various aspects of the software.

## Features

The software offers the following features:

- **Format Extraction**: Supports over 50 different formats including common media formats, archives, executables, and documents.
- **Rich Metadata**: Collects rich metadata, including common file properties and format-specific properties such as image dimensions.
- **Hashing**: Computes hashes using various algorithms to describe and identify the data.
- **RDF Encoding**: Encodes all the extracted information in RDF using common vocabularies found on the semantic web.
- **Serialization**: Allows saving the resulting RDF in one of the many RDF serialization formats.
- **SPARQL Support**: Provides the ability to use SPARQL to extract information or data using the processed RDF.

## Additional Abilities

The suite goes beyond basic functionality with the following advanced abilities:

- **Abstraction Levels**: Represents and describes files at different levels of abstraction, including the file node, its binary/text content, the encoded media object (such as an XML document), or the resource it represents.
- **Multiple Formats**: Derives multiple formats from a single file, enabling identification of multiple formats (like ISO and UDF from disk images).
- **Recursive Exploration**: Looks recursively into archives or other resources storing resources, allowing traversal of complex file structures.
- **Configurability**: All components of the tool can be configured or disabled as per specific requirements.
- **Plugin Support**: Offers the possibility to develop plugins for additional functionality by extending the tool's capabilities.

## Uses

This application provides a valuable solution to individuals and organizations seeking to process files in a semantic and extensible manner. By leveraging RDF and its associated capabilities, the tool enables users to gain insights into large sets of files or archives, perform searches using SPARQL based on specific domain criteria, and identify common or distinct entities across different datasets.

The results can be utilized in various ways:

- **Insight into File Sets**: The tool allows users to gain a deep understanding of large collections of files or archives by extracting and organizing their relevant metadata in RDF format. This can be especially useful for researchers or analysts working with extensive file repositories.

- **Enhanced Search Options**: By utilizing the power of SPARQL, users can perform advanced searches on the processed RDF data. This enables them to define complex queries based on specific domain criteria, facilitating more comprehensive and precise search results.

- **Compact Metadata Representation**: The software provides a way to work with metadata in a compact and detailed form that is detached from the original source. This can be particularly beneficial for analysts who require a concise and structured representation of file metadata for further analysis and processing.

- **Improved File Processing Systems**: Organizations can leverage the use of RDF to enhance their file processing systems by gaining more control over the type of data accepted and processed. This can contribute to improved efficiency, security, and compliance with specific data processing requirements.

- **Enhanced User Experience for File Hosting Sites**: File hosting sites can utilize the software suite or its parts to offer users a wide range of search options based on the rich metadata associated with their files. This provides users with a more comprehensive and intuitive search experience.

## Structure

The software is structured as a collection of projects, each serving specific purposes and dependencies. These projects can be distributed as packages, allowing them to be utilized as libraries in other solutions based on the unique requirements of those solutions.

### [Base Library](SFI)

This is the core library of the software. It defines the common interfaces used in other projects and includes core analyzers and formats that do not rely on external libraries outside of .NET.

### [RDF Library](SFI.RDF)

The RDF Library project provides a concrete implementation of the core classes and interfaces of the core, specifically for RDF output.

### [Formats](SFI.Formats) and [Hashes](SFI.Hashes)

Each supported format, hash algorithm, or a collection thereof is developed as a separate project, able to be loaded or unloaded at will or used in combination with other pipelines.

### [Application](SFI.Application)

The Application project is the primary project to use when embedding the software as a component within another application.

### [Console Application](SFI.ConsoleApp)

This project offers an executable console application for .NET, allowing users to utilize the *Semantic File Inspector* as a standalone command-line tool.

### [Web Application](SFI.WebApp)

The Web Application project presents an ASP.NET Core Blazor WebAssembly application featuring a single page for running the *Semantic File Inspector* directly within the browser.
