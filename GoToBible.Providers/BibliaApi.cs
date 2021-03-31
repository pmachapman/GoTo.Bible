// -----------------------------------------------------------------------
// <copyright file="BibliaApi.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using GoToBible.Model;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// The Biblia API Provider.
    /// </summary>
    /// <seealso cref="GoToBible.Providers.ApiProvider" />
    /// <seealso cref="GoToBible.Model.IProvider" />
    public class BibliaApi : ApiProvider
    {
        /// <summary>
        /// The copyright message.
        /// </summary>
        private const string Copyright = "This site uses the <a href=\"http://biblia.com/\" target=\"_blank\">Biblia</a> web services from <a href=\"http://www.logos.com/\" target=\"_blank\">Logos Bible Software</a>.";

        /// <summary>
        /// The canon.
        /// </summary>
        private static readonly BookHelper Canon = new ProtestantCanon();

        /// <summary>
        /// The options.
        /// </summary>
        private readonly BibliaApiOptions options;

        /// <summary>
        /// Initialises a new instance of the <see cref="BibliaApi" /> class.
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
        public override bool SupportsItalics { get; } = false;

        /// <inheritdoc/>
        public override async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
        {
            string url = $"contents/{translation}.txt?key={this.options.ApiKey}";
            string cacheKey = this.GetCacheKey(url);
            string json = await this.Cache.GetStringAsync(cacheKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                using HttpResponseMessage response = await this.HttpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync();
                    await this.Cache.SetStringAsync(cacheKey, json, CacheEntryOptions);
                }
                else
                {
                    Debug.Print($"{response.StatusCode} error in BibliaApi.GetBooksAsync()");
                    yield break;
                }
            }

            var chapters = DeserializeAnonymousType(json, new
            {
                books = EmptyListOf(new
                {
                    passage = string.Empty,
                    chapters = EmptyListOf(new
                    {
                        passage = string.Empty,
                    }),
                }),
            });

            if (chapters?.books.Any() ?? false)
            {
                foreach (var book in chapters.books)
                {
                    List<ChapterReference> chapterReferences = new List<ChapterReference>();
                    if (includeChapters)
                    {
                        foreach (string bookAndChapter in book.chapters.Select(c => c.passage))
                        {
                            chapterReferences.Add(new ChapterReference(bookAndChapter));
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
        public override async Task<Chapter> GetChapterAsync(string translation, string book, int chapterNumber)
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
            string chapterPart;
            if (Canon.GetNumberOfChapters(book) == 1)
            {
                chapterPart = string.Empty;
            }
            else
            {
                chapterPart = $"+{chapterNumber}";
            }

            // Load the book
            string url = $"content/{translation}.txt?key={this.options.ApiKey}&passage={book}{chapterPart}&eachVerse=[VerseNum]++[VerseText]\\n";
            string cacheKey = this.GetCacheKey(url);
            string output = await this.Cache.GetStringAsync(cacheKey);

            if (string.IsNullOrWhiteSpace(output))
            {
                using HttpResponseMessage response = await this.HttpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    output = await response.Content.ReadAsStringAsync();
                    await this.Cache.SetStringAsync(cacheKey, output, CacheEntryOptions);
                }
                else
                {
                    Debug.Print($"{response.StatusCode} error in BibliaApi.GetChapterAsync()");
                    return chapter;
                }
            }

            if (!string.IsNullOrWhiteSpace(output))
            {
                // Get the text
                chapter.Text = output.NormaliseLineEndings().Replace("\n", string.Empty).Replace("\\n", "\r\n", StringComparison.OrdinalIgnoreCase).Trim();

                // Get the next/previous chapters
                string previousChapter = string.Empty;
                string thisChapter = $"{book} {chapterNumber}";
                await foreach (string nextChapter in this.GetChaptersAsync(translation))
                {
                    if (chapter.PreviousChapterReference.IsValid)
                    {
                        chapter.NextChapterReference = new ChapterReference(nextChapter);
                        break;
                    }
                    else if (nextChapter == thisChapter)
                    {
                        chapter.PreviousChapterReference = new ChapterReference(previousChapter);
                        continue;
                    }

                    // Set the previous chapter for the next iteration (if it needs it)
                    previousChapter = nextChapter;
                }

                // Return the chapter
                return chapter;
            }
            else
            {
                return chapter;
            }
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<Translation> GetTranslationsAsync()
        {
            string url = $"find?key={this.options.ApiKey}";
            string cacheKey = this.GetCacheKey(url);
            string json = await this.Cache.GetStringAsync(cacheKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                using HttpResponseMessage response = await this.HttpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync();
                    await this.Cache.SetStringAsync(cacheKey, json, CacheEntryOptions);
                }
                else
                {
                    Debug.Print($"{response.StatusCode} error in BibliaApi.GetTranslationsAsync()");
                    yield break;
                }
            }

            var translations = DeserializeAnonymousType(json, new
            {
                bibles = EmptyListOf(new
                {
                    bible = string.Empty,
                    title = string.Empty,
                    publicationDate = string.Empty,
                    languages = new List<string>(),
                    copyright = string.Empty,
                    extendedCopyright = string.Empty,
                }),
            });
            if (translations is not null)
            {
                foreach (var translation in translations.bibles)
                {
                    if (!int.TryParse(translation.publicationDate, out int year))
                    {
                        year = 0;
                    }

                    string? language = null;
                    if (translation.languages.Any())
                    {
                        language = new CultureInfo(translation.languages.First()).DisplayName;
                    }

                    // Get the text copyright
                    string? copyright = string.Empty;
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

        /// <summary>
        /// Gets all of the books and chapters in a translation asynchronously.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <returns>
        /// The list of chapters.
        /// </returns>
        private async IAsyncEnumerable<string> GetChaptersAsync(string translation)
        {
            await foreach (Book book in this.GetBooksAsync(translation, true))
            {
                foreach (ChapterReference chapter in book.Chapters)
                {
                    yield return chapter.ToString();
                }
            }
        }
    }
}
