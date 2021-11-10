// -----------------------------------------------------------------------
// <copyright file="LogosProvider.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Threading.Tasks;
    using GoToBible.Model;

    /// <summary>
    /// The Logos Bible Software Provider.
    /// </summary>
    /// <remarks>
    /// See https://wiki.logos.com/Logos_4_COM_API and https://wiki.logos.com/How_to_Use_the_COM_API for documentation.
    /// </remarks>
    /// <seealso cref="GoToBible.Model.IProvider" />
    [SupportedOSPlatform("windows")]
    public class LogosProvider : IProvider
    {
        /// <summary>
        /// The book mappings.
        /// </summary>
        /// <remarks>
        /// See https://wiki.logos.com/COM_API_Bible_Book_Abbreviations for this list.
        /// </remarks>
        private static readonly IReadOnlyDictionary<string, string> BookMapping = new Dictionary<string, string>
        {
            ["Ge"] = "Genesis",
            ["Ex"] = "Exodus",
            ["Le"] = "Leviticus",
            ["Nu"] = "Numbers",
            ["Dt"] = "Deuteronomy",
            ["Jos"] = "Joshua",
            ["Jdg"] = "Judges",
            ["Ru"] = "Ruth",
            ["1Sa"] = "1 Samuel",
            ["2Sa"] = "2 Samuel",
            ["1Ki"] = "1 Kings",
            ["2Ki"] = "2 Kings",
            ["1Ch"] = "1 Chronicles",
            ["2Ch"] = "2 Chronicles",
            ["Ezr"] = "Ezra",
            ["Ne"] = "Nehemiah",
            ["Es"] = "Esther",
            ["Job"] = "Job",
            ["Ps"] = "Psalms",
            ["Pr"] = "Proverbs",
            ["Ec"] = "Ecclesiastes",
            ["So"] = "Song of Solomon",
            ["Is"] = "Isaiah",
            ["Je"] = "Jeremiah",
            ["La"] = "Lamentations",
            ["Eze"] = "Ezekiel",
            ["Da"] = "Daniel",
            ["Ho"] = "Hosea",
            ["Joe"] = "Joel",
            ["Am"] = "Amos",
            ["Ob"] = "Obadiah",
            ["Jon"] = "Jonah",
            ["Mic"] = "Micah",
            ["Na"] = "Nahum",
            ["Hab"] = "Habakkuk",
            ["Zep"] = "Zephaniah",
            ["Hag"] = "Haggai",
            ["Zec"] = "Zechariah",
            ["Mal"] = "Malachi",
            ["Tob"] = "Tobit",
            ["Jdt"] = "Judith",
            ["GkEs"] = "Greek Esther",
            ["Wis"] = "Wisdom of Solomon",
            ["Sir"] = "Sirach",
            ["Bar"] = "Baruch",
            ["LetJer"] = "Letter of Jeremiah",
            ["SongThr"] = "Song of Three Youths",
            ["Sus"] = "Susanna",
            ["Bel"] = "Bel and the Dragon",
            ["1Mac"] = "1 Maccabees",
            ["2Mac"] = "2 Maccabees",
            ["1Esd"] = "1 Esdras",
            ["PrMan"] = "Prayer of Manasseh",
            ["Ps151"] = "Psalm 151",
            ["3Mac"] = "3 Maccabees",
            ["2Esd"] = "2 Esdras",
            ["4Mac"] = "4 Maccabees",
            ["Ode"] = "Odes",
            ["PsSol"] = "Psalms of Solomon",
            ["Laod"] = "Epistle to the Laodiceans",
            ["Mt"] = "Matthew",
            ["Mk"] = "Mark",
            ["Lk"] = "Luke",
            ["Jn"] = "John",
            ["Ac"] = "Acts",
            ["Ro"] = "Romans",
            ["1Co"] = "1 Corinthians",
            ["2Co"] = "2 Corinthians",
            ["Ga"] = "Galatians",
            ["Eph"] = "Ephesians",
            ["Php"] = "Philippians",
            ["Col"] = "Colossians",
            ["1Th"] = "1 Thessalonians",
            ["2Th"] = "2 Thessalonians",
            ["1Ti"] = "1 Timothy",
            ["2Ti"] = "2 Timothy",
            ["Tt"] = "Titus",
            ["Phm"] = "Philemon",
            ["Heb"] = "Hebrews",
            ["Jas"] = "James",
            ["1Pe"] = "1 Peter",
            ["2Pe"] = "2 Peter",
            ["1Jn"] = "1 John",
            ["2Jn"] = "2 John",
            ["3Jn"] = "3 John",
            ["Jud"] = "Jude",
            ["Re"] = "Revelation",
            ["1Kgdms"] = "1 Kingdoms",
            ["2Kgdms"] = "2 Kingdoms",
            ["3Kgdms"] = "3 Kingdoms",
            ["4Kgdms"] = "4 Kingdoms",
            ["EsdA"] = "Esdras A",
            ["EsdB"] = "Esdras B",
        };

        /// <summary>
        /// This translation.
        /// </summary>
        private static readonly Translation Translation = new Translation
        {
            Code = "LOGOS",
            Name = "Preferred Logos Bible",
            Provider = nameof(LogosProvider),
            Year = 2009,
        };

        /// <summary>
        /// The Logos launcher.
        /// </summary>
        private readonly dynamic? launcher;

        /// <summary>
        /// Initialises a new instance of the <see cref="LogosProvider"/> class.
        /// </summary>
        public LogosProvider()
        {
            try
            {
                Type? type = Type.GetTypeFromProgID("LogosBibleSoftware.Launcher", true);
                if (type != null)
                {
                    this.launcher = Activator.CreateInstance(type);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is ArgumentException
                    or ArgumentNullException
                    or COMException
                    or InvalidComObjectException
                    or MemberAccessException
                    or MethodAccessException
                    or MissingMethodException
                    or NotSupportedException
                    or TargetInvocationException
                    or TypeLoadException))
                {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public string Id => nameof(LogosProvider);

        /// <inheritdoc/>
        public string Name => "Logos Bible Software";

        /// <inheritdoc/>
        public void Dispose() => GC.SuppressFinalize(this);

        /// <inheritdoc/>
        public async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
        {
            await Task.CompletedTask;
            yield break;
        }

        /// <inheritdoc/>
        public Task<Chapter> GetChapterAsync(string translation, string book, int chapterNumber)
        {
            // Set up the chapter
            Chapter chapter = new Chapter
            {
                Book = book,
                ChapterNumber = chapterNumber,
                Copyright = string.Empty,
                NextChapterReference = new ChapterReference(),
                PreviousChapterReference = new ChapterReference(),
                SupportsItalics = false,
                Translation = translation,
            };

            // If Logos is running
            if (this.launcher?.Application != null)
            {
                dynamic? request = this.launcher.Application.CopyBibleVerses.CreateRequest();
                if (request != null)
                {
                    request.Reference = this.launcher.Application.DataTypes.GetDataType("bible").ParseReference($"{book} {chapterNumber}");
                    chapter.Text = this.launcher.Application.CopyBibleVerses.GetText(request);
                    dynamic referenceDetails = request.Reference.Details;
                    if (referenceDetails?.NextChapter?.Details != null)
                    {
                        // This is usually an integer (e.g. 1), but could be a letter (e.g. A) or a number-letter (e.g. 1A) or a special value like Title
                        if (!int.TryParse(referenceDetails.NextChapter.Details.Chapter, out int nextChapter))
                        {
                            nextChapter = 0;
                        }

                        chapter.NextChapterReference = new ChapterReference(BookMapping[referenceDetails.NextChapter.Details.Book], nextChapter);
                    }

                    if (referenceDetails?.PreviousChapter?.Details != null)
                    {
                        // This is usually an integer (e.g. 1), but could be a letter (e.g. A) or a number-letter (e.g. 1A) or a special value like Title
                        if (!int.TryParse(referenceDetails.PreviousChapter.Details.Chapter, out int previousChapter))
                        {
                            previousChapter = 0;
                        }

                        chapter.PreviousChapterReference = new ChapterReference(BookMapping[referenceDetails.PreviousChapter.Details.Book], previousChapter);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(chapter.Text))
            {
                return Task.FromResult(chapter);
            }
            else
            {
                return Task.FromResult(chapter);
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<Translation> GetTranslationsAsync()
        {
            if (this.launcher?.Application != null)
            {
                yield return await Task.FromResult(Translation);
            }
        }
    }
}
