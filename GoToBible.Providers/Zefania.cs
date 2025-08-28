// -----------------------------------------------------------------------
// <copyright file="Zefania.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GoToBible.Model;
using Microsoft.Extensions.Options;

/// <summary>
/// The Zefania XML Provider.
/// </summary>
/// <seealso cref="LocalResourceProvider" />
public class Zefania(IOptions<LocalResourceOptions> options) : LocalResourceProvider(options)
{
    /// <summary>
    /// A value indicating whether or not this instance has been disposed.
    /// </summary>
    private bool disposedValue;

    /// <summary>
    /// Finalizes an instance of the <see cref="Zefania"/> class.
    /// </summary>
    /// <remarks>
    /// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    /// </remarks>
    ~Zefania() => this.Dispose(false);

    /// <inheritdoc/>
    public override string Id => nameof(Zefania);

    /// <inheritdoc/>
    public override string Name => nameof(Zefania);

    /// <inheritdoc/>
    public override void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Book> GetBooksAsync(
        string translation,
        bool includeChapters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await this.EnsureTranslationsAreCachedAsync(cancellationToken);

        // Ensure we have translations
        if (this.Translations.Count > 0)
        {
            // Get the translation
            LocalTranslation? zefaniaTranslation = this.Translations.FirstOrDefault(t =>
                t.Code == translation
            );
            if (zefaniaTranslation is not null)
            {
                // Make sure the file is extracted, if it is a zip file
                string fileName = this.ExtractFile(zefaniaTranslation.Filename);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(Path.Join(this.Options.Directory, fileName));
                if (xmlDocument.DocumentElement?.HasChildNodes ?? false)
                {
                    foreach (XmlNode bookNode in xmlDocument.DocumentElement.ChildNodes)
                    {
                        string? bookName = bookNode.Attributes?["bname"]?.InnerText;
                        if (!string.IsNullOrWhiteSpace(bookName))
                        {
                            // Create the list of chapters
                            List<ChapterReference> chapters = [];
                            if (includeChapters)
                            {
                                foreach (XmlNode chapterNode in bookNode.ChildNodes)
                                {
                                    if (
                                        int.TryParse(
                                            chapterNode.Attributes?["cnumber"]?.InnerText,
                                            out int chapterNumber
                                        )
                                    )
                                    {
                                        chapters.Add(new ChapterReference(bookName, chapterNumber));
                                    }
                                }
                            }

                            // Create and return the book
                            yield return new Book
                            {
                                Chapters = chapters.AsReadOnly(),
                                Name = bookName,
                            };
                        }
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public override async Task<Chapter> GetChapterAsync(
        string translation,
        string book,
        int chapterNumber,
        CancellationToken cancellationToken = default
    )
    {
        await this.EnsureTranslationsAreCachedAsync(cancellationToken);

        // Ensure we have translations
        if (this.Translations.Count > 0)
        {
            // Generate the cache key
            string cacheKey = $"{translation}-{book}-{chapterNumber}";
            if (this.Cache.TryGetValue(cacheKey, out Chapter? cacheChapter))
            {
                return cacheChapter;
            }

            // Get the translation
            LocalTranslation? zefaniaTranslation = this.Translations.FirstOrDefault(t =>
                t.Code == translation
            );
            if (zefaniaTranslation is not null)
            {
                // Make sure the file is extracted, if it is a zip file
                string fileName = this.ExtractFile(zefaniaTranslation.Filename);

                // The filename will be an XML file at this point
                if (!string.IsNullOrWhiteSpace(book) && chapterNumber > 0)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(Path.Join(this.Options.Directory, fileName));
                    if (xmlDocument.DocumentElement?.HasChildNodes ?? false)
                    {
                        foreach (XmlNode bookNode in xmlDocument.DocumentElement.ChildNodes)
                        {
                            if (
                                string.Equals(
                                    bookNode.Attributes?["bname"]?.InnerText,
                                    book,
                                    StringComparison.InvariantCultureIgnoreCase
                                )
                            )
                            {
                                if (bookNode.HasChildNodes)
                                {
                                    foreach (XmlNode chapterNode in bookNode.ChildNodes)
                                    {
                                        if (
                                            chapterNode.Attributes?["cnumber"]?.InnerText
                                            == chapterNumber.ToString(CultureInfo.InvariantCulture)
                                        )
                                        {
                                            if (chapterNode.HasChildNodes)
                                            {
                                                // Get all verses in this chapter
                                                StringBuilder sb = new StringBuilder();
                                                foreach (
                                                    XmlNode verseNode in chapterNode.ChildNodes
                                                )
                                                {
                                                    sb.Append(
                                                        verseNode.Attributes?["vnumber"]?.InnerText
                                                    );
                                                    sb.Append("  ");
                                                    sb.Append(
                                                        verseNode
                                                            .InnerText.Replace("-- ", "–")
                                                            .Trim()
                                                    );
                                                    sb.AppendLine(" ");
                                                }

                                                Chapter chapter = new Chapter
                                                {
                                                    Book = book,
                                                    ChapterNumber = chapterNumber,
                                                    Text = sb.ToString(),
                                                    Translation = translation,
                                                };
                                                await this.GetPreviousAndNextChaptersAsync(
                                                    chapter,
                                                    cancellationToken
                                                );
                                                this.Cache.TryAdd(cacheKey, chapter);
                                                return chapter;
                                            }

                                            break;
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        // Default to an empty chapter
        return new Chapter
        {
            Book = book,
            ChapterNumber = chapterNumber,
            Translation = translation,
        };
    }

    /// <inheritdoc cref="IDisposable" />
    protected void Dispose(bool disposing)
    {
        if (!this.disposedValue && disposing && this.Translations.Count > 0)
        {
            // dispose managed state (managed objects)

            // Make sure we have the translations cache set up
            foreach (LocalTranslation translation in this.Translations)
            {
                // Make sure the translation file is is a zip file
                string fileExtension = Path.GetExtension(translation.Filename)
                    .ToUpperInvariant();
                if (fileExtension == ".ZIP")
                {
                    // Check if a non-strongs version is in a strongs file
                    string xmlFilename =
                        Path.GetFileNameWithoutExtension(translation.Filename) + ".xml";
                    if (
                        translation.Filename.Contains(
                            "_STRONG",
                            StringComparison.OrdinalIgnoreCase
                        ) && !File.Exists(Path.Join(this.Options.Directory, xmlFilename))
                    )
                    {
                        xmlFilename = xmlFilename.Replace(
                            "_strong",
                            string.Empty,
                            StringComparison.OrdinalIgnoreCase
                        );
                    }

                    // Clean up the file
                    string xmlFilePath = Path.Join(this.Options.Directory, xmlFilename);
                    try
                    {
                        if (File.Exists(xmlFilePath))
                        {
                            File.Delete(xmlFilePath);
                        }
                    }
                    catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
                    {
                        // Log the exception or handle it appropriately
                        Debug.WriteLine($"Error deleting file: {xmlFilePath}");
                    }
                }
            }
        }

        // free unmanaged resources (unmanaged objects) and override finalizer
        // set large fields to null
        this.disposedValue = true;
    }

    /// <summary>
    /// Extracts the file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>
    /// The file name.
    /// </returns>
    private string ExtractFile(string fileName)
    {
        string fileExtension = Path.GetExtension(fileName).ToUpperInvariant();
        if (fileExtension == ".ZIP")
        {
            string xmlFilename = Path.GetFileNameWithoutExtension(fileName) + ".xml";
            if (!File.Exists(Path.Join(this.Options.Directory, xmlFilename)))
            {
                try
                {
                    ZipFile.ExtractToDirectory(
                        Path.Join(this.Options.Directory, fileName),
                        this.Options.Directory
                    );
                }
                catch (IOException)
                {
                    // Error unzipping, the file should already be there
                }
            }

            // Check if a non-strongs version is in a strongs file
            if (
                fileName.Contains("_STRONG", StringComparison.OrdinalIgnoreCase)
                && !File.Exists(Path.Join(this.Options.Directory, xmlFilename))
            )
            {
                xmlFilename = xmlFilename.Replace(
                    "_strong",
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase
                );
            }

            // Update the filename in our copy of the object
            fileName = xmlFilename;
        }

        return fileName;
    }
}
