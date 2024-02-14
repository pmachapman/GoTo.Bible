// -----------------------------------------------------------------------
// <copyright file="LogosProvider.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
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
    private static readonly IReadOnlyDictionary<string, string> BookMapping = new Dictionary<
        string,
        string
    >
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
    /// The books with only one chapter.
    /// </summary>
    private static readonly string[] OneChapterBooks =
    [
        "Obadiah",
        "Letter of Jeremiah",
        "Song of Three Youths",
        "Susanna",
        "Bel and the Dragon",
        "Prayer of Manasseh",
        "Psalm 151",
        "Epistle to the Laodiceans",
        "Philemon",
        "2 John",
        "3 John",
        "Jude",
    ];

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
    /// Initializes a new instance of the <see cref="LogosProvider"/> class.
    /// </summary>
    public LogosProvider()
    {
        try
        {
            Type? type = Type.GetTypeFromProgID("LogosBibleSoftware.Launcher", true);
            if (type is not null)
            {
                this.launcher = Activator.CreateInstance(type);
            }
        }
        catch (Exception ex)
        {
            if (
                ex
                is not (
                    ArgumentException
                    or ArgumentNullException
                    or COMException
                    or InvalidComObjectException
                    or MemberAccessException
                    or MethodAccessException
                    or MissingMethodException
                    or NotSupportedException
                    or TargetInvocationException
                    or TypeLoadException
                )
            )
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
    public async IAsyncEnumerable<Book> GetBooksAsync(
        string translation,
        bool includeChapters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await Task.CompletedTask;
        yield break;
    }

    /// <inheritdoc/>
    public Task<Chapter> GetChapterAsync(
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
            Copyright = string.Empty,
            NextChapterReference = new ChapterReference(),
            PreviousChapterReference = new ChapterReference(),
            SupportsItalics = false,
            Translation = translation,
        };

        // If Logos is running
        if (this.launcher?.Application is not null)
        {
            dynamic? request = this.launcher.Application.CopyBibleVerses.CreateRequest();
            if (request is not null)
            {
                string reference = OneChapterBooks.Contains(book)
                    ? book
                    : $"{book} {chapterNumber}";
                request.Reference = this
                    .launcher.Application.DataTypes.GetDataType("bible")
                    .ParseReference(reference);

                // Get the text and fix nbsp (Alt+0160) characters
                string text = this.launcher.Application.CopyBibleVerses.GetText(request);
                text = text.Replace(" ", " ", StringComparison.OrdinalIgnoreCase);

                // Sometimes we get extra verses at the start or end. These are bugs in the Logos COM API
                StringBuilder sb = new StringBuilder();
                bool addLine = false;
                foreach (
                    string line in text.Split(
                        Environment.NewLine,
                        StringSplitOptions.RemoveEmptyEntries
                    )
                )
                {
                    addLine = addLine switch
                    {
                        false when line.StartsWith("1 ", StringComparison.OrdinalIgnoreCase)
                            => true,
                        true when line.StartsWith("1 ", StringComparison.OrdinalIgnoreCase)
                            => false,
                        _ => addLine,
                    };

                    if (addLine)
                    {
                        sb.AppendLine(line);
                    }
                }

                // If something went wrong, perhaps the starting verse was not supposed to be 1
                chapter.Text = sb.Length == 0 ? text : sb.ToString();

                dynamic referenceDetails = request.Reference.Details;
                if (referenceDetails?.NextChapter?.Details is not null)
                {
                    // This is usually an integer (e.g. 1), but could be a letter (e.g. A) or a number-letter (e.g. 1A) or a special value like Title
                    if (
                        !int.TryParse(
                            referenceDetails.NextChapter.Details.Chapter,
                            out int nextChapter
                        )
                    )
                    {
                        nextChapter = 0;
                    }

                    chapter.NextChapterReference = new ChapterReference(
                        BookMapping[referenceDetails.NextChapter.Details.Book],
                        nextChapter
                    );
                }

                if (referenceDetails?.PreviousChapter?.Details is not null)
                {
                    // This is usually an integer (e.g. 1), but could be a letter (e.g. A) or a number-letter (e.g. 1A) or a special value like Title
                    if (
                        !int.TryParse(
                            referenceDetails.PreviousChapter.Details.Chapter,
                            out int previousChapter
                        )
                    )
                    {
                        previousChapter = 0;
                    }

                    chapter.PreviousChapterReference = new ChapterReference(
                        BookMapping[referenceDetails.PreviousChapter.Details.Book],
                        previousChapter
                    );
                }
            }
        }

        return Task.FromResult(chapter);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<Translation> GetTranslationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (this.launcher?.Application is not null)
        {
            yield return await Task.FromResult(Translation);
        }
    }
}
