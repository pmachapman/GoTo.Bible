// -----------------------------------------------------------------------
// <copyright file="ApiProvider.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;

/// <summary>
/// The API Provider base class.
/// </summary>
/// <seealso cref="IProvider" />
public abstract class ApiProvider : IProvider
{
    /// <summary>
    /// A list of blocked translations that cause unnecessary duplicates.
    /// </summary>
    public static readonly ReadOnlyCollection<string> BlockedTranslations = new List<string>
    {
        "BibleApi-32664dc3288a28df-02",
        "BibleApi-32664dc3288a28df-03",
        "BibleApi-685d1470fe4d5c3b-01",
        "BibleApi-6bab4d6c61b31b80-01",
        "BibleApi-7142879509583d59-02",
        "BibleApi-7142879509583d59-03",
        "BibleApi-7142879509583d59-04",
        "BibleApi-926aa5efbc5e04e2-01",
        "BibleApi-9879dbb7cfe39e4d-02",
        "BibleApi-9879dbb7cfe39e4d-03",
        "BibleApi-9879dbb7cfe39e4d-04",
        "BibleApi-bba9f40183526463-01",
        "BibleApi-de4e12af7f28f599-02",
        "BibleApi-f72b840c855f362c-04",
        "BibliaApi-asv",
        "BibliaApi-kjv",
        "BibliaApi-kjv1900",
        "BibliaApi-kjvapoc",
        "DigitalBiblePlatformApi-AAHWBTN2ET",
        "DigitalBiblePlatformApi-EN1ESV",
        "DigitalBiblePlatformApi-ENGASV",
        "DigitalBiblePlatformApi-ENGESH",
        "DigitalBiblePlatformApi-ENGKJV",
        "DigitalBiblePlatformApi-ENGNLH",
        "DigitalBiblePlatformApi-ENGNLT",
        "DigitalBiblePlatformApi-ENGREV",
        "DigitalBiblePlatformApi-ENGWEB",
    }.AsReadOnly();

    /// <summary>
    /// Name substitutions to help users of the web application.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> NameSubstitutions = new Dictionary<
        string,
        string
    >
    {
        { "The Holy Bible, American Standard Version", "American Standard Version" },
        { "English Standard Version®", "English Standard Version (2007)" },
        { "New American Standard Bible (NASB)", "New American Standard Bible (1995)" },
        { "King James (Authorised) Version (Ecumenical)", "King James Version" },
        { "TCENT", "Text-Critical English New Testament" },
    };

    /// <inheritdoc/>
    public abstract string Id { get; }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract void Dispose();

    /// <inheritdoc/>
    public abstract IAsyncEnumerable<Book> GetBooksAsync(
        string translation,
        bool includeChapters,
        CancellationToken cancellationToken = default
    );

    /// <inheritdoc/>
    public abstract Task<Chapter> GetChapterAsync(
        string translation,
        string book,
        int chapterNumber,
        CancellationToken cancellationToken = default
    );

    /// <inheritdoc/>
    public abstract IAsyncEnumerable<Translation> GetTranslationsAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the next and previous chapters for the specified chapter.
    /// </summary>
    /// <param name="chapter">The chapter to get the previous and next chapters for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected async Task GetPreviousAndNextChaptersAsync(
        Chapter chapter,
        CancellationToken cancellationToken = default
    )
    {
        // Get the next/previous chapters
        bool getNextChapter = false;
        string previousChapter = string.Empty;
        string thisChapter = $"{chapter.Book} {chapter.ChapterNumber}";
        await foreach (
            string nextChapter in this.GetChaptersAsync(chapter.Translation, cancellationToken)
        )
        {
            if (getNextChapter)
            {
                chapter.NextChapterReference = new ChapterReference(nextChapter);
                break;
            }

            if (string.Compare(nextChapter, thisChapter, StringComparison.OrdinalIgnoreCase) == 0)
            {
                chapter.PreviousChapterReference = new ChapterReference(previousChapter);
                getNextChapter = true;
                continue;
            }

            // Set the previous chapter for the next iteration (if it needs it)
            previousChapter = nextChapter;
        }
    }

    /// <summary>
    /// Gets all of the books and chapters in a translation asynchronously.
    /// </summary>
    /// <param name="translation">The translation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The list of chapters.
    /// </returns>
    private async IAsyncEnumerable<string> GetChaptersAsync(
        string translation,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await foreach (Book book in this.GetBooksAsync(translation, true, cancellationToken))
        {
            foreach (ChapterReference chapter in book.Chapters)
            {
                // Handle one chapter books
                if (chapter.ChapterNumber == 0)
                {
                    chapter.ChapterNumber = 1;
                }

                yield return chapter.ToString();
            }
        }
    }
}
