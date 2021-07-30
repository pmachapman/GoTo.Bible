// -----------------------------------------------------------------------
// <copyright file="BundledTranslations.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using GoToBible.Model;

    /// <summary>
    /// The Bundled Translations Bible Provider.
    /// </summary>
    /// <seealso cref="IProvider" />
    public class BundledTranslations : IProvider
    {
        /// <summary>
        /// The SBL copyright messages.
        /// </summary>
        private static readonly Dictionary<string, string> Copyright = new Dictionary<string, string>
        {
            { "BCPPSALMS", "PUBLIC DOMAIN, except in the United Kingdom, where a Crown Copyright applies to printing the BCP. See <a href=\"http://www.cambridge.org/about-us/who-we-are/queens-printers-patent\" target=\"_blank\">http://www.cambridge.org/about-us/who-we-are/queens-printers-patent</a>" },
            { "BCPPSALMSALT", "PUBLIC DOMAIN, except in the United Kingdom, where a Crown Copyright applies to printing the BCP. See <a href=\"http://www.cambridge.org/about-us/who-we-are/queens-printers-patent\" target=\"_blank\">http://www.cambridge.org/about-us/who-we-are/queens-printers-patent</a>" },
            { "BSB", "Copyright &copy;2016, 2020 by Bible Hub. All Rights Reserved Worldwide. Free Licensing for use in Websites, Apps, Software, and Audio: <a href=\"http://berean.bible/licensing.htm\" target=\"_blank\">http://berean.bible/licensing.htm</a>" },
            { "LAOANG", "Public Domain" },
            { "LAOGRK", "Public Domain" },
            { "LAOLAT", "Public Domain" },
            { "NTA", "New Translation of the Apocrypha by <a href=\"https://goto.bible/\" target=\"_blank\">Peter Chapman</a> is licensed under <a href=\"https://creativecommons.org/licenses/by/4.0/\" target=\"_blank\">CC BY 4.0</a>" },
            { "NTANOTES", "New Translation of the Apocrypha by <a href=\"https://goto.bible/\" target=\"_blank\">Peter Chapman</a> is licensed under <a href=\"https://creativecommons.org/licenses/by/4.0/\" target=\"_blank\">CC BY 4.0</a>" },
            { "SBLGNT", "Scripture quotations marked <a href=\"http://sblgnt.com/\" target=\"_blank\">SBLGNT</a> are from the <a href=\"http://sblgnt.com/\" target=\"_blank\">SBL Greek New Testament</a>. Copyright &copy; 2010 <a href=\"http://www.sbl-site.org/\" target=\"_blank\">Society of Biblical Literature</a> and <a href=\"http://www.logos.com/\" target=\"_blank\">Logos Bible Software</a>." },
            { "SBLGNTAPP", "Scripture quotations marked <a href=\"http://sblgnt.com/\" target=\"_blank\">SBLGNT</a> are from the <a href=\"http://sblgnt.com/\" target=\"_blank\">SBL Greek New Testament</a>. Copyright &copy; 2010 <a href=\"http://www.sbl-site.org/\" target=\"_blank\">Society of Biblical Literature</a> and <a href=\"http://www.logos.com/\" target=\"_blank\">Logos Bible Software</a>." },
        };

        /// <summary>
        /// The canon.
        /// </summary>
        private static readonly Dictionary<string, BookHelper> Canon = new Dictionary<string, BookHelper>
        {
            { "BCPPSALMS", new BookHelper("Psalm", 150) },
            { "BCPPSALMSALT", new BookHelper("Psalm", 150) },
            { "BSB", new ProtestantCanon() },
            { "LAOANG", new BookHelper("Laodiceans", 1) },
            { "LAOGRK", new BookHelper("Laodiceans", 1) },
            { "LAOLAT", new BookHelper("Laodiceans", 1) },
            {
                "NTA", new BookHelper(new OrderedDictionary
                {
                    { "psalm 151", 1 },
                    { "manasseh", 1 },
                    { "laodiceans", 1 },
                })
            },
            {
                "NTANOTES", new BookHelper(new OrderedDictionary
                {
                    { "psalm 151", 1 },
                    { "manasseh", 1 },
                    { "laodiceans", 1 },
                })
            },
            { "SBLGNT", new NewTestamentCanon() },
            { "SBLGNTAPP", new NewTestamentCanon() },
        };

        private static readonly Dictionary<string, bool> SupportsItalics = new Dictionary<string, bool>
        {
            { "BCPPSALMS", false },
            { "BCPPSALMSALT", false },
            { "BSB", false },
            { "LAOANG", false },
            { "LAOGRK", false },
            { "LAOLAT", false },
            { "NTA", true },
            { "NTANOTES", true },
            { "SBLGNT", false },
            { "SBLGNTAPP", false },
        };

        /// <inheritdoc/>
        public string Id => nameof(BundledTranslations);

        /// <inheritdoc/>
        public string Name => "Bundled Translations";

        /// <inheritdoc/>
        public void Dispose() => GC.SuppressFinalize(this);

        /// <inheritdoc/>
        public async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
        {
            foreach (Book book in Canon[translation].GetBooks(includeChapters))
            {
                yield return await Task.FromResult(book);
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
                Copyright = Copyright[translation],
                NextChapterReference = new ChapterReference(),
                PreviousChapterReference = new ChapterReference(),
                SupportsItalics = SupportsItalics[translation],
                Translation = translation,
            };

            // Make sure we are in the new testament canon
            if (Canon[translation].IsValidChapter(book, chapterNumber))
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
                            string lineToAppend = line[lineStart.Length..].Replace("\t", "  ");
                            Match hasVerse = Regex.Match(lineToAppend, @"\[\d+\]");
                            if (hasVerse.Success)
                            {
                                // See SBLGNT Acts 19:40 for an example of a verse like this
                                lineToAppend = lineToAppend.Replace(hasVerse.Value, $"\r\n{hasVerse.Value}", StringComparison.OrdinalIgnoreCase);
                            }

                            // Remove verse number from Psalm inscriptions
                            if (lineToAppend.StartsWith("0  ", StringComparison.OrdinalIgnoreCase))
                            {
                                lineToAppend = lineToAppend[3..];
                            }

                            sb.AppendLine(lineToAppend);
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
                chapter.PreviousChapterReference = Canon[translation].GetPreviousChapter(book, chapterNumber);
                chapter.NextChapterReference = Canon[translation].GetNextChapter(book, chapterNumber);
                return chapter;
            }
            else
            {
                return chapter;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<Translation> GetTranslationsAsync()
        {
            yield return await Task.FromResult(new Translation
            {
                CanBeExported = true,
                Code = "BCPPSALMS",
                Copyright = Copyright["BCPPSALMS"],
                Language = "English",
                Name = "Coverdale Psalter",
                Provider = this.Id,
                Year = 1539,
            });
            yield return await Task.FromResult(new Translation
            {
                CanBeExported = true,
                Code = "BCPPSALMSALT",
                Copyright = Copyright["BCPPSALMSALT"],
                Language = "English",
                Name = "Coverdale Psalter (Alternate Versification)",
                Provider = this.Id,
                Year = 1539,
            });
            yield return await Task.FromResult(new Translation
            {
                CanBeExported = true,
                Code = "BSB",
                Copyright = Copyright["BSB"],
                Language = "English",
                Name = "Berean Study Bible",
                Provider = this.Id,
                Year = 2020,
            });
            yield return await Task.FromResult(new Translation
            {
                Author = "Joseph Barber Lightfoot; Updated by Peter Chapman",
                CanBeExported = true,
                Code = "LAOGRK",
                Copyright = Copyright["LAOGRK"],
                Language = "Greek",
                Name = "Epistle to the Laodiceans (Greek)",
                Provider = this.Id,
                Year = 2021,
            });
            yield return await Task.FromResult(new Translation
            {
                Author = "Joseph Barber Lightfoot; Updated by Peter Chapman",
                CanBeExported = true,
                Code = "LAOLAT",
                Copyright = Copyright["LAOLAT"],
                Language = "Latin",
                Name = "Epistle to the Laodiceans (Latin)",
                Provider = this.Id,
                Year = 2021,
            });
            yield return await Task.FromResult(new Translation
            {
                Author = "John Wycliffe",
                CanBeExported = true,
                Code = "LAOANG",
                Copyright = Copyright["LAOANG"],
                Language = "Old English",
                Name = "Epistle to the Laodiceans (Old English)",
                Provider = this.Id,
                Year = 2021,
            });
            yield return await Task.FromResult(new Translation
            {
                CanBeExported = true,
                Code = "NTA",
                Copyright = Copyright["NTA"],
                Language = "English",
                Name = "New Translation of the Apocrypha",
                Provider = this.Id,
                Year = 2021,
            });
            yield return await Task.FromResult(new Translation
            {
                CanBeExported = true,
                Commentary = true,
                Code = "NTANOTES",
                Copyright = Copyright["NTANOTES"],
                Language = "English",
                Name = "New Translation of the Apocrypha Notes",
                Provider = this.Id,
                Year = 2021,
            });
            yield return await Task.FromResult(new Translation
            {
                Author = "Michael W. Holmes",
                Code = "SBLGNT",
                Copyright = Copyright["SBLGNT"],
                Language = "Greek",
                Name = "SBL Greek New Testament",
                Provider = this.Id,
                Year = 2010,
            });
            yield return await Task.FromResult(new Translation
            {
                Author = "Michael W. Holmes",
                Code = "SBLGNTAPP",
                Commentary = true,
                Copyright = Copyright["SBLGNTAPP"],
                Language = "Greek",
                Name = "SBL Greek New Testament Apparatus",
                Provider = this.Id,
                Year = 2010,
            });
        }
    }
}
