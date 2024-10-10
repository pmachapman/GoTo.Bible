// -----------------------------------------------------------------------
// <copyright file="NetBible.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;
using Microsoft.Extensions.Caching.Distributed;

/// <summary>
/// The NET Bible Provider.
/// </summary>
/// <seealso cref="WebApiProvider" />
public class NetBible : WebApiProvider
{
    /// <summary>
    /// The copyright message.
    /// </summary>
    private const string Copyright =
        "The Scriptures quoted are from the NET Bible&reg; <a href=\"http://netbible.com\" target=\"_blank\">http://netbible.com</a> copyright &copy;1996, 2019 used with permission from Biblical Studies Press, L.L.C. All rights reserved.";

    /// <summary>
    /// The canon.
    /// </summary>
    private static readonly BookHelper Canon = new ProtestantCanon();

    /// <summary>
    /// This translation.
    /// </summary>
    private static readonly Translation Translation = new Translation
    {
        Code = "NET",
        Copyright = Copyright,
        Language = "English",
        Name = "NET Bible",
        Provider = nameof(NetBible),
        Year = 2019,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="NetBible" /> class.
    /// </summary>
    /// <param name="cache">The cache.</param>
    /// <param name="runningInBrowser">If <c>true</c>, we are running in a Browser</param>
    public NetBible(IDistributedCache cache, bool runningInBrowser)
    : base(cache)
    {
        this.HttpClient.BaseAddress = new Uri("https://labs.bible.org/api", UriKind.Absolute);
        if (!runningInBrowser)
        {
            this.HttpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
            this.HttpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
        }
    }

    /// <inheritdoc/>
    public override string Id => nameof(NetBible);

    /// <inheritdoc />
    public override bool LocalOnly => true;

    /// <inheritdoc/>
    public override string Name => "NET Bible API";

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Book> GetBooksAsync(
        string translation,
        bool includeChapters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (Book book in Canon.GetBooks(includeChapters))
        {
            yield return await Task.FromResult(book);
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
            Copyright = Copyright,
            NextChapterReference = new ChapterReference(),
            PreviousChapterReference = new ChapterReference(),
            Translation = translation,
        };

        // Load the book
        string url = $"?passage={book}+{chapterNumber}&formatting=plain&type=json";
        string cacheKey = this.GetCacheKey(url);
        string? json = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

        // The NET will return the first chapter for any invalid references
        if (!Canon.IsValidChapter(book, chapterNumber))
        {
            return chapter;
        }

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
                Debug.Print($"{response.StatusCode} error in NetBible.GetChapterAsync()");
                return chapter;
            }
        }

        var data = DeserializeAnonymousType(
            json,
            EmptyListOf(
                new
                {
                    bookname = string.Empty,
                    chapter = string.Empty,
                    verse = string.Empty,
                    text = string.Empty,
                }
            )
        );
        if (data is not null && data.Count > 0)
        {
            chapter.Text = string.Join(
                Environment.NewLine,
                data.Select(d => $"{d.verse}  {d.text}")
            );

            // Clean up 3 John 15
            chapter.Text = chapter.Text.Replace("(1:15)", Environment.NewLine + "15  ");

            // Add the next and previous chapter references
            chapter.PreviousChapterReference = Canon.GetPreviousChapter(book, chapterNumber);
            chapter.NextChapterReference = Canon.GetNextChapter(book, chapterNumber);
        }

        return chapter;
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Translation> GetTranslationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        yield return await Task.FromResult(Translation);
    }
}
