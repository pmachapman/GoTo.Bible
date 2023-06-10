// -----------------------------------------------------------------------
// <copyright file="Zefania.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CsvHelper;
using GoToBible.Model;
using Microsoft.Extensions.Options;

/// <summary>
/// The Zefania XML Provider.
/// </summary>
/// <seealso cref="ApiProvider" />
public class Zefania : ApiProvider
{
    /// <summary>
    /// The scripture cache.
    /// </summary>
    private readonly ConcurrentDictionary<string, Chapter> cache = new ConcurrentDictionary<string, Chapter>();

    /// <summary>
    /// The options.
    /// </summary>
    private readonly LocalResourceOptions options;

    /// <summary>
    /// A value indicating whether or not the path to Zefania is valid.
    /// </summary>
    private readonly bool isValidPath;

    /// <summary>
    /// A value indicating whether or not this instance has been disposed.
    /// </summary>
    private bool disposedValue;

    /// <summary>
    /// The translations cache.
    /// </summary>
    private readonly List<LocalTranslation> translations = new List<LocalTranslation>();

    /// <summary>
    /// Initialises a new instance of the <see cref="Zefania" /> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentException">Invalid Zefania Directory - options.</exception>
    public Zefania(IOptions<LocalResourceOptions> options)
    {
        // Set the options
        this.options = options.Value;

        // Check the path
        this.isValidPath = Directory.Exists(this.options.ResourceDirectory) && File.Exists(Path.Combine(this.options.ResourceDirectory, "index.csv"));
    }

    /// <summary>
    /// Finalises an instance of the <see cref="Zefania"/> class.
    /// </summary>
    /// <remarks>
    /// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    /// </remarks>
    ~Zefania() => this.Dispose(false);

    /// <inheritdoc/>
    public override string Id => nameof(Zefania);

    /// <inheritdoc/>
    public override string Name => "Zefania";

    /// <inheritdoc/>
    public override void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
    {
        await this.EnsureTranslationsAreCachedAsync();

        // Ensure we have translations
        if (this.translations.Any())
        {
            // Get the translation
            LocalTranslation? zefaniaTranslation = this.translations.FirstOrDefault(t => t.Code == translation);
            if (zefaniaTranslation is not null)
            {
                // Make sure the file is extracted, if it is a zip file
                string fileName = this.ExtractFile(zefaniaTranslation.Filename);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(Path.Combine(this.options.ResourceDirectory, fileName));
                if (xmlDocument.DocumentElement?.HasChildNodes ?? false)
                {
                    foreach (XmlNode bookNode in xmlDocument.DocumentElement.ChildNodes)
                    {
                        string? bookName = bookNode.Attributes?["bname"]?.InnerText;
                        if (!string.IsNullOrWhiteSpace(bookName))
                        {
                            // Create the list of chapters
                            List<ChapterReference> chapters = new List<ChapterReference>();
                            if (includeChapters)
                            {
                                foreach (XmlNode chapterNode in bookNode.ChildNodes)
                                {
                                    if (int.TryParse(chapterNode.Attributes?["cnumber"]?.InnerText, out int chapterNumber))
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
    public override async Task<Chapter> GetChapterAsync(string translation, string book, int chapterNumber)
    {
        await this.EnsureTranslationsAreCachedAsync();

        // Ensure we have translations
        if (this.translations.Any())
        {
            // Generate the cache key
            string cacheKey = $"{translation}-{book}-{chapterNumber}";
            if (this.cache.TryGetValue(cacheKey, out Chapter? cacheChapter))
            {
                return cacheChapter;
            }

            // Get the translation
            LocalTranslation? zefaniaTranslation = this.translations.FirstOrDefault(t => t.Code == translation);
            if (zefaniaTranslation is not null)
            {
                // Make sure the file is extracted, if it is a zip file
                string fileName = this.ExtractFile(zefaniaTranslation.Filename);

                // The filename will be an XML file at this point
                if (!string.IsNullOrWhiteSpace(book) && chapterNumber > 0)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(Path.Combine(this.options.ResourceDirectory, fileName));
                    if (xmlDocument.DocumentElement?.HasChildNodes ?? false)
                    {
                        foreach (XmlNode bookNode in xmlDocument.DocumentElement.ChildNodes)
                        {
                            if (string.Equals(bookNode.Attributes?["bname"]?.InnerText, book, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (bookNode.HasChildNodes)
                                {
                                    foreach (XmlNode chapterNode in bookNode.ChildNodes)
                                    {
                                        if (chapterNode.Attributes?["cnumber"]?.InnerText == chapterNumber.ToString(CultureInfo.InvariantCulture))
                                        {
                                            if (chapterNode.HasChildNodes)
                                            {
                                                // Get all verses in this chapter
                                                StringBuilder sb = new StringBuilder();
                                                foreach (XmlNode verseNode in chapterNode.ChildNodes)
                                                {
                                                    sb.Append(verseNode.Attributes?["vnumber"]?.InnerText);
                                                    sb.Append("  ");
                                                    sb.Append(verseNode.InnerText.Trim());
                                                    sb.AppendLine(" ");
                                                }

                                                Chapter chapter = new Chapter
                                                {
                                                    Book = book,
                                                    ChapterNumber = chapterNumber,
                                                    Text = sb.ToString(),
                                                    Translation = translation,
                                                };
                                                await this.GetPreviousAndNextChaptersAsync(chapter);
                                                this.cache.TryAdd(cacheKey, chapter);
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

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Translation> GetTranslationsAsync()
    {
        if (this.isValidPath)
        {
            using StreamReader reader = new StreamReader(Path.Combine(this.options.ResourceDirectory, "index.csv"));
            using CsvReader csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            bool initialiseCache = !this.translations.Any();
            await foreach (LocalTranslation translation in csvReader.GetRecordsAsync<LocalTranslation>())
            {
                if (translation.Provider == "Zefania")
                {
                    translation.Provider = this.Id;
                    if (initialiseCache)
                    {
                        this.translations.Add(translation);
                    }

                    yield return translation;
                }
            }
        }
    }

    /// <inheritdoc cref="IDisposable" />
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)

                // Make sure we have the translations cache set up
                if (this.translations.Any())
                {
                    foreach (LocalTranslation translation in this.translations)
                    {
                        // Make sure the translation file is is a zip file
                        string fileExtension = Path.GetExtension(translation.Filename).ToUpperInvariant();
                        if (fileExtension == ".ZIP")
                        {
                            // Check if a non-strongs version is in a strongs file
                            string xmlFilename = Path.GetFileNameWithoutExtension(translation.Filename) + ".xml";
                            if (translation.Filename.ToUpperInvariant().Contains("_STRONG") && !File.Exists(Path.Combine(this.options.ResourceDirectory, xmlFilename)))
                            {
                                xmlFilename = xmlFilename.Replace("_strong", string.Empty, StringComparison.OrdinalIgnoreCase);
                            }

                            // Clean up the file
                            string xmlFilePath = Path.Combine(this.options.ResourceDirectory, xmlFilename);
                            if (File.Exists(xmlFilePath))
                            {
                                File.Delete(xmlFilePath);
                            }
                        }
                    }
                }
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            this.disposedValue = true;
        }
    }

    /// <summary>
    /// Ensures the translations are cached asynchronously.
    /// </summary>
    /// <returns>
    /// The task.
    /// </returns>
    private async Task EnsureTranslationsAreCachedAsync()
    {
        // Make sure we have the translations cache set up
        if (!this.translations.Any())
        {
            // This is just so we can have some async code to cancel the error
            await foreach (Translation? _ in this.GetTranslationsAsync())
            {
            }
        }
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
            if (!File.Exists(Path.Combine(this.options.ResourceDirectory, xmlFilename)))
            {
                try
                {
                    ZipFile.ExtractToDirectory(Path.Combine(this.options.ResourceDirectory, fileName), this.options.ResourceDirectory);
                }
                catch (IOException)
                {
                    // Error unzipping, the file should already be there
                }
            }

            // Check if a non-strongs version is in a strongs file
            if (fileName.ToUpperInvariant().Contains("_STRONG") && !File.Exists(Path.Combine(this.options.ResourceDirectory, xmlFilename)))
            {
                xmlFilename = xmlFilename.Replace("_strong", string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            // Update the filename in our copy of the object
            fileName = xmlFilename;
        }

        return fileName;
    }
}
