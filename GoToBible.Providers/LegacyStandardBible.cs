// -----------------------------------------------------------------------
// <copyright file="LegacyStandardBible.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GoToBible.Model;
using Microsoft.Extensions.Options;

/// <summary>
/// The Legacy Standard Bible XML Provider.
/// </summary>
/// <seealso cref="ApiProvider" />
public partial class LegacyStandardBible : LocalResourceProvider
{
    /// <summary>
    /// The canon.
    /// </summary>
    private static readonly BookHelper Canon = new ProtestantCanon();

    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyStandardBible"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentException">Invalid Resource Directory - options.</exception>
    public LegacyStandardBible(IOptions<LocalResourceOptions> options) : base(options)
    {
    }

    /// <inheritdoc/>
    public override string Id => nameof(LegacyStandardBible);

    /// <inheritdoc/>
    public override string Name => "Legacy Standard Bible";

    /// <inheritdoc/>
    /// <inheritdoc />
    public override void Dispose() => GC.SuppressFinalize(this);

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
        await this.EnsureTranslationsAreCachedAsync();

        // Ensure we have translations
        if (this.Translations.Any())
        {
            // Generate the cache key
            string bookNum = Canon.GetBookNum(book).ToString().PadLeft(2, '0');
            string cacheKey = $"{{{{{bookNum}::{chapterNumber}}}}}";
            if (this.Cache.TryGetValue(cacheKey, out Chapter? cacheChapter))
            {
                return cacheChapter;
            }

            // Get the translation
            LocalTranslation? zefaniaTranslation = this.Translations.FirstOrDefault(t => t.Code == translation);
            if (zefaniaTranslation is not null)
            {
                // We use this to get the Psalm Superscription
                string previousChapter = $"{{{{{bookNum}::{chapterNumber - 1}}}}}";
                bool getSuperscription = false;

                // The next chapter codes are used to stop processing
                string[] nextChapter = { $"{{{{{bookNum}::{chapterNumber + 1}}}}}", $"{{{{{bookNum + 1}::1}}}}" };
                StringBuilder sb = new StringBuilder();
                await foreach (string line in File.ReadLinesAsync(Path.Combine(this.Options.Directory, zefaniaTranslation.Filename)))
                {
                    if (getSuperscription && line.StartsWith("<SS>", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine($"{FormatLine(line)}");
                    }
                    else if (line.Contains(cacheKey, StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine(FormatLine(line));
                        getSuperscription = false;
                    }
                    else if (line.Contains(nextChapter.First(), StringComparison.OrdinalIgnoreCase)
                             || line.Contains(nextChapter.Last(), StringComparison.OrdinalIgnoreCase))
                    {
                        // We have retrieved the last line of the chapter
                        break;
                    }
                    else if (line.Contains(previousChapter, StringComparison.OrdinalIgnoreCase))
                    {
                        // The next <SS> is this Psalm's superscription (if it exists)
                        getSuperscription = true;
                    }
                }

                Chapter chapter = new Chapter
                {
                    Book = book,
                    ChapterNumber = chapterNumber,
                    SupportsItalics = true,
                    Text = sb.ToString(),
                    Translation = translation,
                };
                await this.GetPreviousAndNextChaptersAsync(chapter);
                this.Cache.TryAdd(cacheKey, chapter);
                return chapter;
            }

        }

        // Default to an empty chapter
        return new Chapter
        {
            Book = book,
            ChapterNumber = chapterNumber,
            Translation = translation,
        };
    }

    /// <summary>
    /// The regular expression to find unused codes.
    /// </summary>
    /// <returns>The unused codes regular expression.</returns>
    [GeneratedRegex(@"<[A-Z\\/]+>", RegexOptions.Compiled)]
    private static partial Regex UnusedCodesRegex();

    /// <summary>
    /// The regular expression to find verse numbers.
    /// </summary>
    /// <returns>The verse number regular expression.</returns>
    [GeneratedRegex("^<[A-Z<>]+>{{[0-9]+::[0-9]+}}([0-9]+)<T> (<[A-Z]+>-)?", RegexOptions.Compiled)]
    private static partial Regex VerseNumberRegex();

    /// <summary>
    /// Formats a line for display.
    /// </summary>
    /// <param name="line">The line of text.</param>
    /// <returns>The line for display.</returns>
    private static string FormatLine(string line)
    {
        line = VerseNumberRegex().Replace(line, "$1  ")
            .Replace("[", "［").Replace("]", "］")
            .Replace("{", "[").Replace("}", "]")
            .Replace("--", "–")
            .Replace("+", string.Empty);
        line = UnusedCodesRegex().Replace(line, string.Empty);
        return line;
    }
}
