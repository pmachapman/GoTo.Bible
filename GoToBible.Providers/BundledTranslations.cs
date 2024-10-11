// -----------------------------------------------------------------------
// <copyright file="BundledTranslations.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;

/// <summary>
/// The Bundled Translations Bible Provider.
/// </summary>
/// <seealso cref="IProvider" />
public partial class BundledTranslations : IProvider
{
    /// <summary>
    /// The SBL copyright messages.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> Copyright = new Dictionary<
        string,
        string
    >
    {
        { "BCP1979PSALMS", "Public Domain" },
        { "BCPPSALMS", "PUBLIC DOMAIN, except in the United Kingdom, where a Crown Copyright applies to printing the BCP. See <a href=\"http://www.cambridge.org/about-us/who-we-are/queens-printers-patent\" target=\"_blank\">http://www.cambridge.org/about-us/who-we-are/queens-printers-patent</a>" },
        { "BCPPSALMSALT", "PUBLIC DOMAIN, except in the United Kingdom, where a Crown Copyright applies to printing the BCP. See <a href=\"http://www.cambridge.org/about-us/who-we-are/queens-printers-patent\" target=\"_blank\">http://www.cambridge.org/about-us/who-we-are/queens-printers-patent</a>" },
        { "BCPPSALMSVUL", "PUBLIC DOMAIN, except in the United Kingdom, where a Crown Copyright applies to printing the BCP. See <a href=\"http://www.cambridge.org/about-us/who-we-are/queens-printers-patent\" target=\"_blank\">http://www.cambridge.org/about-us/who-we-are/queens-printers-patent</a>" },
        { "BSB", "The Holy Bible, Berean Standard Bible, BSB is produced in cooperation with <a href=\"//biblehub.com\" target=\"_blank\">Bible Hub</a>, <a href=\"//discoverybible.com\" target=\"_blank\">Discovery Bible</a>, <a href=\"//openbible.com\" target=\"_blank\">OpenBible.com</a>, and the Berean Bible Translation Committee. This text of God's Word has been <a href=\"https://creativecommons.org/publicdomain/zero/1.0/\" target=\"_blank\"> dedicated to the public domain</a>." },
        { "LAOANG", "Public Domain" },
        { "LAOGRK", "Public Domain" },
        { "LAOLAT", "Public Domain" },
        { "NTA", "New Translation of the Apocrypha by <a href=\"https://goto.bible/\" target=\"_blank\">Peter Chapman</a> is licensed under <a href=\"https://creativecommons.org/licenses/by/4.0/\" target=\"_blank\">CC BY 4.0</a>" },
        { "NTANOTES", "New Translation of the Apocrypha by <a href=\"https://goto.bible/\" target=\"_blank\">Peter Chapman</a> is licensed under <a href=\"https://creativecommons.org/licenses/by/4.0/\" target=\"_blank\">CC BY 4.0</a>" },
        { "SBLGNT", "Scripture quotations marked <a href=\"http://sblgnt.com/\" target=\"_blank\">SBLGNT</a> are from the <a href=\"http://sblgnt.com/\" target=\"_blank\">SBL Greek New Testament</a>. Copyright &copy; 2010 <a href=\"http://www.sbl-site.org/\" target=\"_blank\">Society of Biblical Literature</a> and <a href=\"http://www.logos.com/\" target=\"_blank\">Logos Bible Software</a>." },
        { "SBLGNTAPP", "Scripture quotations marked <a href=\"http://sblgnt.com/\" target=\"_blank\">SBLGNT</a> are from the <a href=\"http://sblgnt.com/\" target=\"_blank\">SBL Greek New Testament</a>. Copyright &copy; 2010 <a href=\"http://www.sbl-site.org/\" target=\"_blank\">Society of Biblical Literature</a> and <a href=\"http://www.logos.com/\" target=\"_blank\">Logos Bible Software</a>." },
        { "TRWHAPP", "Public Domain" },
    };

    /// <summary>
    /// The canon.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, BookHelper> Canon = new Dictionary<
        string,
        BookHelper
    >
    {
        { "BCP1979PSALMS", new BookHelper("Psalm", 150) },
        { "BCPPSALMS", new BookHelper("Psalm", 150) },
        { "BCPPSALMSALT", new BookHelper("Psalm", 150) },
        { "BCPPSALMSVUL", new BookHelper("Psalm", 150) },
        { "BSB", new ProtestantCanon() },
        { "LAOANG", new BookHelper("Laodiceans", 1) },
        { "LAOGRK", new BookHelper("Laodiceans", 1) },
        { "LAOLAT", new BookHelper("Laodiceans", 1) },
        {
            "NTA",
            new BookHelper(
                new OrderedDictionary
                {
                    { "psalm 151", 1 },
                    { "manasseh", 1 },
                    { "laodiceans", 1 },
                }
            )
        },
        {
            "NTANOTES",
            new BookHelper(
                new OrderedDictionary
                {
                    { "psalm 151", 1 },
                    { "manasseh", 1 },
                    { "laodiceans", 1 },
                }
            )
        },
        { "SBLGNT", new NewTestamentCanon() },
        { "SBLGNTAPP", new NewTestamentCanon() },
        { "TRWHAPP", new NewTestamentCanon() },
    };

    private static readonly IReadOnlyDictionary<string, bool> SupportsItalics = new Dictionary<
        string,
        bool
    >
    {
        { "BCP1979PSALMS", false },
        { "BCPPSALMS", false },
        { "BCPPSALMSALT", false },
        { "BCPPSALMSVUL", false },
        { "BSB", false },
        { "LAOANG", false },
        { "LAOGRK", false },
        { "LAOLAT", false },
        { "NTA", true },
        { "NTANOTES", true },
        { "SBLGNT", false },
        { "SBLGNTAPP", false },
        { "TRWHAPP", false },
    };

    /// <summary>
    /// The renderer for apparatus generation.
    /// </summary>
    private readonly IRenderer? renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BundledTranslations" /> class.
    /// </summary>
    /// ReSharper disable once UnusedMember.Global
    public BundledTranslations() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BundledTranslations" /> class.
    /// </summary>
    /// <param name="renderer">The renderer. This is used for the apparatus generator.</param>
    public BundledTranslations(IRenderer renderer) => this.renderer = renderer;

    /// <inheritdoc/>
    public string Id => nameof(BundledTranslations);

    /// <inheritdoc/>
    public string Name => "Bundled Translations";

    /// <inheritdoc/>
    public void Dispose() => GC.SuppressFinalize(this);

    /// <inheritdoc/>
    public async IAsyncEnumerable<Book> GetBooksAsync(
        string translation,
        bool includeChapters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (Book book in Canon[translation].GetBooks(includeChapters))
        {
            yield return await Task.FromResult(book);
        }
    }

    /// <inheritdoc/>
    public async Task<Chapter> GetChapterAsync(
        string translation,
        string book,
        int chapterNumber,
        CancellationToken cancellationToken = default
    )
    {
        // Set up the chapter
        Chapter chapter = new Chapter
        {
            Book = book,
            ChapterNumber = chapterNumber,
            Copyright = Copyright[translation],
            NextChapterReference = new ChapterReference(),
            PreviousChapterReference = new ChapterReference(),
            SupportsItalics = SupportsItalics[translation],
            Translation = translation,
        };

        // Make sure we are in the new testament canon
        if (Canon[translation].IsValidChapter(book, chapterNumber))
        {
            // Use this when developing to test your translation
            // await using Stream? stream = File.OpenRead(@$"..\..\..\..\GoToBible.Providers\Texts\{translation}.txt");

            // Get the text
            await using Stream? stream = this.GetType()
                .Assembly.GetManifestResourceStream($"GoToBible.Providers.Texts.{translation}.txt");
            if (stream is not null)
            {
                // Read the stream from the assembly
                using StreamReader reader = new StreamReader(stream);
                bool readingChapter = false;
                string lineStart = $"{book} {chapterNumber}:";
                StringBuilder sb = new StringBuilder();
                while (await reader.ReadLineAsync(cancellationToken) is { } line)
                {
                    if (line.StartsWith(lineStart, StringComparison.OrdinalIgnoreCase))
                    {
                        // Read each verse of the chapter
                        readingChapter = true;
                        string lineToAppend = line[lineStart.Length..].Replace("\t", "  ");
                        Match hasVerse = HasVerseRegex().Match(lineToAppend);
                        if (hasVerse.Success)
                        {
                            // See SBLGNT Acts 19:40 for an example of a verse like this
                            lineToAppend = lineToAppend.Replace(
                                hasVerse.Value,
                                $"\r\n{hasVerse.Value}",
                                StringComparison.OrdinalIgnoreCase
                            );
                        }

                        // Remove verse number from Psalm inscriptions
                        if (lineToAppend.StartsWith("0  ", StringComparison.OrdinalIgnoreCase))
                        {
                            lineToAppend = lineToAppend[3..];
                        }

                        sb.AppendLine(lineToAppend);
                    }
                    else if (readingChapter)
                    {
                        // We do not need to read any more
                        break;
                    }
                }

                chapter.Text = sb.ToString();
            }
            else if (
                this.renderer is not null
                && translation == "TRWHAPP"
                && this.renderer.Providers.Any(r => r.Id == nameof(BibliaApi))
            )
            {
                // Use the renderer to generate this apparatus
                ApparatusRenderingParameters parameters = new ApparatusRenderingParameters
                {
                    Format = RenderFormat.Apparatus,
                    InterlinearIgnoresCase = true,
                    InterlinearIgnoresDiacritics = true,
                    InterlinearIgnoresPunctuation = true,
                    PassageReference = book.AsPassageReference(chapterNumber),
                    PrimaryProvider = nameof(BibliaApi),
                    PrimaryTranslation = "tr1894mr",
                    RenderNeighbourForAddition = true,
                    SecondaryProvider = nameof(BibliaApi),
                    SecondaryTranslation = "wh1881mr",
                };
                RenderedPassage renderedPassage = await this.renderer.RenderAsync(
                    parameters,
                    false,
                    cancellationToken
                );
                chapter.Text = renderedPassage.Content;
            }
        }

        if (!string.IsNullOrWhiteSpace(chapter.Text))
        {
            chapter.PreviousChapterReference = Canon[translation]
                .GetPreviousChapter(book, chapterNumber);
            chapter.NextChapterReference = Canon[translation].GetNextChapter(book, chapterNumber);
            return chapter;
        }
        else
        {
            return chapter;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<Translation> GetTranslationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        yield return await Task.FromResult(
            new Translation
            {
                CanBeExported = true,
                Code = "BCP1979PSALMS",
                Copyright = Copyright["BCP1979PSALMS"],
                Language = "English",
                Name = "Book of Common Prayer (1979) Psalter",
                Provider = this.Id,
                Year = 1979,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                CanBeExported = true,
                Code = "BCPPSALMS",
                Copyright = Copyright["BCPPSALMS"],
                Language = "English",
                Name = "Coverdale Psalter",
                Provider = this.Id,
                Year = 1539,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                CanBeExported = true,
                Code = "BCPPSALMSALT",
                Copyright = Copyright["BCPPSALMSALT"],
                Language = "English",
                Name = "Coverdale Psalter (KJV Versification)",
                Provider = this.Id,
                Year = 1539,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                CanBeExported = true,
                Code = "BCPPSALMSVUL",
                Copyright = Copyright["BCPPSALMSVUL"],
                Language = "English",
                Name = "Coverdale Psalter (Douay Versification)",
                Provider = this.Id,
                Year = 1539,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                CanBeExported = true,
                Code = "BSB",
                Copyright = Copyright["BSB"],
                Language = "English",
                Name = "Berean Standard Bible",
                Provider = this.Id,
                Year = 2020,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                Author = "Joseph Barber Lightfoot; Updated by Peter Chapman",
                CanBeExported = true,
                Code = "LAOGRK",
                Copyright = Copyright["LAOGRK"],
                Language = "Greek",
                Name = "Epistle to the Laodiceans (Greek)",
                Provider = this.Id,
                Year = 2021,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                Author = "Joseph Barber Lightfoot; Updated by Peter Chapman",
                CanBeExported = true,
                Code = "LAOLAT",
                Copyright = Copyright["LAOLAT"],
                Language = "Latin",
                Name = "Epistle to the Laodiceans (Latin)",
                Provider = this.Id,
                Year = 2021,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                Author = "John Wycliffe",
                CanBeExported = true,
                Code = "LAOANG",
                Copyright = Copyright["LAOANG"],
                Language = "Old English",
                Name = "Epistle to the Laodiceans (Old English)",
                Provider = this.Id,
                Year = 2021,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                CanBeExported = true,
                Code = "NTA",
                Copyright = Copyright["NTA"],
                Language = "English",
                Name = "New Translation of the Apocrypha",
                Provider = this.Id,
                Year = 2021,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                CanBeExported = true,
                Commentary = true,
                Code = "NTANOTES",
                Copyright = Copyright["NTANOTES"],
                Language = "English",
                Name = "New Translation of the Apocrypha Notes",
                Provider = this.Id,
                Year = 2021,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                Author = "Michael W. Holmes",
                Code = "SBLGNT",
                Copyright = Copyright["SBLGNT"],
                Language = "Greek",
                Name = "SBL Greek New Testament",
                Provider = this.Id,
                Year = 2010,
            }
        );
        yield return await Task.FromResult(
            new Translation
            {
                Author = "Michael W. Holmes",
                Code = "SBLGNTAPP",
                Commentary = true,
                Copyright = Copyright["SBLGNTAPP"],
                Language = "Greek",
                Name = "SBL Greek New Testament Apparatus",
                Provider = this.Id,
                Year = 2010,
            }
        );

        // We can only generate an apparatus if we have a renderer
        if (this.renderer is not null)
        {
            yield return await Task.FromResult(
                new Translation
                {
                    Author = "Peter Chapman",
                    Code = "TRWHAPP",
                    Commentary = true,
                    Copyright = Copyright["TRWHAPP"],
                    Language = "Greek",
                    Name = "Textus Receptus/Westcott and Hort Apparatus",
                    Provider = this.Id,
                    Year = 2022,
                }
            );
        }
    }

    /// <summary>
    /// The regular express to determine if a line of text has a verse number bracketed in it.
    /// </summary>
    /// <returns>The mid-line verse number regular expression.</returns>
    /// <remarks>See Acts 19:40 in the SBLGNT for an example of a verse like this.</remarks>
    [GeneratedRegex(@"\[\d+\]", RegexOptions.Compiled)]
    private static partial Regex HasVerseRegex();
}
