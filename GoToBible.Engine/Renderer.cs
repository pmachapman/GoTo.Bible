// -----------------------------------------------------------------------
// <copyright file="Renderer.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
namespace GoToBible.Engine;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;

/// <summary>
/// The renderer.
/// </summary>
public partial class Renderer : IRenderer
{
    /// <summary>
    /// A horizontal line.
    /// </summary>
    private const string HorizontalLine =
        "<hr style=\"padding:0;border:none;width:100%;height:1px;color:#000;background-color:#000\">";

    /// <summary>
    /// A value indicating whether or not this instance has been disposed.
    /// </summary>
    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="Renderer"/> class.
    /// </summary>
    public Renderer() => this.Providers = new List<IProvider>();

    /// <summary>
    /// Initializes a new instance of the <see cref="Renderer" /> class.
    /// </summary>
    /// <param name="providers">The providers.</param>
    public Renderer(IEnumerable<IProvider> providers) => this.Providers = providers.ToList();

    /// <summary>
    /// Finalizes an instance of the <see cref="Renderer"/> class.
    /// </summary>
    /// <remarks>Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method</remarks>
    ~Renderer() => this.Dispose(false);

    /// <inheritdoc/>
    public IReadOnlyCollection<IProvider> Providers { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task<RenderedPassage> RenderAsync(
        RenderingParameters parameters,
        bool renderCompleteHtmlPage,
        CancellationToken cancellationToken = default
    )
    {
        // Set up the rendered passage
        RenderedPassage renderedPassage = new RenderedPassage();

        // Get the first provider
        IProvider? firstProvider = this.Providers.FirstOrDefault(p =>
            p.Id == parameters.PrimaryProvider
        );
        if (firstProvider is null)
        {
            return renderedPassage;
        }

        // Get the first translation
        Chapter firstChapter = await firstProvider.GetChapterAsync(
            parameters.PrimaryTranslation,
            parameters.PassageReference.ChapterReference,
            cancellationToken
        );

        // Setup the previous chapter reference
        renderedPassage.PreviousPassage = firstChapter.PreviousChapterReference.IsValid
            ? firstChapter.PreviousChapterReference.AsPassageReference()
            : new PassageReference();

        // Setup the next chapter reference
        renderedPassage.NextPassage = firstChapter.NextChapterReference.IsValid
            ? firstChapter.NextChapterReference.AsPassageReference()
            : new PassageReference();

        // If we do not have content, we will suggest another passage
        bool hasContent = false;

        // Render appropriately
        if (parameters.Format == RenderFormat.Text)
        {
            // Strip italics
            if (firstChapter.SupportsItalics)
            {
                StringBuilder sb = new StringBuilder();
                string[] lines = firstChapter.Text.Split(
                    [Environment.NewLine],
                    StringSplitOptions.RemoveEmptyEntries
                );
                foreach (string line in lines)
                {
                    hasContent = true;
                    sb.AppendLine(line.StripItalics());
                }

                renderedPassage.Content = sb.ToString();
            }
            else
            {
                // We cannot do interlinear in text mode
                renderedPassage.Content = firstChapter.Text;
            }
        }
        else if (parameters.Format == RenderFormat.Accordance)
        {
            // Strip italics
            if (firstChapter.SupportsItalics)
            {
                StringBuilder sb = new StringBuilder();
                string[] lines = firstChapter.Text.Split(
                    [Environment.NewLine],
                    StringSplitOptions.RemoveEmptyEntries
                );
                foreach (string line in lines)
                {
                    hasContent = true;
                    sb.Append(parameters.PassageReference.ChapterReference);
                    sb.Append(':');
                    sb.AppendLine(line.RenderItalics("i").TrimStart());
                }

                renderedPassage.Content = sb.ToString();
            }
            else
            {
                // We cannot do interlinear in text mode
                hasContent = !string.IsNullOrWhiteSpace(firstChapter.Text);
                renderedPassage.Content = firstChapter.Text;
            }
        }
        else
        {
            // Nuke the second translation, if it is the same
            if (parameters.PrimaryTranslation == parameters.SecondaryTranslation)
            {
                parameters.SecondaryTranslation = string.Empty;
            }

            // If we have a second translation
            StringBuilder sb = new StringBuilder();
            if (renderCompleteHtmlPage)
            {
                if (parameters.Format == RenderFormat.Spreadsheet)
                {
                    sb.AppendLine("Book,Chapter,Verse,Occurrence,Phrase,Variant");
                }
                else
                {
                    sb.AppendLine("<!DOCTYPE html>");
                    sb.AppendLine(
                        "<html><head><meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />"
                    );
                    sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
                    sb.Append("<style>");
                    sb.Append(parameters.RenderCss());
                    sb.AppendLine("</style></head><body>");
                }
            }

            if (!string.IsNullOrWhiteSpace(parameters.SecondaryTranslation))
            {
                // Get the second provider
                IProvider secondProvider =
                    this.Providers.FirstOrDefault(p =>
                        p.Id == (parameters.SecondaryProvider ?? parameters.PrimaryProvider)
                    ) ?? firstProvider;

                // Get the second translation, if specified
                Chapter secondChapter = await secondProvider.GetChapterAsync(
                    parameters.SecondaryTranslation,
                    parameters.PassageReference.ChapterReference,
                    cancellationToken
                );

                // If the next chapter reference is invalid, see if this chapter has a valid reference
                if (
                    !renderedPassage.NextPassage.IsValid
                    && secondChapter.NextChapterReference.IsValid
                )
                {
                    renderedPassage.NextPassage =
                        secondChapter.NextChapterReference.AsPassageReference();
                }

                // If the previous chapter reference is invalid, see if this chapter has a valid reference
                if (
                    !renderedPassage.PreviousPassage.IsValid
                    && secondChapter.PreviousChapterReference.IsValid
                )
                {
                    renderedPassage.PreviousPassage =
                        secondChapter.PreviousChapterReference.AsPassageReference();
                }

                // Render both interlinear
                List<string> lines1 =
                [
                    .. firstChapter.Text.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
                ];
                List<string> lines2 =
                [
                    .. secondChapter.Text.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
                ];

                // Add any missing verses
                for (int i = 0; i < lines1.Count; i++)
                {
                    int expectedVerseNumber = i + 1;
                    if (
                        int.TryParse(
                            lines1[i].AsSpan(0, lines1[i].IndexOf(' ')),
                            out int verseNumber
                        )
                        && verseNumber > expectedVerseNumber
                    )
                    {
                        lines1.Insert(i, $"{expectedVerseNumber}  ");
                    }
                }

                for (int i = 0; i < lines2.Count; i++)
                {
                    int expectedVerseNumber = i + 1;
                    if (
                        int.TryParse(
                            lines2[i].AsSpan(0, lines2[i].IndexOf(' ')),
                            out int verseNumber
                        )
                        && verseNumber > expectedVerseNumber
                    )
                    {
                        lines2.Insert(i, $"{expectedVerseNumber}  ");
                    }
                }

                // Add a superscription, if missing
                if (
                    lines1.Count > lines2.Count
                    && lines2.Count > 0
                    && int.TryParse(lines2[0].AsSpan(0, lines2[0].IndexOf(' ')), out int _)
                    && !int.TryParse(lines1[0].AsSpan(0, lines1[0].IndexOf(' ')), out int _)
                )
                {
                    lines2.Insert(0, string.Empty);
                }
                else if (
                    lines2.Count > lines1.Count
                    && lines1.Count > 0
                    && int.TryParse(lines1[0].AsSpan(0, lines1[0].IndexOf(' ')), out int _)
                    && !int.TryParse(lines2[0].AsSpan(0, lines2[0].IndexOf(' ')), out int _)
                )
                {
                    lines1.Insert(0, string.Empty);
                }

                // The following is used to calculate rendering suggestions
                int linesWithLessThanThreeWordsInCommon = 0;
                int totalInterlinearLines =
                    lines1.Count < lines2.Count ? lines1.Count : lines2.Count;

                // Render each line
                for (int i = 0; i < lines1.Count; i++)
                {
                    string content = string.Empty;
                    hasContent = true;
                    if (i < lines2.Count)
                    {
                        RenderedVerse firstAttempt = RenderInterlinearLinesAsHtml(
                            lines1[i],
                            lines2[i],
                            parameters,
                            false,
                            firstChapter.SupportsItalics
                        );
                        RenderedVerse secondAttempt = RenderInterlinearLinesAsHtml(
                            lines1[i],
                            lines2[i],
                            parameters,
                            true,
                            firstChapter.SupportsItalics
                        );

                        // If there are no words in any, skip
                        if (
                            firstAttempt
                                is
                            {
                                TotalWordsLine1: 0,
                                TotalWordsLine2: 0,
                                DivergentPhrases: 0,
                                WordsInCommon: 0
                            }
                            && secondAttempt
                                is
                            {
                                TotalWordsLine1: 0,
                                TotalWordsLine2: 0,
                                DivergentPhrases: 0,
                                WordsInCommon: 0
                            }
                        )
                        {
                            continue;
                        }

                        // Calculate which rendering to use, based on verse statistics
                        bool useFirstAttempt;
                        if (firstAttempt.WordsInCommon == secondAttempt.WordsInCommon)
                        {
                            // Use the number of divergent phrases, as both have the same number of words in common
                            useFirstAttempt =
                                firstAttempt.DivergentPhrases > secondAttempt.DivergentPhrases;
                        }
                        else
                        {
                            // When rendering the text we want more words in common
                            useFirstAttempt =
                                firstAttempt.WordsInCommon > secondAttempt.WordsInCommon;
                        }

                        if (parameters is { IsDebug: true, Format: RenderFormat.Html })
                        {
                            // Display debugging information for the renderer
                            // This does not have to be user-friendly output
                            if (i > 0)
                            {
                                sb.Append(HorizontalLine);
                            }

                            // Build the verse statistics output
                            string verseStatistics1 =
                                $" <span style=\"font-family:consolas,courier\">[DivergentPhrases={firstAttempt.DivergentPhrases},TotalWordsLine1={firstAttempt.TotalWordsLine1},TotalWordsLine2={firstAttempt.TotalWordsLine2},WordsInCommon={firstAttempt.WordsInCommon}]</span><br>";
                            string verseStatistics2 =
                                $" <span style=\"font-family:consolas,courier\">[DivergentPhrases={secondAttempt.DivergentPhrases},TotalWordsLine1={secondAttempt.TotalWordsLine1},TotalWordsLine2={secondAttempt.TotalWordsLine2},WordsInCommon={secondAttempt.WordsInCommon}]</span><br>";

                            const string bestNote =
                                " title=\"This is the method selected as the best by the renderer\"";
                            if (useFirstAttempt)
                            {
                                sb.Append(
                                    $"<strong style=\"font-family:consolas,courier\"{bestNote}>Reverse Scan*</strong>{verseStatistics1} {firstAttempt.Content}"
                                );
                                sb.Append(
                                    $"<strong style=\"font-family:consolas,courier\">Forward Scan&nbsp;</strong>{verseStatistics2} {secondAttempt.Content}"
                                );

                                // Store information for calculating suggestions
                                if (firstAttempt.WordsInCommon < 3)
                                {
                                    linesWithLessThanThreeWordsInCommon++;
                                }
                            }
                            else
                            {
                                sb.Append(
                                    $"<strong style=\"font-family:consolas,courier\"{bestNote}>Reverse Scan&nbsp;</strong>{verseStatistics1} {firstAttempt.Content}"
                                );
                                sb.Append(
                                    $"<strong style=\"font-family:consolas,courier\"{bestNote}>Forward Scan*</strong>{verseStatistics2} {secondAttempt.Content}"
                                );

                                // Store information for calculating suggestions
                                if (secondAttempt.WordsInCommon < 3)
                                {
                                    linesWithLessThanThreeWordsInCommon++;
                                }
                            }
                        }
                        else if (useFirstAttempt)
                        {
                            // Render the first attempt
                            content = firstAttempt.Content;

                            // Store information for calculating suggestions
                            if (firstAttempt.WordsInCommon < 3)
                            {
                                linesWithLessThanThreeWordsInCommon++;
                            }
                        }
                        else
                        {
                            // Render the second attempt
                            content = secondAttempt.Content;

                            // Store information for calculating suggestions
                            if (secondAttempt.WordsInCommon < 3)
                            {
                                linesWithLessThanThreeWordsInCommon++;
                            }
                        }
                    }
                    else
                    {
                        content = RenderInterlinearLinesAsHtml(
                            lines1[i],
                            string.Empty,
                            parameters,
                            false,
                            firstChapter.SupportsItalics
                        ).Content;
                    }

                    // Render the content if we are in HTML
                    if (parameters.Format == RenderFormat.Html)
                    {
                        sb.Append(content);
                    }
                    else
                    {
                        // If we are rendering an apparatus, make sure it has content
                        string textContent = content.StripHtml().Trim();

                        // If we only have a verse number, or hyphenated verse number, skip
                        if (!VerseNumberRegex().IsMatch(textContent))
                        {
                            sb.Append(content);
                        }
                    }
                }

                // If lines 2 was longer, add its remaining entries
                if (lines2.Count > lines1.Count)
                {
                    for (int i = lines1.Count; i < lines2.Count; i++)
                    {
                        hasContent = true;
                        sb.Append(
                            RenderInterlinearLinesAsHtml(
                                string.Empty,
                                lines2[i],
                                parameters,
                                false,
                                secondChapter.SupportsItalics
                            ).Content
                        );
                    }
                }

                // Make sure we have some content
                if (hasContent && parameters.Format == RenderFormat.Html)
                {
                    // Get all of the translations
                    List<Translation> translations = [];
                    await foreach (
                        Translation translation in firstProvider.GetTranslationsAsync(
                            cancellationToken
                        )
                    )
                    {
                        translations.Add(translation);
                    }

                    if (secondProvider.Id != firstProvider.Id)
                    {
                        await foreach (
                            Translation translation in secondProvider.GetTranslationsAsync(
                                cancellationToken
                            )
                        )
                        {
                            translations.Add(translation);
                        }
                    }

                    // Get the primary and secondary translations for the following calculations
                    Translation? primaryTranslation = translations.FirstOrDefault(t =>
                        t.Code == parameters.PrimaryTranslation
                    );
                    Translation? secondaryTranslation = translations.FirstOrDefault(t =>
                        t.Code == parameters.SecondaryTranslation
                    );

                    // If we are rendering as HTML or an apparatus
                    if (parameters.Format != RenderFormat.Spreadsheet)
                    {
                        // Fix up the tooltips
                        string primaryTranslationName =
                            primaryTranslation?.UniqueName(translations) ?? "Primary Translation";
                        string secondaryTranslationName =
                            secondaryTranslation?.UniqueName(translations)
                            ?? "Secondary Translation";
                        sb.Replace(
                            $"<span title=\"{parameters.PrimaryTranslation}\">",
                            $"<span title=\"{primaryTranslationName}\">"
                        );
                        sb.Replace(
                            $"<span title=\"{parameters.SecondaryTranslation}\">",
                            $"<span title=\"{secondaryTranslationName}\">"
                        );

                        // Supplement the translation copyrights if the chapter copyrights are missing
                        if (
                            string.IsNullOrWhiteSpace(firstChapter.Copyright)
                            && !string.IsNullOrWhiteSpace(primaryTranslation?.Copyright)
                        )
                        {
                            firstChapter.Copyright = primaryTranslation.Copyright;
                        }

                        if (
                            string.IsNullOrWhiteSpace(secondChapter.Copyright)
                            && !string.IsNullOrWhiteSpace(secondaryTranslation?.Copyright)
                        )
                        {
                            secondChapter.Copyright = secondaryTranslation.Copyright;
                        }

                        // Display copyright
                        if (
                            !string.IsNullOrWhiteSpace(firstChapter.Copyright)
                            || !string.IsNullOrWhiteSpace(secondChapter.Copyright)
                        )
                        {
                            sb.Append("<p class=\"copyright\">");
                            if (!string.IsNullOrWhiteSpace(firstChapter.Copyright))
                            {
                                sb.Append(
                                    $"<strong>{primaryTranslationName}: </strong> {firstChapter.Copyright}"
                                );
                            }

                            if (
                                !string.IsNullOrWhiteSpace(firstChapter.Copyright)
                                && !string.IsNullOrWhiteSpace(secondChapter.Copyright)
                            )
                            {
                                sb.Append("<br>");
                            }

                            if (!string.IsNullOrWhiteSpace(secondChapter.Copyright))
                            {
                                sb.Append(
                                    $"<strong>{secondaryTranslationName}: </strong> {secondChapter.Copyright}"
                                );
                            }

                            sb.AppendLine("</p>");
                        }
                    }

                    // Generate the rendering suggestions
                    if (
                        (
                            !parameters.InterlinearIgnoresCase
                            || !parameters.InterlinearIgnoresDiacritics
                            || !parameters.InterlinearIgnoresPunctuation
                        )
                        && linesWithLessThanThreeWordsInCommon > totalInterlinearLines / 2
                        && primaryTranslation?.Language == secondaryTranslation?.Language
                    )
                    {
                        // If at least one of "Ignore Case", "Ignore Diacritics", and "Ignore Punctuation" is false, and
                        // more than half of the rendered passage has less than three words in common, and the two
                        // translations are the same language, then suggest to ignore case, diacritics, and punctuation.
                        renderedPassage.Suggestions.IgnoreCaseDiacriticsAndPunctuation = true;
                    }

                    // Display the rendering suggestions, if we are debugging
                    if (parameters.IsDebug && parameters.Format != RenderFormat.Spreadsheet)
                    {
                        sb.Append("<p>");
                        sb.Append(HorizontalLine);
                        sb.Append("<strong>Rendering Suggestions</strong><br>");
                        sb.Append("Ignore Case, Diacritics and Punctuation: <em>");
                        sb.Append(
                            renderedPassage.Suggestions.IgnoreCaseDiacriticsAndPunctuation
                                ? "Yes"
                                : "No"
                        );
                        sb.AppendLine("</em></p>");
                    }
                }
            }
            else
            {
                // Just render the first translation
                foreach (
                    string line in firstChapter.Text.Split(
                        [Environment.NewLine],
                        StringSplitOptions.RemoveEmptyEntries
                    )
                )
                {
                    hasContent = true;
                    sb.Append(RenderLineAsHtml(line, parameters, firstChapter.SupportsItalics));
                }

                // We do not display copyright for a spreadsheet
                if (parameters.Format != RenderFormat.Spreadsheet)
                {
                    // Use the translation copyright if the chapter copyright is missing
                    if (string.IsNullOrWhiteSpace(firstChapter.Copyright))
                    {
                        await foreach (
                            Translation translation in firstProvider.GetTranslationsAsync(
                                cancellationToken
                            )
                        )
                        {
                            if (translation.Code == parameters.PrimaryTranslation)
                            {
                                firstChapter.Copyright = translation.Copyright ?? string.Empty;
                                break;
                            }
                        }
                    }

                    // Display copyright
                    if (!string.IsNullOrWhiteSpace(firstChapter.Copyright))
                    {
                        sb.AppendLine($"<p class=\"copyright\">{firstChapter.Copyright}</p>");
                    }
                }
            }

            if (renderCompleteHtmlPage && parameters.Format != RenderFormat.Spreadsheet)
            {
                // End the document
                sb.AppendLine("</body></html>");
            }

            renderedPassage.Content = sb.ToString();
        }

        // If we do not have content, make a suggestion
        if (!hasContent)
        {
            await foreach (
                Book book in firstProvider.GetBooksAsync(
                    parameters.PrimaryTranslation,
                    true,
                    cancellationToken
                )
            )
            {
                renderedPassage.Suggestions.NavigateToChapter = book.Chapters.First();
                break;
            }
        }

        // Return the rendered passage
        return renderedPassage;
    }

    /// <inheritdoc cref="IDisposable" />
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                foreach (IProvider provider in this.Providers)
                {
                    provider.Dispose();
                }
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            this.disposedValue = true;
        }
    }

    /// <summary>
    /// Formats a series of strings for CSV.
    /// </summary>
    /// <param name="values">Each of the CSV column values.</param>
    /// <returns>The CSV string, including a line break.</returns>
    private static string FormatCsv(params string[] values)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < values.Length; i++)
        {
            sb.Append(values[i].EncodeCsvField());
            if (i == values.Length - 1)
            {
                sb.AppendLine();
            }
            else
            {
                sb.Append(',');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// The regular expression to italicise words.
    /// </summary>
    /// <returns>The italicise words regular expression.</returns>
    [GeneratedRegex("(<em>[^<]+)([ ])([^<]+</em>)", RegexOptions.Compiled)]
    private static partial Regex ItaliciseWordsRegex();

    /// <summary>
    /// Renders the interlinear line as HTML.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="line1">The first line.</param>
    /// <param name="line2">The second line.</param>
    /// <param name="parameters">The rendering parameters.</param>
    /// <param name="insertAt">The index to insert at. If -1, the segments are appended.</param>
    /// <param name="baseLine">The line which <c>param1</c> sits within.Used for apparatus calculations.</param>
    /// <param name="verseNumber">The verse number. This should be the verse number of the base line.</param>
    /// <param name="approximatePosition">The approximate position of this phrase. Use for occurrence number calculations.</param>
    private static void RenderInterlinearLineSegmentsAsHtml(
        StringBuilder sb,
        string line1,
        string line2,
        RenderingParameters parameters,
        int insertAt,
        string baseLine,
        string verseNumber,
        int approximatePosition
    )
    {
        // Do not allow empty lines
        if (string.IsNullOrWhiteSpace(line1) && string.IsNullOrWhiteSpace(line2))
        {
            return;
        }
        else if (string.IsNullOrWhiteSpace(line1) && parameters.Format != RenderFormat.Spreadsheet)
        {
            line1 = "&nbsp;";
        }
        else if (string.IsNullOrWhiteSpace(line2))
        {
            if (parameters is SpreadsheetRenderingParameters spreadsheetRenderingParameters)
            {
                line2 = spreadsheetRenderingParameters.OmissionMarker;
            }
            else if (parameters.Format != RenderFormat.Spreadsheet)
            {
                line2 = "&nbsp;";
            }
        }

        string lineToRender;
        if (parameters.Format == RenderFormat.Html)
        {
            // Render interlinear lines
            lineToRender =
                $"</span><span class=\"supsub\"><span title=\"{parameters.PrimaryTranslation}\">{line1}</span><span title=\"{parameters.SecondaryTranslation}\">{line2}</span></span><span> ";
        }
        else
        {
            // Render as an apparatus or a spreadsheet

            // See if the phrase occurs more than once
            int occurrence = 0;
            if (baseLine.CountOccurrences(line1, StringComparison.OrdinalIgnoreCase) > 1)
            {
                occurrence = baseLine.GetOccurrence(
                    line1,
                    approximatePosition,
                    StringComparison.OrdinalIgnoreCase
                );
            }

            // Clean up the line
            line1 = line1.Trim();

            // Render as spreadsheet
            if (parameters.Format == RenderFormat.Spreadsheet)
            {
                // Book,Chapter,Verse,Occurrence,Phrase,Variant
                lineToRender = FormatCsv(
                    parameters.PassageReference.ChapterReference.Book,
                    parameters.PassageReference.ChapterReference.ChapterNumber.ToString(),
                    verseNumber,
                    occurrence.ToString(),
                    line1,
                    line2.Trim()
                );
            }
            else if (line2.Contains("%OMITTED_PHRASE%", StringComparison.OrdinalIgnoreCase))
            {
                // Allow substitutions for omitted phrases
                lineToRender =
                    line2.Replace("%OMITTED_PHRASE%", line1, StringComparison.OrdinalIgnoreCase)
                    + " | ";
            }
            else if (occurrence > 0)
            {
                // There is more than one occurrence in this line
                string occurrenceMarker = string.Empty;
                if (parameters is ApparatusRenderingParameters apparatusParameters)
                {
                    occurrenceMarker = apparatusParameters.OccurrenceMarker.Replace(
                        "%OCCURRENCE%",
                        occurrence.ToString(),
                        StringComparison.OrdinalIgnoreCase
                    );
                }

                lineToRender = $"<strong>{line1}</strong>{occurrenceMarker} {line2} | ";
            }
            else
            {
                // There is only one occurrence in this line
                lineToRender = $"<strong>{line1}</strong> {line2} | ";
            }
        }

        // Render the line
        if (insertAt == -1)
        {
            sb.Append(lineToRender);
        }
        else
        {
            sb.Insert(insertAt, lineToRender);
        }
    }

    /// <summary>
    /// Renders the line as HTML.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <param name="parameters">The rendering parameters.</param>
    /// <param name="supportsItalics">If set to <c>true</c>, we are to support italics.</param>
    /// <returns>
    /// The line as HTML.
    /// </returns>
    private static string RenderLineAsHtml(
        string line,
        RenderingParameters parameters,
        bool supportsItalics
    )
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            // Clean up
            line = line.Trim();

            // Extract the verse number
            if (line.Contains(' '))
            {
                // If we have a verse number, display it as such
                string verseNumber = line.GetVerseNumber();
                if (verseNumber.IsValidVerseNumber())
                {
                    line = verseNumber.MatchesHighlightedVerses(
                        parameters.PassageReference.HighlightedVerses
                    )
                        ? $"<sup id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber}\">{verseNumber}</sup>  <mark>"
                          + line[(line.IndexOf(' ') + 1)..].Trim()
                          + "</mark>"
                        : $"<sup id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber}\">{verseNumber}</sup>  "
                          + line[(line.IndexOf(' ') + 1)..].Trim();
                }
            }
            else if (int.TryParse(line, out int _))
            {
                // There is only a verse number, so do not render
                return string.Empty;
            }

            // Render additional words in italics
            if (supportsItalics)
            {
                line = parameters.RenderItalics ? line.RenderItalics() : line.StripItalics();
            }

            // Add a HTML and text new line
            return parameters.Format == RenderFormat.Spreadsheet
                ? line
                : $"{line} <br>{Environment.NewLine}";
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Renders the interlinear lines as HTML.
    /// </summary>
    /// <param name="line1">The first line.</param>
    /// <param name="line2">The second line.</param>
    /// <param name="parameters">The rendering parameters.</param>
    /// <param name="reverseScan">If set to <c>true</c>, scan in reverse order.</param>
    /// <param name="supportsItalics">If set to <c>true</c>, we are to support italics.</param>
    /// <returns>
    /// The lines as HTML content, with the verse statistics calculated for the rendering (if interlinear).
    /// </returns>
    private static RenderedVerse RenderInterlinearLinesAsHtml(
        string line1,
        string line2,
        RenderingParameters parameters,
        bool reverseScan,
        bool supportsItalics
    )
    {
        // Declare variables
        RenderedVerse renderedVerse = new RenderedVerse();

        // See if we are rendering two lines in interlinear format
        if (!string.IsNullOrWhiteSpace(line1) && !string.IsNullOrWhiteSpace(line2))
        {
            // Render additional words in italics
            if (supportsItalics)
            {
                if (parameters.RenderItalics && parameters.Format != RenderFormat.Spreadsheet)
                {
                    line1 = line1.RenderItalics();

                    // Only run a maximum for the number of spaces in the line
                    for (
                        int i = 0;
                        i
                            < line1
                                .Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)
                                .Length;
                        i++
                    )
                    {
                        string updatedLine1 = ItaliciseWordsRegex()
                            .Replace(line1, "$1</em>$2<em>$3");
                        if (updatedLine1 == line1)
                        {
                            break;
                        }
                        else
                        {
                            line1 = updatedLine1;
                        }
                    }

                    line1 = line1.Replace("<em><em>", "<em>", StringComparison.OrdinalIgnoreCase);
                    line1 = line1.Replace(
                        "</em></em>",
                        "</em>",
                        StringComparison.OrdinalIgnoreCase
                    );
                }
                else
                {
                    line1 = line1.StripItalics();
                }
            }

            if (supportsItalics)
            {
                if (parameters.RenderItalics && parameters.Format != RenderFormat.Spreadsheet)
                {
                    line2 = line2.RenderItalics();

                    // Only run a maximum for the number of spaces in the line
                    for (
                        int i = 0;
                        i
                            < line2
                                .Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)
                                .Length;
                        i++
                    )
                    {
                        string updatedLine2 = ItaliciseWordsRegex()
                            .Replace(line2, "$1</em>$2<em>$3");
                        if (updatedLine2 == line2)
                        {
                            break;
                        }
                        else
                        {
                            line2 = updatedLine2;
                        }
                    }

                    line2 = line2.Replace("<em><em>", "<em>", StringComparison.OrdinalIgnoreCase);
                    line2 = line2.Replace(
                        "</em></em>",
                        "</em>",
                        StringComparison.OrdinalIgnoreCase
                    );
                }
                else
                {
                    line2 = line2.StripItalics();
                }
            }

            // Set up
            StringBuilder sb = new StringBuilder();

            // Extract the verse number
            if (line1.Contains(' ') && line2.Contains(' '))
            {
                // Get the verse number
                string verseNumber1 = line1.GetVerseNumber();
                string verseNumber2 = line2.GetVerseNumber();

                // Ensure that the verse numbers are valid verse numbers
                if (!verseNumber1.IsValidVerseNumber())
                {
                    verseNumber1 = string.Empty;
                }

                if (!verseNumber2.IsValidVerseNumber())
                {
                    verseNumber2 = string.Empty;
                }

                // Prepare the lines
                if (!string.IsNullOrWhiteSpace(verseNumber1))
                {
                    line1 = line1[(line1.IndexOf(' ') + 1)..].Trim();
                }

                if (!string.IsNullOrWhiteSpace(verseNumber2))
                {
                    line2 = line2[(line2.IndexOf(' ') + 1)..].Trim();
                }

                // Split the words
                List<string> words1 =
                [
                    .. line1.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)
                ];
                List<string> words2 =
                [
                    .. line2.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)
                ];

                // Get the word counts
                renderedVerse.TotalWordsLine1 = words1.Count;
                renderedVerse.TotalWordsLine2 = words2.Count;

                // Perform a reverse scan for matches
                if (reverseScan)
                {
                    words1.Reverse();
                    words2.Reverse();
                }

                bool interlinear = false;
                string interlinear1 = string.Empty;
                string interlinear2 = string.Empty;
                string lastWordInCommon = string.Empty;
                int approximatePosition = 0;
                for (int i = 0; i < Math.Max(words1.Count, words2.Count); i++)
                {
                    string word1 = string.Empty;
                    if (i < words1.Count)
                    {
                        word1 = words1[i];
                    }

                    string word2 = string.Empty;
                    if (i < words2.Count)
                    {
                        word2 = words2[i];
                    }

                    if (
                        string.Compare(
                            word1.Clean(),
                            word2.Clean(),
                            CultureInfo.InvariantCulture,
                            parameters.AsCompareOptions()
                        ) == 0
                    )
                    {
                        // If we are in interlinear mode, return to non-interlinear mode
                        if (interlinear)
                        {
                            // If we are to render the neighbouring word in the apparatus for additions
                            bool skipThisWord = false;
                            if (
                                parameters
                                    is SpreadsheetRenderingParameters
                                {
                                    RenderNeighbourForAddition: true
                                }
                                && string.IsNullOrWhiteSpace(interlinear1)
                            )
                            {
                                // Add the appropriate neighbour
                                skipThisWord = true;
                                interlinear1 = word1;
                                interlinear2 = reverseScan
                                    ? $"{word2} {interlinear2}"
                                    : $"{interlinear2} {word2}";
                            }

                            // Render interlinear lines
                            RenderInterlinearLineSegmentsAsHtml(
                                sb,
                                interlinear1,
                                interlinear2,
                                parameters,
                                reverseScan ? 0 : -1,
                                line1,
                                verseNumber1,
                                approximatePosition
                            );

                            // Reset interlinear
                            interlinear = false;
                            interlinear1 = string.Empty;
                            interlinear2 = string.Empty;

                            // Record as a divergent phrase
                            renderedVerse.DivergentPhrases++;

                            // Skip this word, if it was added to the interlinear segments
                            if (skipThisWord)
                            {
                                continue;
                            }
                        }

                        // Remember the approximate position of end of this word,
                        // so that we can roughly know where the divergent phrase starts
                        if (i < words1.Count)
                        {
                            int index = 0;
                            int count = i + 1;
                            if (reverseScan)
                            {
                                // Exclude the current word in our getting the words from the reversed list
                                index = count;
                                count = words1.Count - count - 1;
                            }

                            approximatePosition =
                                count < 1
                                    ? 0
                                    : words1.GetRange(index, count).Sum(w => w.Length + 1);
                        }

                        // Remember the last word in common for the RenderNeighbourForAddition setting
                        // when generating an apparatus. This may be used if the last phrase of a pair
                        // of lines is divergent.
                        lastWordInCommon = word1;

                        // The following word is not divergent
                        renderedVerse.WordsInCommon++;

                        // If we are rendering as HTML, render this word
                        if (parameters.Format == RenderFormat.Html)
                        {
                            if (reverseScan)
                            {
                                sb.Insert(0, $"{word1} ");
                            }
                            else
                            {
                                sb.Append($"{word1} ");
                            }
                        }
                    }
                    else
                    {
                        // If word1 or word2 is empty, then we have already aligned these line segments
                        if (!string.IsNullOrWhiteSpace(word1) && !string.IsNullOrWhiteSpace(word2))
                        {
                            // Get the word matches on the rest of the line
                            Dictionary<int, int> matches = [];
                            for (int k = i; k < words1.Count; k++)
                            {
                                for (int l = i; l < words2.Count; l++)
                                {
                                    if (
                                        string.Compare(
                                            words1[k].Clean(),
                                            words2[l].Clean(),
                                            CultureInfo.InvariantCulture,
                                            parameters.AsCompareOptions()
                                        ) == 0
                                    )
                                    {
                                        matches.Add(k, l);
                                        break;
                                    }
                                }
                            }

                            // If there is any match in the next few words where key and value are the same, skip
                            if (
                                matches.Count > 0
                                && !matches.Any(m => m.Key == m.Value && m.Key < i + 7)
                            )
                            {
                                KeyValuePair<int, int> closest1 = matches.MinBy(m => m.Key);
                                KeyValuePair<int, int> closest2 = matches.MinBy(m => m.Value);

                                // If there is a closest match where key and value are the same, skip
                                if (
                                    closest1.Key != closest1.Value
                                    && closest2.Key != closest2.Value
                                )
                                {
                                    if (
                                        closest1.Key == closest2.Key
                                        && closest1.Value == closest2.Value
                                    )
                                    {
                                        // If they are the same, just do it
                                        if (closest1.Key < closest1.Value)
                                        {
                                            words1.InsertRange(
                                                i,
                                                Enumerable.Repeat(
                                                    string.Empty,
                                                    closest1.Value - closest1.Key
                                                )
                                            );
                                        }
                                        else
                                        {
                                            words2.InsertRange(
                                                i,
                                                Enumerable.Repeat(
                                                    string.Empty,
                                                    closest1.Key - closest1.Value
                                                )
                                            );
                                        }
                                    }
                                    else if (
                                        Math.Abs(closest1.Key - closest1.Value)
                                        < Math.Abs(closest2.Value - closest2.Key)
                                    )
                                    {
                                        if (
                                            Math.Abs(closest1.Key - closest1.Value)
                                            == closest1.Key - closest1.Value
                                        )
                                        {
                                            if (closest1.Key < closest1.Value)
                                            {
                                                words1.InsertRange(
                                                    i,
                                                    Enumerable.Repeat(
                                                        string.Empty,
                                                        closest1.Value - closest1.Key
                                                    )
                                                );
                                            }
                                            else
                                            {
                                                words2.InsertRange(
                                                    i,
                                                    Enumerable.Repeat(
                                                        string.Empty,
                                                        closest1.Key - closest1.Value
                                                    )
                                                );
                                            }
                                        }
                                        else
                                        {
                                            if (closest1.Value < closest1.Key)
                                            {
                                                words2.InsertRange(
                                                    i,
                                                    Enumerable.Repeat(
                                                        string.Empty,
                                                        closest1.Key - closest1.Value
                                                    )
                                                );
                                            }
                                            else
                                            {
                                                words1.InsertRange(
                                                    i,
                                                    Enumerable.Repeat(
                                                        string.Empty,
                                                        closest1.Value - closest1.Key
                                                    )
                                                );
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (
                                            Math.Abs(closest2.Value - closest2.Key)
                                            == closest2.Value - closest2.Key
                                        )
                                        {
                                            if (closest2.Key < closest2.Value)
                                            {
                                                words1.InsertRange(
                                                    i,
                                                    Enumerable.Repeat(
                                                        string.Empty,
                                                        closest2.Value - closest2.Key
                                                    )
                                                );
                                            }
                                            else
                                            {
                                                words2.InsertRange(
                                                    i,
                                                    Enumerable.Repeat(
                                                        string.Empty,
                                                        closest2.Key - closest2.Value
                                                    )
                                                );
                                            }
                                        }
                                        else
                                        {
                                            if (closest2.Value < closest2.Key)
                                            {
                                                words2.InsertRange(
                                                    i,
                                                    Enumerable.Repeat(
                                                        string.Empty,
                                                        closest2.Key - closest2.Value
                                                    )
                                                );
                                            }
                                            else
                                            {
                                                words1.InsertRange(
                                                    i,
                                                    Enumerable.Repeat(
                                                        string.Empty,
                                                        closest2.Value - closest2.Key
                                                    )
                                                );
                                            }
                                        }
                                    }

                                    i--;
                                    continue;
                                }
                            }
                        }

                        interlinear = true;
                        if (!string.IsNullOrWhiteSpace(word1))
                        {
                            if (reverseScan)
                            {
                                interlinear1 = $"{word1} {interlinear1}";
                            }
                            else
                            {
                                interlinear1 += $"{word1} ";
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(word2))
                        {
                            if (reverseScan)
                            {
                                interlinear2 = $"{word2} {interlinear2}";
                            }
                            else
                            {
                                interlinear2 += $"{word2} ";
                            }
                        }
                    }
                }

                // Render remaining interlinear lines
                if (interlinear)
                {
                    // If we are to render the neighbouring word in the apparatus for additions
                    if (
                        parameters
                            is SpreadsheetRenderingParameters { RenderNeighbourForAddition: true }
                        && string.IsNullOrWhiteSpace(interlinear1)
                    )
                    {
                        // Add the appropriate neighbour
                        // Note how the order for interlinear2 varies from the block this same operation is performed above.
                        // That is because there we are working with the word after the interlinear portion, here as we are at
                        // end of the line, we are dealing with the word before the interlinear portion.
                        interlinear1 = lastWordInCommon;
                        interlinear2 = reverseScan
                            ? $"{interlinear2} {lastWordInCommon}"
                            : $"{lastWordInCommon} {interlinear2}";
                    }

                    RenderInterlinearLineSegmentsAsHtml(
                        sb,
                        interlinear1,
                        interlinear2,
                        parameters,
                        reverseScan ? 0 : -1,
                        line1,
                        verseNumber1,
                        approximatePosition
                    );

                    // Record as a divergent phrase
                    renderedVerse.DivergentPhrases++;
                }

                // Finish HTML and Apparatus content
                if (parameters.Format != RenderFormat.Spreadsheet)
                {
                    sb.Append("</span>");

                    // Add any highlighting
                    if (
                        verseNumber1.MatchesHighlightedVerses(
                            parameters.PassageReference.HighlightedVerses
                        )
                        || verseNumber2.MatchesHighlightedVerses(
                            parameters.PassageReference.HighlightedVerses
                        )
                    )
                    {
                        // Add the verse number
                        if (verseNumber1 == verseNumber2)
                        {
                            sb.Insert(
                                0,
                                $"<sup id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber1}\">{verseNumber1}</sup>  <mark><span>"
                            );
                        }
                        else
                        {
                            sb.Insert(
                                0,
                                $"<span class=\"supsub\" id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber1}\"><span class=\"sup\">{verseNumber1}</span><span class=\"sup\">{verseNumber2}</span></span>  <mark><span> "
                            );
                        }

                        sb.Append("</mark>");
                    }
                    else
                    {
                        // Add the verse number without highlighting
                        if (verseNumber1 == verseNumber2)
                        {
                            sb.Insert(
                                0,
                                $"<sup id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber1}\">{verseNumber1}</sup>  <span>"
                            );
                        }
                        else
                        {
                            sb.Insert(
                                0,
                                $"<span class=\"supsub\" id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber1}\"><span class=\"sup\">{verseNumber1}</span><span class=\"sup\">{verseNumber2}</span></span>  <span> "
                            );
                        }
                    }
                }
            }

            if (parameters.Format == RenderFormat.Html)
            {
                // Add a HTML and text new line
                renderedVerse.Content = $"{sb} <br>{Environment.NewLine}";
            }
            else if (parameters.Format == RenderFormat.Spreadsheet)
            {
                // We do not need an extra line break for a spreadsheet
                renderedVerse.Content = sb.ToString();
            }
            else
            {
                // Cleanup the end of the apparatus
                if (parameters.Format == RenderFormat.Apparatus && sb[^9] == '|')
                {
                    // Remove "| " from the end (" | </span>") by replacing with a space
                    // as HTML doesn't mind multiple spaces
                    sb[^9] = ' ';
                }

                // Render an apparatus plainly
                renderedVerse.Content = $"{sb}{Environment.NewLine}";
            }
        }
        else if (!string.IsNullOrWhiteSpace(line1))
        {
            // Extract the verse number
            if (line1.Contains(' '))
            {
                // If we have a verse number, display it as such
                string verseNumber = line1.GetVerseNumber();
                if (verseNumber.IsValidVerseNumber())
                {
                    if (parameters.Format == RenderFormat.Spreadsheet)
                    {
                        // Just in case parameters is not a SpreadsheetRenderingParameters
                        string omissionMarker = SpreadsheetRenderingParameters.Omit;
                        if (
                            parameters
                            is SpreadsheetRenderingParameters spreadsheetRenderingParameters
                        )
                        {
                            omissionMarker = spreadsheetRenderingParameters.OmissionMarker;
                        }

                        // Book,Chapter,Verse,Occurrence,Phrase,Variant
                        line1 = FormatCsv(
                            parameters.PassageReference.ChapterReference.Book,
                            parameters.PassageReference.ChapterReference.ChapterNumber.ToString(),
                            verseNumber,
                            "0",
                            line2[(line2.IndexOf(' ') + 1)..].Trim(),
                            omissionMarker
                        );
                    }
                    else if (
                        verseNumber.MatchesHighlightedVerses(
                            parameters.PassageReference.HighlightedVerses
                        )
                    )
                    {
                        line1 =
                            $"<sup id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber}\">{verseNumber}</sup>  <span class=\"supsub\"><span title=\"{parameters.PrimaryTranslation}\"><mark>"
                            + line1[(line1.IndexOf(' ') + 1)..].Trim()
                            + "</mark></span><span>&nbsp;</span></span>";
                    }
                    else
                    {
                        line1 =
                            $"<sup id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber}\">{verseNumber}</sup>  <span class=\"supsub\"><span title=\"{parameters.PrimaryTranslation}\">"
                            + line1[(line1.IndexOf(' ') + 1)..].Trim()
                            + "</span><span>&nbsp;</span></span>";
                    }
                }
                else if (parameters.Format != RenderFormat.Spreadsheet)
                {
                    line1 =
                        $"<span class=\"supsub\"><span title=\"{parameters.PrimaryTranslation}\">"
                        + line1.Trim()
                        + "</span><span>&nbsp;</span></span>";
                }
            }

            // Set the statistics
            renderedVerse.DivergentPhrases = 1;

            // Add an HTML and text new line if not a spreadsheet
            renderedVerse.Content =
                parameters.Format == RenderFormat.Spreadsheet
                    ? line1
                    : $"{line1} <br>{Environment.NewLine}";
        }
        else if (!string.IsNullOrWhiteSpace(line2))
        {
            // Extract the verse number
            if (line2.Contains(' '))
            {
                // If we have a verse number, display it as such
                string verseNumber = line2.GetVerseNumber();
                if (verseNumber.IsValidVerseNumber())
                {
                    if (parameters.Format == RenderFormat.Spreadsheet)
                    {
                        // Book,Chapter,Verse,Occurrence,Phrase,Variant
                        line2 = FormatCsv(
                            parameters.PassageReference.ChapterReference.Book,
                            parameters.PassageReference.ChapterReference.ChapterNumber.ToString(),
                            verseNumber,
                            "0",
                            string.Empty,
                            line2[(line2.IndexOf(' ') + 1)..].Trim()
                        );
                    }
                    else if (
                        verseNumber.MatchesHighlightedVerses(
                            parameters.PassageReference.HighlightedVerses
                        )
                    )
                    {
                        line2 =
                            $"<sup id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber}\">{verseNumber}</sup>  <span class=\"supsub\"><span>&nbsp;</span><span title=\"{parameters.SecondaryTranslation}\"><mark>"
                            + line2[(line2.IndexOf(' ') + 1)..].Trim()
                            + "</mark></span></span>";
                    }
                    else
                    {
                        line2 =
                            $"<sup id=\"{parameters.PassageReference.ChapterReference.ToString().EncodePassageForUrl()}_{verseNumber}\">{verseNumber}</sup>  <span class=\"supsub\"><span>&nbsp;</span><span title=\"{parameters.SecondaryTranslation}\">"
                            + line2[(line2.IndexOf(' ') + 1)..].Trim()
                            + "</span></span>";
                    }
                }
                else if (parameters.Format != RenderFormat.Spreadsheet)
                {
                    line2 =
                        $"<span class=\"supsub\"><span title=\"{parameters.PrimaryTranslation}\">"
                        + line2.Trim()
                        + "</span><span>&nbsp;</span></span>";
                }
            }

            // Set the statistics
            renderedVerse.DivergentPhrases = 1;

            // Add an HTML and text new line if not a spreadsheet
            renderedVerse.Content =
                parameters.Format == RenderFormat.Spreadsheet
                    ? line2
                    : $"{line2} <br>{Environment.NewLine}";
        }
        else
        {
            renderedVerse.Content = string.Empty;
        }

        return renderedVerse;
    }

    /// <summary>
    /// The regular expression to find verse numbers.
    /// </summary>
    /// <returns>The verse number regular expression.</returns>
    [GeneratedRegex("^[\\-0-9]+$", RegexOptions.Compiled)]
    private static partial Regex VerseNumberRegex();
}
