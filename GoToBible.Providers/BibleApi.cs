// -----------------------------------------------------------------------
// <copyright file="BibleApi.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GoToBible.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

/// <summary>
/// The API.Bible Provider.
/// </summary>
/// <seealso cref="IProvider" />
public partial class BibleApi : ApiProvider
{
    /// <summary>
    /// The regular expression to split verse lines.
    /// </summary>
    [GeneratedRegex("(^|[ ]{2,})\\[", RegexOptions.Compiled)]
    private static partial Regex VerseLinesRegex();

    /// <summary>
    /// The regular expression to clean up verse numbers.
    /// </summary>
    private static readonly Regex VerseNumberRegex = new Regex($"{Regex.Escape(Environment.NewLine)}(\\d+|\\d+\\-\\d+|\\d+[a-z])\\]", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the <see cref="BibleApi" /> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cache">The cache.</param>
    public BibleApi(IOptions<BibleApiOptions> options, IDistributedCache cache)
        : base(cache)
    {
        this.HttpClient.BaseAddress = new Uri("https://api.scripture.api.bible/v1/", UriKind.Absolute);
        if (!string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            this.HttpClient.DefaultRequestHeaders.Add("api-key", options.Value.ApiKey);
        }
    }

    /// <inheritdoc/>
    public override string Id => nameof(BibleApi);

    /// <inheritdoc/>
    public override string Name => "Bible.API";

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
    {
        string url = $"bibles/{translation}/books";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey);

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
                Debug.Print($"{response.StatusCode} error in BibleApi.GetBooksAsync()");
                yield break;
            }
        }

        var books = DeserializeAnonymousType(json, new
        {
            data = EmptyListOf(new
            {
                id = string.Empty,
                bibleId = string.Empty,
                abbreviation = string.Empty,
                name = string.Empty,
                nameLong = string.Empty,
            }),
        });
        if (books is not null)
        {
            foreach (var bookData in books.data)
            {
                string bookName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(ReverseBookCodeMap[bookData.id]);
                if (!includeChapters)
                {
                    // Return the book
                    yield return new Book
                    {
                        Name = bookName,
                    };
                }
                else
                {
                    // Get the chapters in the book
                    url = $"bibles/{translation}/books/{bookData.id}/chapters";
                    cacheKey = this.GetCacheKey(url);
                    json = await this.Cache.GetStringAsync(cacheKey);

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
                            Debug.Print($"{response.StatusCode} error in BibleApi.GetBooksAsync() [Chapters]");
                            yield break;
                        }
                    }

                    var chapters = DeserializeAnonymousType(json, new
                    {
                        data = EmptyListOf(new
                        {
                            id = string.Empty,
                            bibleId = string.Empty,
                            bookId = string.Empty,
                            number = string.Empty,
                            reference = string.Empty,
                        }),
                    });

                    List<ChapterReference> chapterReferences = new List<ChapterReference>();
                    if (chapters is not null)
                    {
                        foreach (var chapter in chapters.data)
                        {
                            int chapterNumber;
                            if (chapter.number == "intro")
                            {
                                chapterNumber = 0;
                            }
                            else if (!int.TryParse(chapter.number, out chapterNumber))
                            {
                                throw new ArgumentException($"Unknown Chapter: {chapter.number}", nameof(translation));
                            }

                            chapterReferences.Add(new ChapterReference(bookName, chapterNumber));
                        }
                    }

                    // Return the book
                    yield return new Book
                    {
                        Name = bookName,
                        Chapters = chapterReferences.AsReadOnly(),
                    };
                }
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

        // See if that book is in the translation
        bool bookExists = false;
        await foreach (Book translationBook in this.GetBooksAsync(translation, false))
        {
            if (translationBook.Name.ToLowerInvariant() == bookName)
            {
                bookExists = true;
                break;
            }
        }

        // The book does not exist in this translation
        if (!bookExists)
        {
            return chapter;
        }

        // Chapter 0 is the intro
        string chapterString = chapterNumber == 0 ? "intro" : chapterNumber.ToString(CultureInfo.InvariantCulture);

        // Load the book
        string url = $"bibles/{translation}/chapters/{bookCode}.{chapterString}?content-type=text&include-titles=false";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey);

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
                Debug.Print($"{response.StatusCode} error in BibleApi.GetChapterAsync()");
                return chapter;
            }
        }

        var chapterJson = DeserializeAnonymousType(json, new
        {
            data = new
            {
                id = string.Empty,
                content = string.Empty,
                copyright = string.Empty,
                next = Nullable(new
                {
                    number = string.Empty,
                    bookId = string.Empty,
                }),
                previous = Nullable(new
                {
                    number = string.Empty,
                    bookId = string.Empty,
                }),
            },
            meta = new
            {
                fumsNoScript = string.Empty,
            },
        });
        if (chapterJson?.data is not null)
        {
            // Get the text
            string output = chapterJson.data.content.Trim().Replace("\n", " ");
            output = VerseLinesRegex().Replace(output, Environment.NewLine);
            output = VerseNumberRegex.Replace(output, $"{Environment.NewLine}$1 ");
            chapter.Copyright = chapterJson.data.copyright.Trim().Replace("\n", " ") + chapterJson.meta.fumsNoScript;
            if (chapterNumber > 0
                && output[..1] != "1"
                && !(output.Contains($"{Environment.NewLine}1 ", StringComparison.OrdinalIgnoreCase) || output.StartsWith($"{Environment.NewLine}1", StringComparison.OrdinalIgnoreCase)))
            {
                output = $"1 {output}";
            }

            chapter.Text = output;

            // Get the previous chapter reference
            if (chapterJson.data.previous is not null
                && ReverseBookCodeMap.TryGetValue(chapterJson.data.previous.bookId, out string? previousBook))
            {
                if (!int.TryParse(chapterJson.data.previous.number, out int previousChapter))
                {
                    previousChapter = 0;
                }

                chapter.PreviousChapterReference = new ChapterReference(previousBook, previousChapter);
            }

            // Get the next chapter reference
            if (chapterJson.data.next is not null
                && ReverseBookCodeMap.TryGetValue(chapterJson.data.next.bookId, out string? nextBook))
            {
                if (!int.TryParse(chapterJson.data.next.number, out int nextChapter))
                {
                    nextChapter = 0;
                }

                chapter.NextChapterReference = new ChapterReference(nextBook, nextChapter);
            }
        }

        // Return the chapter
        return chapter;
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Translation> GetTranslationsAsync()
    {
        const string url = "bibles";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey);

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
                Debug.Print($"{response.StatusCode} error in BibleApi.GetTranslationsAsync()");
                yield break;
            }
        }

        var translations = DeserializeAnonymousType(json, new
        {
            data = EmptyListOf(new
            {
                id = string.Empty,
                name = string.Empty,
                description = string.Empty,
                language = new
                {
                    name = string.Empty,
                },
            }),
        });
        if (translations is not null)
        {
            foreach (var translation in translations.data)
            {
                string name = translation.name;
                if (translations.data.Count(t => t.name == name) > 1)
                {
                    name += $" ({translation.description})";
                }

                // Standardise the language
                string language = translation.language.name;
                language = language switch
                {
                    "Greek, Ancient" => "Greek",
                    "German, Standard" => "German",
                    _ => language,
                };

                yield return new Translation
                {
                    Code = translation.id,
                    Language = language,
                    Name = name,
                    Provider = this.Id,
                };
            }
        }
    }
}
