// -----------------------------------------------------------------------
// <copyright file="BibleApi.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
    public class BibleApi : ApiProvider
    {
        /// <summary>
        /// A map of the Bible API book codes to the Passage Reference book names.
        /// </summary>
        private static readonly ReadOnlyDictionary<string, string> BookCodeMap = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            // Old Testament
            ["genesis"] = "GEN",
            ["exodus"] = "EXO",
            ["leviticus"] = "LEV",
            ["numbers"] = "NUM",
            ["deuteronomy"] = "DEU",
            ["joshua"] = "JOS",
            ["judges"] = "JDG",
            ["ruth"] = "RUT",
            ["1 samuel"] = "1SA",
            ["2 samuel"] = "2SA",
            ["1 kings"] = "1KI",
            ["2 kings"] = "2KI",
            ["1 chronicles"] = "1CH",
            ["2 chronicles"] = "2CH",
            ["ezra"] = "EZR",
            ["nehemiah"] = "NEH",
            ["esther"] = "EST",
            ["job"] = "JOB",
            ["psalm"] = "PSA",
            ["proverbs"] = "PRO",
            ["ecclesiastes"] = "ECC",
            ["song of solomon"] = "SNG",
            ["isaiah"] = "ISA",
            ["jeremiah"] = "JER",
            ["lamentations"] = "LAM",
            ["ezekiel"] = "EZK",
            ["daniel"] = "DAN",
            ["hosea"] = "HOS",
            ["joel"] = "JOL",
            ["amos"] = "AMO",
            ["obadiah"] = "OBA",
            ["jonah"] = "JON",
            ["micah"] = "MIC",
            ["nahum"] = "NAM",
            ["habakkuk"] = "HAB",
            ["zephaniah"] = "ZEP",
            ["haggai"] = "HAG",
            ["zechariah"] = "ZEC",
            ["malachi"] = "MAL",

            // Old Testament Apocrypha
            ["1 esdras"] = "1ES",
            ["2 esdras"] = "2ES",
            ["tobit"] = "TOB",
            ["judith"] = "JDT",
            ["esther (greek)"] = "ESG",
            ["wisdom"] = "WIS",
            ["ecclesiasticus"] = "SIR",
            ["baruch"] = "BAR",
            ["epistle of jeremy"] = "LJE",
            ["daniel (greek)"] = "DAG",
            ["song of the three"] = "S3Y",
            ["susanna"] = "SUS",
            ["bel and the dragon"] = "BEL",
            ["manasseh"] = "MAN",
            ["1 maccabees"] = "1MA",
            ["2 maccabees"] = "2MA",
            ["3 maccabees"] = "3MA",
            ["4 maccabees"] = "4MA",
            ["psalm 151"] = "PS2",

            // New Testament
            ["matthew"] = "MAT",
            ["mark"] = "MRK",
            ["luke"] = "LUK",
            ["john"] = "JHN",
            ["acts"] = "ACT",
            ["romans"] = "ROM",
            ["1 corinthians"] = "1CO",
            ["2 corinthians"] = "2CO",
            ["galatians"] = "GAL",
            ["ephesians"] = "EPH",
            ["philippians"] = "PHP",
            ["colossians"] = "COL",
            ["1 thessalonians"] = "1TH",
            ["2 thessalonians"] = "2TH",
            ["1 timothy"] = "1TI",
            ["2 timothy"] = "2TI",
            ["titus"] = "TIT",
            ["philemon"] = "PHM",
            ["hebrews"] = "HEB",
            ["james"] = "JAS",
            ["1 peter"] = "1PE",
            ["2 peter"] = "2PE",
            ["1 john"] = "1JN",
            ["2 john"] = "2JN",
            ["3 john"] = "3JN",
            ["jude"] = "JUD",
            ["revelation"] = "REV",

            // New Testament Apocrypha
            ["laodiceans"] = "LAO",
        });

        /// <summary>
        /// The reverse book code map.
        /// </summary>
        private static readonly ReadOnlyDictionary<string, string> ReverseBookCodeMap
            = new ReadOnlyDictionary<string, string>(BookCodeMap.ToDictionary(x => x.Value, x => x.Key));

        /// <summary>
        /// The regular expression to split verse lines.
        /// </summary>
        private static readonly Regex VerseLinesRegex = new Regex("(^|[ ]{2,})\\[", RegexOptions.Compiled);

        /// <summary>
        /// The regular expression to clean up verse numbers.
        /// </summary>
        private static readonly Regex VerseNumberRegex = new Regex($"{Regex.Escape(Environment.NewLine)}(\\d+|\\d+\\-\\d+|\\d+[a-z])\\]", RegexOptions.Compiled);

        /// <summary>
        /// Initialises a new instance of the <see cref="BibleApi" /> class.
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
        public override bool SupportsItalics { get; } = false;

        /// <inheritdoc/>
        public override async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
        {
            string url = $"bibles/{translation}/books";
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
            if (!BookCodeMap.ContainsKey(bookName))
            {
                return chapter;
            }

            // Get the book code
            string bookCode = BookCodeMap[bookName];

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
                output = VerseLinesRegex.Replace(output, Environment.NewLine);
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
                if (chapterJson.data.previous != null && ReverseBookCodeMap.ContainsKey(chapterJson.data.previous.bookId))
                {
                    string previousBook = ReverseBookCodeMap[chapterJson.data.previous.bookId];
                    if (!int.TryParse(chapterJson.data.previous.number, out int previousChapter))
                    {
                        previousChapter = 0;
                    }

                    chapter.PreviousChapterReference = new ChapterReference(previousBook, previousChapter);
                }

                // Get the next chapter reference
                if (chapterJson.data.next != null && ReverseBookCodeMap.ContainsKey(chapterJson.data.next.bookId))
                {
                    string nextBook = ReverseBookCodeMap[chapterJson.data.next.bookId];
                    if (!int.TryParse(chapterJson.data.next.number, out int nextChapter))
                    {
                        nextChapter = 0;
                    }

                    chapter.NextChapterReference = new ChapterReference(nextBook, nextChapter);
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
            string url = "bibles";
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
                    if (language == "Greek, Ancient")
                    {
                        language = "Greek";
                    }
                    else if (language == "German, Standard")
                    {
                        language = "German";
                    }

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
}
