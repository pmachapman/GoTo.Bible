// -----------------------------------------------------------------------
// <copyright file="CoverdalePsalter.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using GoToBible.Model;

    /// <summary>
    /// The Coverdale Psalter Bible Provider.
    /// </summary>
    /// <seealso cref="IProvider" />
    public class CoverdalePsalter : IProvider
    {
        /// <summary>
        /// The copyright message.
        /// </summary>
        private const string Copyright = "PUBLIC DOMAIN, except in the United Kingdom, where a Crown Copyright applies to printing the BCP. See http://www.cambridge.org/about-us/who-we-are/queens-printers-patent";

        /// <summary>
        /// The canon.
        /// </summary>
        private static readonly BookHelper Canon = new OneBookHelper("Psalm", 150);

        /// <inheritdoc/>
        public string Id => nameof(CoverdalePsalter);

        /// <inheritdoc/>
        public string Name => "Coverdale Psalter";

        /// <inheritdoc/>
        public bool SupportsItalics => false;

        /// <inheritdoc/>
        public void Dispose() => GC.SuppressFinalize(this);

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            foreach (Book book in Canon.GetBooks(includeChapters))
            {
                yield return book;
            }
        }

        /// <inheritdoc/>
        public async Task<Chapter> GetChapterAsync(string translation, string book, int chapterNumber)
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

            // Make sure we are in the canon
            if (Canon.IsValidChapter(book, chapterNumber))
            {
                // Get the text
                await using Stream? stream = this.GetType().Assembly.GetManifestResourceStream($"GoToBible.Providers.Texts.{translation}.txt");
                if (stream != null)
                {
                    using StreamReader reader = new StreamReader(stream);
                    bool readingChapter = false;
                    string lineStart = $"{book} {chapterNumber}:";
                    string? line;
                    StringBuilder sb = new StringBuilder();
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.StartsWith(lineStart, StringComparison.OrdinalIgnoreCase))
                        {
                            // Read each verse of the chapter
                            readingChapter = true;
                            sb.AppendLine(line[lineStart.Length..]);
                        }
                        else if (readingChapter)
                        {
                            // We do not need to read any more
                            break;
                        }
                    }

                    chapter.Text = sb.ToString();
                }
            }

            if (!string.IsNullOrWhiteSpace(chapter.Text))
            {
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
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<Translation> GetTranslationsAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield return new Translation
            {
                Code = "BCPPSALMS",
                Copyright = Copyright,
                Language = "English",
                Name = "Coverdale Psalter",
                Provider = nameof(CoverdalePsalter),
                Year = 1539,
            };
            yield return new Translation
            {
                Code = "BCPPSALMSALT",
                Copyright = Copyright,
                Language = "English",
                Name = "Coverdale Psalter (Alternate Versification)",
                Provider = nameof(CoverdalePsalter),
                Year = 1539,
            };
        }
    }
}
