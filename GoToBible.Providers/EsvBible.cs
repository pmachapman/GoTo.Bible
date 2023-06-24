// -----------------------------------------------------------------------
// <copyright file="EsvBible.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

/// <summary>
/// The ESV Bible Provider.
/// </summary>
/// <seealso cref="WebApiProvider" />
public partial class EsvBible : WebApiProvider
{
    /// <summary>
    /// The copyright message.
    /// </summary>
    private const string Copyright = "Scripture quotations are from the ESV&reg; Bible (The Holy Bible, English Standard Version&reg;), copyright &copy; 2001 by Crossway, a publishing ministry of Good News Publishers. Used by permission. All rights reserved. You may not copy or download more than 500 consecutive verses of the ESV Bible or more than one half of any book of the ESV Bible.";

    /// <summary>
    /// The books of the bible.
    /// </summary>
    private static readonly string[] Books =
    {
        "Genesis",
        "Exodus",
        "Leviticus",
        "Numbers",
        "Deuteronomy",
        "Joshua",
        "Judges",
        "Ruth",
        "1 Samuel",
        "2 Samuel",
        "1 Kings",
        "2 Kings",
        "1 Chronicles",
        "2 Chronicles",
        "Ezra",
        "Nehemiah",
        "Esther",
        "Job",
        "Psalm",
        "Proverbs",
        "Ecclesiastes",
        "Song of Solomon",
        "Isaiah",
        "Jeremiah",
        "Lamentations",
        "Ezekiel",
        "Daniel",
        "Hosea",
        "Joel",
        "Amos",
        "Obadiah",
        "Jonah",
        "Micah",
        "Nahum",
        "Habakkuk",
        "Zephaniah",
        "Haggai",
        "Zechariah",
        "Malachi",
        "Matthew",
        "Mark",
        "Luke",
        "John",
        "Acts",
        "Romans",
        "1 Corinthians",
        "2 Corinthians",
        "Galatians",
        "Ephesians",
        "Philippians",
        "Colossians",
        "1 Thessalonians",
        "2 Thessalonians",
        "1 Timothy",
        "2 Timothy",
        "Titus",
        "Philemon",
        "Hebrews",
        "James",
        "1 Peter",
        "2 Peter",
        "1 John",
        "2 John",
        "3 John",
        "Jude",
        "Revelation",
    };

    /// <summary>
    /// The canon.
    /// </summary>
    private static readonly BookHelper Canon = new ProtestantCanon();

    /// <summary>
    /// This translation.
    /// </summary>
    private static readonly Translation Translation = new Translation
    {
        Code = "ESV",
        Copyright = Copyright,
        Language = "English",
        Name = "English Standard Version",
        Provider = nameof(EsvBible),
        Year = 2016, // The latest revision
    };

    /// <summary>
    /// The options.
    /// </summary>
    private readonly EsvBibleOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EsvBible" /> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cache">The cache.</param>
    public EsvBible(IOptions<EsvBibleOptions> options, IDistributedCache cache)
        : base(cache)
    {
        this.HttpClient.BaseAddress = new Uri("https://api.esv.org/v3/passage/text/", UriKind.Absolute);
        this.options = options.Value;
        if (!string.IsNullOrWhiteSpace(this.options.ApiKey))
        {
            string apiKey = this.options.ApiKey;
            if (!apiKey.StartsWith("Token", StringComparison.OrdinalIgnoreCase))
            {
                apiKey = "Token " + apiKey;
            }

            this.HttpClient.DefaultRequestHeaders.Add("Authorization", apiKey);
        }
    }

    /// <inheritdoc/>
    public override string Id => nameof(EsvBible);

    /// <inheritdoc/>
    public override string Name => "ESV API";

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (Book book in Canon.GetBooks(includeChapters))
        {
            yield return await Task.FromResult(book);
        }
    }

    /// <inheritdoc/>
    public override async Task<Chapter> GetChapterAsync(string translation, string book, int chapterNumber, CancellationToken cancellationToken = default)
    {
        // Set up the chapter
        Chapter chapter = new Chapter
        {
            Book = book,
            ChapterNumber = chapterNumber,
            Copyright = Copyright,
            NextChapterReference = new ChapterReference(),
            PreviousChapterReference = new ChapterReference(),
            Translation = translation,
        };

        // If there is only one chapter, do not use a chapter number
        string chapterPart = Canon.GetNumberOfChapters(book) == 1 ? string.Empty : $"+{chapterNumber}";

        // Load the book
        string url = $"?q={book}{chapterPart}&include-passage-references=false&include-footnotes=false&include-headings=false&include-short-copyright=false&indent-poetry=false";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            using HttpResponseMessage response = await this.HttpClient.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync(cancellationToken);
                await this.Cache.SetStringAsync(cacheKey, json, CacheEntryOptions, cancellationToken);
            }
            else
            {
                Debug.Print($"{response.StatusCode} error in EsvBible.GetChapterAsync()");
                return chapter;
            }
        }

        var data = DeserializeAnonymousType(json, new
        {
            passage_meta = EmptyListOf(new
            {
                prev_chapter = default(List<int>?),
                next_chapter = default(List<int>?),
            }),
            passages = new List<string>(),
        });
        if (data is not null && data.passages.Any())
        {
            // Get the text
            string output = data.passages.First();

            // Clean up the Song of Solomon
            output = output.Replace("\n\nHe\n\n", "\n").Replace("\n\nShe\n\n", "\n").Replace("\n\nOthers\n\n", "\n");

            // Final clean up
            output = output.Trim().Replace("\n", " ").Replace("  ", " ");

            // Strip Psalm sub heading
            if (output[..3] != "[1]")
            {
                output = output[output.IndexOf('[')..];
            }

            output = output.RemoveDuplicateSpaces();
            output = VerseNumberRegex().Replace(output, $"{Environment.NewLine}$1  ");
            chapter.Text = output;

            // Get the previous and next chapter references
            if (data.passage_meta.Any())
            {
                if (data.passage_meta.First().prev_chapter?.Any() ?? false)
                {
                    chapter.PreviousChapterReference = GetChapterReference(data.passage_meta.First().prev_chapter?.First() ?? 0);
                }

                if (data.passage_meta.First().next_chapter?.Any() ?? false)
                {
                    chapter.NextChapterReference = GetChapterReference(data.passage_meta.First().next_chapter?.First() ?? 0);
                }
            }
        }

        // Return the chapter
        return chapter;
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Translation> GetTranslationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(this.options.ApiKey))
        {
            yield return await Task.FromResult(Translation);
        }
    }

    /// <summary>
    /// Gets the chapter reference from a passage number.
    /// </summary>
    /// <param name="passage">The passage.</param>
    /// <returns>
    /// The chapter reference.
    /// </returns>
    private static ChapterReference GetChapterReference(int passage)
    {
        if (passage > 1000000)
        {
            int chapterVerse = passage % 1000000;
            int bookIndex = (passage / 1000000) - 1;
            int chapter = chapterVerse / 1000;
            if (bookIndex < Books.Length)
            {
                return new ChapterReference(Books[bookIndex], chapter);
            }
        }

        // Default to invalid reference
        return new ChapterReference();
    }

    /// <summary>
    /// The regular expression to find verse numbers.
    /// </summary>
    [GeneratedRegex("\\[(\\d+)\\] ", RegexOptions.Compiled)]
    private static partial Regex VerseNumberRegex();
}
