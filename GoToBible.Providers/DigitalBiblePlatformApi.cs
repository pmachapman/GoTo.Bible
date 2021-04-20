// -----------------------------------------------------------------------
// <copyright file="DigitalBiblePlatformApi.cs" company="Conglomo">
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
    using System.Text;
    using System.Threading.Tasks;
    using GoToBible.Model;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// The Digital Bible Platform API Provider.
    /// </summary>
    /// <seealso cref="GoToBible.Providers.ApiProvider" />
    public class DigitalBiblePlatformApi : ApiProvider
    {
        /// <summary>
        /// A map of the Digital Bible Platform API book codes to the Passage Reference book names.
        /// </summary>
        /// <remarks>
        /// Retrieved from <c>https://dbt.io/library/bookname?key={key}&amp;language_code=ENG&amp;v=2</c>.
        /// This contains the codes for all books supported by GoTo.Bible, but in reality the DBP API only returns the Old and New Testaments.
        /// </remarks>
        private static readonly ReadOnlyDictionary<string, string> BookCodeMap = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            // Old Testament
            ["genesis"] = "Gen",
            ["exodus"] = "Exod",
            ["leviticus"] = "Lev",
            ["numbers"] = "Num",
            ["deuteronomy"] = "Deut",
            ["joshua"] = "Josh",
            ["judges"] = "Judg",
            ["ruth"] = "Ruth",
            ["1 samuel"] = "1Sam",
            ["2 samuel"] = "2Sam",
            ["1 kings"] = "1Kgs",
            ["2 kings"] = "2Kgs",
            ["1 chronicles"] = "1Chr",
            ["2 chronicles"] = "2Chr",
            ["ezra"] = "Ezra",
            ["nehemiah"] = "Neh",
            ["esther"] = "Esth",
            ["job"] = "Job",
            ["psalm"] = "Ps",
            ["proverbs"] = "Prov",
            ["ecclesiastes"] = "Eccl",
            ["song of solomon"] = "Song",
            ["isaiah"] = "Isa",
            ["jeremiah"] = "Jer",
            ["lamentations"] = "Lam",
            ["ezekiel"] = "Ezek",
            ["daniel"] = "Dan",
            ["hosea"] = "Hos",
            ["joel"] = "Joel",
            ["amos"] = "Amos",
            ["obadiah"] = "Obad",
            ["jonah"] = "Jonah",
            ["micah"] = "Mic",
            ["nahum"] = "Nah",
            ["habakkuk"] = "Hab",
            ["zephaniah"] = "Zeph",
            ["haggai"] = "Hag",
            ["zechariah"] = "Zech",
            ["malachi"] = "Mal",

            // Old Testament Apocrypha
            ["1 esdras"] = "1Esd",
            ["2 esdras"] = "2Esd",
            ["tobit"] = "Tob",
            ["judith"] = "Jdt",
            ["esther (greek)"] = "AddEsth",
            ["wisdom"] = "Wis",
            ["ecclesiasticus"] = "Sir",
            ["baruch"] = "Bar",
            ["epistle of jeremy"] = "EpJer",
            ["daniel (greek)"] = "DanGr",
            ["song of the three"] = "PrAzar",
            ["susanna"] = "Sus",
            ["bel and the dragon"] = "Bel",
            ["manasseh"] = "PrMan",
            ["1 maccabees"] = "1Macc",
            ["2 maccabees"] = "2Macc",
            ["3 maccabees"] = "3Macc",
            ["4 maccabees"] = "4Macc",
            ["psalm 151"] = "Ps151",

            // New Testament
            ["matthew"] = "Matt",
            ["mark"] = "Mark",
            ["luke"] = "Luke",
            ["john"] = "John",
            ["acts"] = "Acts",
            ["romans"] = "Rom",
            ["1 corinthians"] = "1Cor",
            ["2 corinthians"] = "2Cor",
            ["galatians"] = "Gal",
            ["ephesians"] = "Eph",
            ["philippians"] = "Phil",
            ["colossians"] = "Col",
            ["1 thessalonians"] = "1Thess",
            ["2 thessalonians"] = "2Thess",
            ["1 timothy"] = "1Tim",
            ["2 timothy"] = "2Tim",
            ["titus"] = "Titus",
            ["philemon"] = "Phlm",
            ["hebrews"] = "Heb",
            ["james"] = "Jas",
            ["1 peter"] = "1Pet",
            ["2 peter"] = "2Pet",
            ["1 john"] = "1John",
            ["2 john"] = "2John",
            ["3 john"] = "3John",
            ["jude"] = "Jude",
            ["revelation"] = "Rev",

            // New Testament Apocrypha
            ["laodiceans"] = "EpLao",
        });

        /// <summary>
        /// The reverse book code map.
        /// </summary>
        private static readonly ReadOnlyDictionary<string, string> ReverseBookCodeMap
            = new ReadOnlyDictionary<string, string>(BookCodeMap.ToDictionary(x => x.Value, x => x.Key));

        /// <summary>
        /// The new testament canon.
        /// </summary>
        private static readonly BookHelper NewTestamentCanon = new NewTestamentCanon();

        /// <summary>
        /// The options.
        /// </summary>
        private readonly DigitalBiblePlatformApiOptions options;

        /// <summary>
        /// The translations cache.
        /// </summary>
        private readonly List<DigitalBiblePlatformTranslation> translations = new List<DigitalBiblePlatformTranslation>();

        /// <summary>
        /// Initialises a new instance of the <see cref="DigitalBiblePlatformApi" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cache">The cache.</param>
        public DigitalBiblePlatformApi(IOptions<DigitalBiblePlatformApiOptions> options, IDistributedCache cache)
            : base(cache)
        {
            this.options = options.Value;
            this.HttpClient.BaseAddress = new Uri("https://dbt.io/", UriKind.Absolute);
        }

        /// <inheritdoc/>
        public override string Id => nameof(DigitalBiblePlatformApi);

        /// <inheritdoc/>
        public override string Name => "Digital Bible Platform API";

        /// <inheritdoc/>
        public override bool SupportsItalics { get; } = false;

        /// <inheritdoc/>
        public override async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
        {
            string url = $"library/book?key={this.options.ApiKey}&dam_id={translation}&v=2";
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
                    Debug.Print($"{response.StatusCode} error in DigitalBiblePlatformApi.GetBooksAsync()");
                    yield break;
                }
            }

            var books = DeserializeAnonymousType(json, EmptyListOf(new
            {
                book_id = string.Empty,
                chapters = string.Empty,
            }));

            if (books?.Any() ?? false)
            {
                foreach (var book in books)
                {
                    string bookName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(ReverseBookCodeMap[book.book_id]);
                    List<ChapterReference> chapterReferences = new List<ChapterReference>();
                    if (includeChapters)
                    {
                        IEnumerable<string> chapters = book.chapters.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => $"{bookName} {c}");
                        foreach (string bookAndChapter in chapters)
                        {
                            chapterReferences.Add(new ChapterReference(bookAndChapter));
                        }
                    }

                    yield return new Book
                    {
                        Chapters = chapterReferences.AsReadOnly(),
                        Name = bookName,
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

            // Select the DAM id
            DigitalBiblePlatformTranslation digitalBiblePlatformTranslation = this.translations.Single(t => t.Code == translation);
            string damId = translation;
            if (digitalBiblePlatformTranslation.DamIds.Count > 1)
            {
                if (NewTestamentCanon.HasBook(bookName))
                {
                    damId = digitalBiblePlatformTranslation.DamIds.FirstOrDefault(d => d.EndsWith("N1ET") || d.EndsWith("N2ET")
                        || d.EndsWith("C1ET") || d.EndsWith("C2ET")
                        || d.EndsWith("P1ET") || d.EndsWith("P2ET")) ?? string.Empty;
                }
                else
                {
                    damId = digitalBiblePlatformTranslation.DamIds.FirstOrDefault(d => d.EndsWith("O1ET") || d.EndsWith("O2ET")
                        || d.EndsWith("C1ET") || d.EndsWith("C2ET")
                        || d.EndsWith("P1ET") || d.EndsWith("P2ET")) ?? string.Empty;
                }
            }

            // Make sure we have a DAM id
            if (string.IsNullOrWhiteSpace(damId))
            {
                return chapter;
            }

            // Load the book
            string url = $"text/verse?key={this.options.ApiKey}&dam_id={damId}&book_id={bookCode}&chapter_id={chapterNumber}&v=2";
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
                    Debug.Print($"{response.StatusCode} error in DigitalBiblePlatformApi.GetChapterAsync()");
                    return chapter;
                }
            }

            if (!string.IsNullOrWhiteSpace(json))
            {
                var chapterJson = DeserializeAnonymousType(json, EmptyListOf(new
                {
                    verse_id = string.Empty,
                    verse_text = string.Empty,
                }));

                // Get the text
                StringBuilder sb = new StringBuilder();
                if (chapterJson != null)
                {
                    foreach (var verse in chapterJson)
                    {
                        string verseText = verse.verse_text.Trim();

                        // Allow multi-verse lines, i.e. "1-2  In the beginning"
                        if (verseText == "-")
                        {
                            sb.Append(verse.verse_id);
                            sb.Append(verseText);
                        }
                        else
                        {
                            sb.Append(verse.verse_id);
                            sb.Append("  ");
                            sb.AppendLine(verseText);
                        }
                    }
                }

                chapter.Text = sb.ToString();

                // Get the copyright information
                chapter.Copyright = await this.GetCopyright(damId);

                // See if we are only dealing with a partial translation
                string translationDamId = damId.EndsWith("P1ET") || damId.EndsWith("P2ET") ? damId : translation;

                // Get the next/previous chapters
                bool getNextChapter = false;
                string previousChapter = string.Empty;
                string thisChapter = $"{book} {chapterNumber}";
                await foreach (string nextChapter in this.GetChaptersAsync(translationDamId))
                {
                    if (getNextChapter)
                    {
                        chapter.NextChapterReference = new ChapterReference(nextChapter);
                        break;
                    }
                    else if (string.Compare(nextChapter, thisChapter, true) == 0)
                    {
                        chapter.PreviousChapterReference = new ChapterReference(previousChapter);
                        getNextChapter = true;
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
            string url = $"library/volume?key={this.options.ApiKey}&media=text&v=2";
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
                    Debug.Print($"{response.StatusCode} error in DigitalBiblePlatformApi.GetTranslationsAsync()");
                    yield break;
                }
            }

            var translations = DeserializeAnonymousType(json, EmptyListOf(new
            {
                dam_id = string.Empty,
                language_family_name = string.Empty,
                language_iso_name = string.Empty,
                language_name = string.Empty,
                version_code = string.Empty,
                volume_name = string.Empty,
            }));
            if (translations is not null)
            {
                List<DigitalBiblePlatformTranslation> digitalBiblePlatformTranslations = new List<DigitalBiblePlatformTranslation>();
                foreach (var translation in translations.GroupBy(t => t.dam_id[..6]))
                {
                    string language = translation.First().language_iso_name;

                    // Some are erroneous labelled "English". If they are correct, language_name will be "English" anyway
                    if (string.IsNullOrWhiteSpace(language) || language == "English")
                    {
                        language = translation.First().language_name;
                    }

                    string code = translation.Key;
                    ReadOnlyCollection<string> damIds = translation.Select(t => t.dam_id).ToList().AsReadOnly();
                    if (damIds.Count == 1)
                    {
                        code = damIds.First();
                    }

                    DigitalBiblePlatformTranslation digitalBiblePlatformTranslation = new DigitalBiblePlatformTranslation
                    {
                        Code = code,
                        DamIds = damIds,
                        Dialect = translation.First().language_family_name,
                        Language = language,
                        Name = translation.First().volume_name,
                        Provider = this.Id,
                    };
                    digitalBiblePlatformTranslations.Add(digitalBiblePlatformTranslation);
                    yield return digitalBiblePlatformTranslation;
                }

                // Initialise the cache if we should
                if (!this.translations.Any())
                {
                    this.translations.AddRange(digitalBiblePlatformTranslations);
                }
            }
        }

        /// <summary>
        /// Gets the copyright information.
        /// </summary>
        /// <param name="damId">The dam identifier.</param>
        /// <returns>
        /// The copyright information.
        /// </returns>
        private async Task<string> GetCopyright(string damId)
        {
            // Get the copyright information for the volume
            string url = $"library/metadata?key={this.options.ApiKey}&dam_id={damId}&v=2";
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
                    Debug.Print($"{response.StatusCode} error in DigitalBiblePlatformApi.GetCopyright()");
                    return string.Empty;
                }
            }

            // Default to empty string on error
            string copyright = string.Empty;
            if (!string.IsNullOrWhiteSpace(json))
            {
                var copyrightJson = DeserializeAnonymousType(json, EmptyListOf(new
                {
                    mark = string.Empty,
                    organization = EmptyListOf(new
                    {
                        organization = string.Empty,
                        organization_english = string.Empty,
                        organization_role = string.Empty,
                    }),
                }));

                if (copyrightJson?.Any() ?? false)
                {
                    copyright = copyrightJson.First().mark;
                    if (string.IsNullOrWhiteSpace(copyright))
                    {
                        if (copyrightJson.First().organization.Any(o => o.organization_role == "holder"))
                        {
                            // Use the copyright holder name, if the mark is empty
                            var organisation = copyrightJson.First().organization.First(o => o.organization_role == "holder");
                            if (!string.IsNullOrWhiteSpace(organisation.organization))
                            {
                                copyright = organisation.organization;
                            }
                            else
                            {
                                copyright = organisation.organization_english;
                            }
                        }
                        else
                        {
                            // Just choose some one
                            var organisation = copyrightJson.First().organization.First();
                            if (!string.IsNullOrWhiteSpace(organisation.organization))
                            {
                                copyright = organisation.organization;
                            }
                            else
                            {
                                copyright = organisation.organization_english;
                            }
                        }
                    }
                }
            }

            return copyright;
        }
    }
}
