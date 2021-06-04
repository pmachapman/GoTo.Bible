// -----------------------------------------------------------------------
// <copyright file="SblGnt.cs" company="Conglomo">
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
    /// The SBL Greek New Testament Bible Provider.
    /// </summary>
    /// <seealso cref="IProvider" />
    public class SblGnt : IProvider
    {
        /// <summary>
        /// The copyright message.
        /// </summary>
        private const string Copyright = "Scripture quotations marked <a href=\"http://sblgnt.com/\" target=\"_blank\">SBLGNT</a> are from the <a href=\"http://sblgnt.com/\" target=\"_blank\">SBL Greek New Testament</a>. Copyright &copy; 2010 <a href=\"http://www.sbl-site.org/\" target=\"_blank\">Society of Biblical Literature</a> and <a href=\"http://www.logos.com/\" target=\"_blank\">Logos Bible Software</a>.";

        /// <summary>
        /// The canon.
        /// </summary>
        private static readonly BookHelper Canon = new NewTestamentCanon();

        /// <inheritdoc/>
        public string Id => nameof(SblGnt);

        /// <inheritdoc/>
        public string Name => "SBL Greek New Testament";

        /// <inheritdoc/>
        public bool SupportsItalics { get; } = false;

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

            // Make sure we are in the new testament canon
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
                            sb.AppendLine(line[lineStart.Length..].Replace("\t", "  "));
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
                Code = "SBLGNT",
                Copyright = Copyright,
                Language = "Greek",
                Name = "SBL Greek New Testament",
                Provider = nameof(SblGnt),
                Year = 2010,
            };
            yield return new Translation
            {
                Code = "SBLGNTAPP",
                Commentary = true,
                Copyright = Copyright,
                Language = "Greek",
                Name = "SBL Greek New Testament Apparatus",
                Provider = nameof(SblGnt),
                Year = 2010,
            };
        }
    }
}
