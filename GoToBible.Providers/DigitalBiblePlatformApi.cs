// -----------------------------------------------------------------------
// <copyright file="DigitalBiblePlatformApi.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

/// <summary>
/// The Digital Bible Platform API Provider.
/// </summary>
/// <seealso cref="WebApiProvider" />
public class DigitalBiblePlatformApi : WebApiProvider
{
    /// <summary>
    /// The New Testament canon.
    /// </summary>
    private static readonly BookHelper NewTestamentCanon = new NewTestamentCanon();

    /// <summary>
    /// The Old Testament canon.
    /// </summary>
    private static readonly BookHelper OldTestamentCanon = new OldTestamentCanon();

    /// <summary>
    /// The options.
    /// </summary>
    private readonly DigitalBiblePlatformApiOptions options;

    /// <summary>
    /// The translations cache.
    /// </summary>
    private readonly List<DigitalBiblePlatformTranslation> translations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DigitalBiblePlatformApi" /> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cache">The cache.</param>
    public DigitalBiblePlatformApi(
        IOptions<DigitalBiblePlatformApiOptions> options,
        IDistributedCache cache
    )
        : base(cache)
    {
        this.options = options.Value;
        this.HttpClient.BaseAddress = new Uri("https://4.dbt.io/api/", UriKind.Absolute);
    }

    /// <inheritdoc/>
    public override string Id => nameof(DigitalBiblePlatformApi);

    /// <inheritdoc/>
    public override string Name => "Digital Bible Platform API";

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Book> GetBooksAsync(
        string translation,
        bool includeChapters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        string url = $"bibles/{translation}/book?key={this.options.ApiKey}&v=4";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            using HttpResponseMessage response = await this.HttpClient.GetAsync(
                url,
                cancellationToken
            );
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync(cancellationToken);
                await this.Cache.SetStringAsync(
                    cacheKey,
                    json,
                    CacheEntryOptions,
                    cancellationToken
                );
            }
            else
            {
                Debug.Print(
                    $"{response.StatusCode} error in DigitalBiblePlatformApi.GetBooksAsync()"
                );
                yield break;
            }
        }

        var bookData = DeserializeAnonymousType(
            json,
            new { data = EmptyListOf(new { book_id = string.Empty, chapters = new List<int>(), }), }
        );

        if (bookData is not null && bookData.data.Count > 0)
        {
            foreach (var book in bookData.data)
            {
                if (ReverseBookCodeMap.TryGetValue(book.book_id, out string? bookName))
                {
                    bookName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(bookName);
                    List<ChapterReference> chapterReferences = [];
                    if (includeChapters)
                    {
                        chapterReferences.AddRange(
                            book.chapters.Select(chapter => new ChapterReference(bookName, chapter))
                        );
                    }

                    yield return new Book
                    {
                        Chapters = chapterReferences.AsReadOnly(),
                        Name = bookName
                    };
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
        // Set up the chapter
        Chapter chapter = new Chapter
        {
            Book = book,
            ChapterNumber = chapterNumber,
            NextChapterReference = new ChapterReference(),
            PreviousChapterReference = new ChapterReference(),
            Translation = translation,
        };

        // Allow Psalm 151
        string bookName = book.ToLowerInvariant();
        if (bookName == "psalm" && chapterNumber == 151)
        {
            bookName = "psalm 151";
            chapterNumber = 1;
        }

        // Check that the book is supported
        if (!BookCodeMap.TryGetValue(bookName, out string? bookCode))
        {
            return chapter;
        }

        // Select the DAM id
        string damId;
        DigitalBiblePlatformTranslation digitalBiblePlatformTranslation = this.translations.Single(
            t => t.Code == translation
        );
        if (digitalBiblePlatformTranslation.DamIds.Count > 1)
        {
            if (NewTestamentCanon.HasBook(bookName))
            {
                damId =
                    digitalBiblePlatformTranslation.DamIds.FirstOrDefault(d =>
                        d.Contains("N_", StringComparison.OrdinalIgnoreCase)
                    )
                    ?? digitalBiblePlatformTranslation.DamIds.First(d =>
                        !d.Contains("O_", StringComparison.OrdinalIgnoreCase)
                    );
            }
            else if (OldTestamentCanon.HasBook(bookName))
            {
                damId =
                    digitalBiblePlatformTranslation.DamIds.FirstOrDefault(d =>
                        d.Contains("O_", StringComparison.OrdinalIgnoreCase)
                    )
                    ?? digitalBiblePlatformTranslation.DamIds.First(d =>
                        !d.Contains("N_", StringComparison.OrdinalIgnoreCase)
                    );
            }
            else
            {
                // We do not know how to resolve this
                damId = digitalBiblePlatformTranslation.DamIds.OrderBy(d => d).First();
            }
        }
        else
        {
            damId = digitalBiblePlatformTranslation.DamIds.FirstOrDefault() ?? translation;
        }

        // Load the book
        string url =
            $"bibles/filesets/{damId}/{bookCode}/{chapterNumber}?key={this.options.ApiKey}&v=4";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            using HttpResponseMessage response = await this.HttpClient.GetAsync(
                url,
                cancellationToken
            );
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync(cancellationToken);
                await this.Cache.SetStringAsync(
                    cacheKey,
                    json,
                    CacheEntryOptions,
                    cancellationToken
                );
            }
            else
            {
                Debug.Print(
                    $"{response.StatusCode} error in DigitalBiblePlatformApi.GetChapterAsync()"
                );
                return chapter;
            }
        }

        if (!string.IsNullOrWhiteSpace(json))
        {
            var chapterJson = DeserializeAnonymousType(
                json,
                new
                {
                    data = EmptyListOf(
                        new
                        {
                            verse_start = 0,
                            verse_end = 0,
                            verse_text = string.Empty,
                        }
                    ),
                }
            );

            // Get the text
            StringBuilder sb = new StringBuilder();
            if (chapterJson is not null)
            {
                foreach (var verse in chapterJson.data)
                {
                    sb.Append(verse.verse_start);

                    // Allow multi-verse lines, i.e. "1-2  In the beginning"
                    if (verse.verse_start != verse.verse_end)
                    {
                        sb.Append('-');
                        sb.Append(verse.verse_end);
                    }

                    sb.Append("  ");
                    sb.AppendLine(verse.verse_text.Trim().Replace("\n", " ").RemoveDuplicateSpaces());
                }
            }

            chapter.Text = sb.ToString();

            // Get the copyright information
            chapter.Copyright = await this.GetCopyrightAsync(translation, cancellationToken);

            // Get the next/previous chapters
            await this.GetPreviousAndNextChaptersAsync(chapter, cancellationToken);
        }

        // Return the chapter
        return chapter;
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Translation> GetTranslationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        bool initialiseTranslations = this.translations.Count == 0;
        int page = 1;
        int lastPage = 0;
        while (page != lastPage)
        {
            string url =
                $"bibles?key={this.options.ApiKey}&media=text_plain&page={page}&limit=50&v=4";
            string cacheKey = this.GetCacheKey(url);
            string? json = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                using HttpResponseMessage response = await this.HttpClient.GetAsync(
                    url,
                    cancellationToken
                );
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync(cancellationToken);
                    await this.Cache.SetStringAsync(
                        cacheKey,
                        json,
                        CacheEntryOptions,
                        cancellationToken
                    );
                }
                else
                {
                    Debug.Print(
                        $"{response.StatusCode} error in DigitalBiblePlatformApi.GetTranslationsAsync()"
                    );
                    yield break;
                }
            }

            // Clean up the JSON
            json = json.Replace("\"dbp-prod\"", "\"dbp_prod\"", StringComparison.Ordinal)
                .Replace(",\"filesets\":[]", string.Empty);

            var dbpTranslations = DeserializeAnonymousType(
                json,
                new
                {
                    data = EmptyListOf(
                        new
                        {
                            abbr = string.Empty,
                            name = string.Empty,
                            vname = string.Empty,
                            date = string.Empty,
                            language = string.Empty,
                            filesets = Nullable(
                                new { dbp_prod = EmptyListOf(new { id = string.Empty, }), }
                            ),
                        }
                    ),
                    meta = new { pagination = new { current_page = 0, last_page = 0, }, },
                }
            );
            if (dbpTranslations is not null)
            {
                // Set page variables
                lastPage = dbpTranslations.meta.pagination.last_page;
                page = dbpTranslations.meta.pagination.current_page;

                List<DigitalBiblePlatformTranslation> digitalBiblePlatformTranslations = [];
                foreach (var translation in dbpTranslations.data)
                {
                    // Get the name
                    string name;
                    if (!string.IsNullOrWhiteSpace(translation.name))
                    {
                        name = translation.name;
                    }
                    else if (!string.IsNullOrWhiteSpace(translation.vname))
                    {
                        name = translation.vname;
                    }
                    else if (!string.IsNullOrWhiteSpace(translation.date))
                    {
                        name = translation.date;
                    }
                    else
                    {
                        name = translation.abbr;
                    }

                    List<string> damIds =
                        translation.filesets?.dbp_prod.Select(t => t.id).ToList() ?? [];
                    DigitalBiblePlatformTranslation digitalBiblePlatformTranslation =
                        new DigitalBiblePlatformTranslation
                        {
                            Code = translation.abbr,
                            DamIds = damIds,
                            Language = translation.language.NormaliseLanguage(),
                            Name = name,
                            Provider = this.Id,
                        };
                    digitalBiblePlatformTranslations.Add(digitalBiblePlatformTranslation);
                    yield return digitalBiblePlatformTranslation;
                }

                // Go to the next page
                page++;

                // Initialise the cache if we should
                if (initialiseTranslations)
                {
                    this.translations.AddRange(digitalBiblePlatformTranslations);
                }

                // If the last page is zero, exit the loop
                if (lastPage == 0)
                {
                    break;
                }
            }
            else
            {
                // No data
                break;
            }
        }
    }

    /// <summary>
    /// Gets the copyright information.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The copyright information.
    /// </returns>
    private async Task<string> GetCopyrightAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        // Get the copyright information for the volume
        string url = $"bibles/{id}/copyright?key={this.options.ApiKey}&v=4";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            using HttpResponseMessage response = await this.HttpClient.GetAsync(
                url,
                cancellationToken
            );
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync(cancellationToken);
                await this.Cache.SetStringAsync(
                    cacheKey,
                    json,
                    CacheEntryOptions,
                    cancellationToken
                );
            }
            else
            {
                Debug.Print(
                    $"{response.StatusCode} error in DigitalBiblePlatformApi.GetCopyright()"
                );
                return string.Empty;
            }
        }

        // Default to empty string on error
        string copyright = string.Empty;
        if (!string.IsNullOrWhiteSpace(json))
        {
            var copyrightJson = DeserializeAnonymousType(
                json,
                EmptyListOf(
                    new { id = string.Empty, copyright = new { copyright = string.Empty, }, }
                )
            );

            if (copyrightJson is not null && copyrightJson.Count > 0)
            {
                copyright =
                    copyrightJson.FirstOrDefault(c => c.id == id)?.copyright.copyright
                    ?? copyrightJson.First().copyright.copyright;
            }
        }

        return copyright;
    }
}
