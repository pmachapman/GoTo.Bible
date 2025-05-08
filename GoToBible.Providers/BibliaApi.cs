// -----------------------------------------------------------------------
// <copyright file="BibliaApi.cs" company="Conglomo">
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
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

/// <summary>
/// The Biblia API Provider.
/// </summary>
/// <seealso cref="WebApiProvider" />
public class BibliaApi : WebApiProvider
{
    /// <summary>
    /// The copyright message.
    /// </summary>
    private const string Copyright =
        "This site uses the <a href=\"http://biblia.com/\" target=\"_blank\">Biblia</a> web services from <a href=\"http://www.logos.com/\" target=\"_blank\">Logos Bible Software</a>.";

    /// <summary>
    /// The canon.
    /// </summary>
    private static readonly BookHelper Canon = new ProtestantCanon();

    /// <summary>
    /// The options.
    /// </summary>
    private readonly BibliaApiOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="BibliaApi" /> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cache">The cache.</param>
    public BibliaApi(IOptions<BibliaApiOptions> options, IDistributedCache cache)
        : base(cache)
    {
        this.options = options.Value;
        this.HttpClient.BaseAddress = new Uri("https://api.biblia.com/v1/bible/", UriKind.Absolute);
    }

    /// <inheritdoc/>
    public override string Id => nameof(BibliaApi);

    /// <inheritdoc/>
    public override string Name => "Biblia API";

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Book> GetBooksAsync(
        string translation,
        bool includeChapters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        string url = $"contents/{translation}.txt?key={this.options.ApiKey}";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.WriteLine($"GET: {this.HttpClient.BaseAddress}{url}");
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
                Debug.WriteLine($"{response.StatusCode} error in BibliaApi.GetBooksAsync()");
                yield break;
            }
        }

        var chapters = DeserializeAnonymousType(
            json,
            new
            {
                books = EmptyListOf(
                    new
                    {
                        passage = string.Empty,
                        chapters = EmptyListOf(new { passage = string.Empty, }),
                    }
                ),
            }
        );

        if (chapters is not null && chapters.books.Count > 0)
        {
            foreach (var book in chapters.books)
            {
                List<ChapterReference> chapterReferences = [];
                if (includeChapters)
                {
                    foreach (string bookAndChapter in book.chapters.Select(c => c.passage))
                    {
                        ChapterReference chapterReference = new ChapterReference(bookAndChapter);
                        if (chapterReference.ChapterNumber == 0)
                        {
                            // Fix Obadiah, Philemon, 2 John, 3 John, Jude
                            chapterReference.ChapterNumber = 1;
                        }

                        chapterReferences.Add(chapterReference);
                    }
                }

                yield return new Book
                {
                    Chapters = chapterReferences.AsReadOnly(),
                    Name = book.passage,
                };
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

        // If there is only one chapter, do not use a chapter number
        string chapterPart =
            Canon.GetNumberOfChapters(book) == 1 ? string.Empty : $"+{chapterNumber}";

        // Load the book
        string url =
            $"content/{translation}.txt?key={this.options.ApiKey}&passage={book}{chapterPart}&eachVerse=[VerseNum]++[VerseText]\\n";
        string cacheKey = this.GetCacheKey(url);
        string? output = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(output))
        {
            Debug.WriteLine($"GET: {this.HttpClient.BaseAddress}{url}");
            using HttpResponseMessage response = await this.HttpClient.GetAsync(
                url,
                cancellationToken
            );
            if (response.IsSuccessStatusCode)
            {
                output = await response.Content.ReadAsStringAsync(cancellationToken);
                await this.Cache.SetStringAsync(
                    cacheKey,
                    output,
                    CacheEntryOptions,
                    cancellationToken
                );
            }
            else
            {
                Debug.WriteLine($"{response.StatusCode} error in BibliaApi.GetChapterAsync()");
                return chapter;
            }
        }

        if (!string.IsNullOrWhiteSpace(output))
        {
            // Get the text
            chapter.Text = output
                .NormaliseLineEndings()
                .Replace("\n", string.Empty)
                .Replace("\\n", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
                .Trim();

            // Clean up Stephanus' Textus Receptus
            if (
                chapter.Text.StartsWith(
                    $"1  {Environment.NewLine}1",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                chapter.Text = chapter.Text[5..];
            }

            // Clean up Modifier Letter Apostrophes in scrmorph
            chapter.Text = chapter.Text.Replace("ʼ", "᾿");

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
        string url = $"find?key={this.options.ApiKey}";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.WriteLine($"GET: {this.HttpClient.BaseAddress}{url}");
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
                Debug.WriteLine($"{response.StatusCode} error in BibliaApi.GetTranslationsAsync()");
                yield break;
            }
        }

        var translations = DeserializeAnonymousType(
            json,
            new
            {
                bibles = EmptyListOf(
                    new
                    {
                        bible = string.Empty,
                        title = string.Empty,
                        publicationDate = string.Empty,
                        languages = new List<string>(),
                        copyright = string.Empty,
                        extendedCopyright = string.Empty,
                    }
                ),
            }
        );
        if (translations is not null)
        {
            foreach (var translation in translations.bibles)
            {
                if (!int.TryParse(translation.publicationDate, out int year))
                {
                    year = 0;
                }

                string? language = null;
                if (translation.languages.Count > 0)
                {
                    language = new CultureInfo(translation.languages.First()).DisplayName;
                }

                // Get the text copyright
                string copyright = string.Empty;
                if (!string.IsNullOrWhiteSpace(translation.extendedCopyright))
                {
                    copyright = translation.extendedCopyright;
                }
                else if (!string.IsNullOrWhiteSpace(translation.copyright))
                {
                    copyright = translation.copyright;
                }

                // Get the provider copyright
                if (!string.IsNullOrWhiteSpace(copyright))
                {
                    if (!copyright.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                    {
                        copyright += ".";
                    }

                    copyright += " " + Copyright;
                }

                yield return new Translation
                {
                    Code = translation.bible,
                    Copyright = copyright,
                    Language = language,
                    Name = translation.title,
                    Provider = this.Id,
                    Year = year,
                };
            }
        }
    }
}
