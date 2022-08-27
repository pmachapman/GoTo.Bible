// -----------------------------------------------------------------------
// <copyright file="NetBible.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GoToBible.Model;
using Microsoft.Extensions.Caching.Distributed;

/// <summary>
/// The NET Bible Provider.
/// </summary>
/// <seealso cref="GoToBible.Providers.ApiProvider" />
public class NetBible : ApiProvider
{
    /// <summary>
    /// The copyright message.
    /// </summary>
    private const string Copyright = "The Scriptures quoted are from the NET Bible&reg; <a href=\"http://netbible.com\" target=\"_blank\">http://netbible.com</a> copyright &copy;1996, 2019 used with permission from Biblical Studies Press, L.L.C. All rights reserved.";

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
    public NetBible(IDistributedCache cache)
        : base(cache) => this.HttpClient.BaseAddress = new Uri("https://labs.bible.org/api/", UriKind.Absolute);

    /// <inheritdoc/>
    public override string Id => nameof(NetBible);

    /// <inheritdoc/>
    public override string Name => "NET Bible API";

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
    {
        foreach (Book book in Canon.GetBooks(includeChapters))
        {
            yield return await Task.FromResult(book);
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
            Copyright = Copyright,
            NextChapterReference = new ChapterReference(),
            PreviousChapterReference = new ChapterReference(),
            Translation = translation,
        };

        // Load the book
        string url = $"?passage={book}+{chapterNumber}&formatting=plain&type=json";
        string cacheKey = this.GetCacheKey(url);
        string json = await this.Cache.GetStringAsync(cacheKey);

        // The NET will return the first chapter for any invalid references
        if (!Canon.IsValidChapter(book, chapterNumber))
        {
            return chapter;
        }

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
                Debug.Print($"{response.StatusCode} error in NetBible.GetChapterAsync()");
                return chapter;
            }
        }

        var data = DeserializeAnonymousType(json, EmptyListOf(
            new
            {
                bookname = string.Empty,
                chapter = string.Empty,
                verse = string.Empty,
                text = string.Empty,
            }));
        if (data is not null && data.Any())
        {
            chapter.Text = string.Join(Environment.NewLine, data.Select(d => $"{d.verse}  {d.text}"));
            chapter.PreviousChapterReference = Canon.GetPreviousChapter(book, chapterNumber);
            chapter.NextChapterReference = Canon.GetNextChapter(book, chapterNumber);
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
        yield return await Task.FromResult(Translation);
    }
}
