// -----------------------------------------------------------------------
// <copyright file="Renderer.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using GoToBible.Model;

    /// <summary>
    /// The renderer.
    /// </summary>
    public class Renderer : IDisposable
    {
        /// <summary>
        /// The regular expression to italicise words.
        /// </summary>
        private static readonly Regex ItaliciseWordsRegex = new Regex("(<em>[^<]+)([ ])([^<]+</em>)", RegexOptions.Compiled);

        /// <summary>
        /// A value indicating whether or not this instance has been disposed.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Initialises a new instance of the <see cref="Renderer"/> class.
        /// </summary>
        public Renderer()
        {
            this.Providers = new List<IProvider>();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Renderer" /> class.
        /// </summary>
        /// <param name="providers">The providers.</param>
        public Renderer(IEnumerable<IProvider> providers)
        {
            this.Providers = providers.ToList();
        }

        /// <summary>
        /// Finalises an instance of the <see cref="Renderer"/> class.
        /// </summary>
        ~Renderer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the provides.
        /// </summary>
        /// <value>
        /// The providers.
        /// </value>
        public List<IProvider> Providers { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Renders the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="renderCompleteHtmlPage">If set to <c>true</c>, render the complete HTML page.</param>
        /// <returns>
        /// The output of the rendering.
        /// </returns>
        public async Task<RenderedPassage> RenderAsync(RenderingParameters parameters, bool renderCompleteHtmlPage)
        {
            // Break up the normalised passage
            // TODO: Use the passage reference end
            string[] passageParts = parameters.PassageReference.Start.Split(':');
            string[] bookParts = passageParts.First().Split(' ');
            string book = string.Join(" ", bookParts.Take(bookParts.Length - 1));
            if (!int.TryParse(bookParts.Last(), out int chapter))
            {
                chapter = 1;
            }

            // Set up the rendered passage
            RenderedPassage renderedPassage = new RenderedPassage();

            // Get the first provider
            IProvider? firstProvider = this.Providers.FirstOrDefault(p => p.Id == parameters.PrimaryProvider);
            if (firstProvider == null)
            {
                return renderedPassage;
            }

            // Get the first translation
            Chapter firstChapter = await firstProvider.GetChapterAsync(parameters.PrimaryTranslation, book, chapter);

            // Setup the previous chapter reference
            if (firstChapter.PreviousChapterReference.IsValid)
            {
                renderedPassage.PreviousPassage = firstChapter.PreviousChapterReference.AsPassageReference();
            }
            else
            {
                renderedPassage.PreviousPassage = new PassageReference();
            }

            // Setup the next chapter reference
            if (firstChapter.NextChapterReference.IsValid)
            {
                renderedPassage.NextPassage = firstChapter.NextChapterReference.AsPassageReference();
            }
            else
            {
                renderedPassage.NextPassage = new PassageReference();
            }

            // Render appropriately
            if (parameters.Format == RenderFormat.Text)
            {
                // Strip italics
                if (this.Providers.FirstOrDefault(p => p.Id == parameters.PrimaryProvider)?.SupportsItalics ?? false)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] lines = firstChapter.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
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
                if (this.Providers.FirstOrDefault(p => p.Id == parameters.PrimaryProvider)?.SupportsItalics ?? false)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] lines = firstChapter.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        sb.AppendLine($"{book} {chapter}:" + line.RenderItalics("i").TrimStart());
                    }

                    renderedPassage.Content = sb.ToString();
                }
                else
                {
                    // We cannot do interlinear in text mode
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
                    sb.AppendLine("<!DOCTYPE html>");
                    sb.AppendLine("<html><head><meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />");
                    sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
                    sb.Append("<style>");
                    sb.Append(parameters.RenderCss());
                    sb.AppendLine("</style></head><body>");
                }

                if (!string.IsNullOrWhiteSpace(parameters.SecondaryTranslation))
                {
                    // Get the second provider
                    IProvider? secondProvider = this.Providers.FirstOrDefault(p => p.Id == (parameters.SecondaryProvider ?? parameters.PrimaryProvider));
                    if (secondProvider == null)
                    {
                        secondProvider = firstProvider;
                    }

                    // Get the second translation, if specified
                    Chapter secondChapter = await secondProvider.GetChapterAsync(parameters.SecondaryTranslation, book, chapter);

                    // If the next chapter reference is invalid, see if this chapter has a valid reference
                    if (!renderedPassage.NextPassage.IsValid && secondChapter.NextChapterReference.IsValid)
                    {
                        renderedPassage.NextPassage = secondChapter.NextChapterReference.AsPassageReference();
                    }

                    // If the previous chapter reference is invalid, see if this chapter has a valid reference
                    if (!renderedPassage.PreviousPassage.IsValid && secondChapter.PreviousChapterReference.IsValid)
                    {
                        renderedPassage.PreviousPassage = secondChapter.PreviousChapterReference.AsPassageReference();
                    }

                    // Render both interlinear
                    bool hasContent = false;
                    List<string> lines1 = firstChapter.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<string> lines2 = secondChapter.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    // Add any missing verses
                    for (int i = 0; i < lines1.Count; i++)
                    {
                        int expectedVerseNumber = i + 1;
                        if (int.TryParse(lines1[i].Substring(0, lines1[i].IndexOf(' ')), out int verseNumber) && verseNumber > expectedVerseNumber)
                        {
                            lines1.Insert(i, $"{expectedVerseNumber}  ");
                        }
                    }

                    for (int i = 0; i < lines2.Count; i++)
                    {
                        int expectedVerseNumber = i + 1;
                        if (int.TryParse(lines2[i].Substring(0, lines2[i].IndexOf(' ')), out int verseNumber) && verseNumber > expectedVerseNumber)
                        {
                            lines2.Insert(i, $"{expectedVerseNumber}  ");
                        }
                    }

                    // Render each line
                    for (int i = 0; i < lines1.Count; i++)
                    {
                        hasContent = true;
                        if (i < lines2.Count)
                        {
                            RenderedVerse firstAttempt = this.RenderInterlinearLinesAsHtml(lines1[i], lines2[i], parameters, false);
                            RenderedVerse secondAttempt = this.RenderInterlinearLinesAsHtml(lines1[i], lines2[i], parameters, true);

                            // Calculate which rendering to use, based on verse statistics
                            bool useFirstAttempt;
                            if (firstAttempt.WordsInCommon == secondAttempt.WordsInCommon)
                            {
                                // Use the number of divergent phrases, as both have the same number of words in common
                                useFirstAttempt = firstAttempt.DivergentPhrases > secondAttempt.DivergentPhrases;
                            }
                            else
                            {
                                useFirstAttempt = firstAttempt.WordsInCommon > secondAttempt.WordsInCommon;
                            }

                            if (parameters.IsDebug)
                            {
                                // Display debugging information for the renderer
                                // This does not have to be user-friendly output
                                if (i > 0)
                                {
                                    sb.Append("<hr style=\"padding:0;border:none;width:100%;height:1px;color:#000;background-color:#000\">");
                                }

                                // Build the verse statistics output
                                string verseStatistics1 = $" <span style=\"font-family:consolas,courier\">[DivergentPhrases={firstAttempt.DivergentPhrases},TotalWordsLine1={firstAttempt.TotalWordsLine1},TotalWordsLine2={firstAttempt.TotalWordsLine2},WordsInCommon={firstAttempt.WordsInCommon}]</span><br>";
                                string verseStatistics2 = $" <span style=\"font-family:consolas,courier\">[DivergentPhrases={secondAttempt.DivergentPhrases},TotalWordsLine1={secondAttempt.TotalWordsLine1},TotalWordsLine2={secondAttempt.TotalWordsLine2},WordsInCommon={secondAttempt.WordsInCommon}]</span><br>";

                                string bestNote = " title=\"This is the method selected as the best by the renderer\"";
                                if (useFirstAttempt)
                                {
                                    sb.Append($"<strong style=\"font-family:consolas,courier\"{bestNote}>Reverse Scan*</strong>{verseStatistics1} {firstAttempt.Content}");
                                    sb.Append($"<strong style=\"font-family:consolas,courier\">Forward Scan&nbsp;</strong>{verseStatistics2} {secondAttempt.Content}");
                                }
                                else
                                {
                                    sb.Append($"<strong style=\"font-family:consolas,courier\"{bestNote}>Reverse Scan&nbsp;</strong>{verseStatistics1} {firstAttempt.Content}");
                                    sb.Append($"<strong style=\"font-family:consolas,courier\"{bestNote}>Forward Scan*</strong>{verseStatistics2} {secondAttempt.Content}");
                                }
                            }
                            else if (useFirstAttempt)
                            {
                                sb.Append(firstAttempt.Content);
                            }
                            else
                            {
                                sb.Append(secondAttempt.Content);
                            }
                        }
                        else
                        {
                            sb.Append(this.RenderInterlinearLinesAsHtml(lines1[i], string.Empty, parameters, false).Content);
                        }
                    }

                    // If lines 2 was longer, add its remaining entries
                    if (lines2.Count > lines1.Count)
                    {
                        sb.AppendLine("</head><body>");
                        for (int i = lines1.Count; i < lines2.Count; i++)
                        {
                            sb.Append(this.RenderInterlinearLinesAsHtml(string.Empty, lines2[i], parameters, false).Content);
                        }
                    }

                    // Make sure we have some content
                    if (hasContent)
                    {
                        // Get all of the translations
                        List<Translation> translations = new List<Translation>();
                        await foreach (Translation translation in firstProvider.GetTranslationsAsync())
                        {
                            translations.Add(translation);
                        }

                        if (secondProvider.Id != firstProvider.Id)
                        {
                            await foreach (Translation translation in secondProvider.GetTranslationsAsync())
                            {
                                translations.Add(translation);
                            }
                        }

                        // Fix up the tooltips
                        Translation? primaryTranslation = translations.FirstOrDefault(t => t.Code == parameters.PrimaryTranslation);
                        string? primaryTranslationName = primaryTranslation?.UniqueName(translations) ?? "Primary Translation";
                        Translation? secondaryTranslation = translations.FirstOrDefault(t => t.Code == parameters.SecondaryTranslation);
                        string? secondaryTranslationName = secondaryTranslation?.UniqueName(translations) ?? "Secondary Translation";
                        sb.Replace($"<span title=\"{parameters.PrimaryTranslation}\">", $"<span title=\"{primaryTranslationName}\">");
                        sb.Replace($"<span title=\"{parameters.SecondaryTranslation}\">", $"<span title=\"{secondaryTranslationName}\">");

                        // Supplement the translation copyrights if the chapter copyrights are missing
                        if (string.IsNullOrWhiteSpace(firstChapter.Copyright) && !string.IsNullOrWhiteSpace(primaryTranslation?.Copyright))
                        {
                            firstChapter.Copyright = primaryTranslation.Copyright;
                        }

                        if (string.IsNullOrWhiteSpace(secondChapter.Copyright) && !string.IsNullOrWhiteSpace(secondaryTranslation?.Copyright))
                        {
                            secondChapter.Copyright = secondaryTranslation.Copyright;
                        }

                        // Display copyright
                        if (!string.IsNullOrWhiteSpace(firstChapter.Copyright) || !string.IsNullOrWhiteSpace(secondChapter.Copyright))
                        {
                            sb.Append($"<p class=\"copyright\">");
                            if (!string.IsNullOrWhiteSpace(firstChapter.Copyright))
                            {
                                sb.Append($"<strong>{primaryTranslationName}: </strong> {firstChapter.Copyright}");
                            }

                            if (!string.IsNullOrWhiteSpace(firstChapter.Copyright) && !string.IsNullOrWhiteSpace(secondChapter.Copyright))
                            {
                                sb.Append("<br>");
                            }

                            if (!string.IsNullOrWhiteSpace(secondChapter.Copyright))
                            {
                                sb.Append($"<strong>{secondaryTranslationName}: </strong> {secondChapter.Copyright}");
                            }

                            sb.AppendLine($"</p>");
                        }
                    }
                }
                else
                {
                    // Just render the first translation
                    foreach (string line in firstChapter.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.Append(this.RenderLineAsHtml(line, parameters));
                    }

                    // Display copyright
                    if (!string.IsNullOrWhiteSpace(firstChapter.Copyright))
                    {
                        sb.AppendLine($"<p class=\"copyright\">{firstChapter.Copyright}</p>");
                    }
                }

                if (renderCompleteHtmlPage)
                {
                    // End the document
                    sb.AppendLine("</body></html>");
                }

                renderedPassage.Content = sb.ToString();
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
        /// Renders the interlinear line as HTML.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="line1">The first line.</param>
        /// <param name="line2">The second line.</param>
        /// <param name="parameters">The rendering parameters.</param>
        /// <param name="insertAt">The index to insert at. If -1, the segments are appended.</param>
        private static void RenderInterlinearLineSegmentsAsHtml(StringBuilder sb, string line1, string line2, RenderingParameters parameters, int insertAt)
        {
            // Do not allow empty lines
            if (string.IsNullOrWhiteSpace(line1) && string.IsNullOrWhiteSpace(line2))
            {
                return;
            }
            else if (string.IsNullOrWhiteSpace(line1))
            {
                line1 = "&nbsp;";
            }
            else if (string.IsNullOrWhiteSpace(line2))
            {
                line2 = "&nbsp;";
            }

            // Render interlinear lines
            string interlinearLine = $"</span><span class=\"supsub\"><span title=\"{parameters.PrimaryTranslation}\">{line1}</span><span title=\"{parameters.SecondaryTranslation}\">{line2}</span></span><span> ";
            if (insertAt == -1)
            {
                sb.Append(interlinearLine);
            }
            else
            {
                sb.Insert(insertAt, interlinearLine);
            }
        }

        /// <summary>
        /// Renders the line as HTML.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="parameters">The rendering parameters.</param>
        /// <returns>
        /// The line as HTML.
        /// </returns>
        private string RenderLineAsHtml(string line, RenderingParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                // Clean up
                line = line.Trim();

                // Extract the verse number
                if (line.Contains(' '))
                {
                    // If we have a verse number, display it as such
                    string verseNumber = line.Substring(0, line.IndexOf(' ')).Trim();
                    if ((verseNumber.Length > 0 && char.IsDigit(verseNumber[0]))
                        || (verseNumber.Length > 1 && char.IsDigit(verseNumber[1])))
                    {
                        line = $"<sup>{verseNumber}</sup>  " + line[(line.IndexOf(' ') + 1)..].Trim();
                    }
                }

                // Render additional words in italics
                if (this.Providers.FirstOrDefault(p => p.Id == parameters.PrimaryProvider)?.SupportsItalics ?? false)
                {
                    if (parameters.RenderItalics)
                    {
                        line = line.RenderItalics();
                    }
                    else
                    {
                        line = line.StripItalics();
                    }
                }

                // Add a HTML and text new line
                return $"{line} <br>\r\n";
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
        /// <returns>
        /// The lines as HTML content, with the verse statistics calculated for the rendering (if interlinear).
        /// </returns>
        private RenderedVerse RenderInterlinearLinesAsHtml(string line1, string line2, RenderingParameters parameters, bool reverseScan)
        {
            // Declare variables
            RenderedVerse renderedVerse = new RenderedVerse();

            // See if we are rendering two lines in interlinear format
            if (!string.IsNullOrWhiteSpace(line1) && !string.IsNullOrWhiteSpace(line2))
            {
                // Render additional words in italics
                if (this.Providers.FirstOrDefault(p => p.Id == parameters.PrimaryProvider)?.SupportsItalics ?? false)
                {
                    if (parameters.RenderItalics)
                    {
                        line1 = line1.RenderItalics();

                        // Only run a maximum for the number of spaces in the line
                        for (int i = 0; i < line1.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length; i++)
                        {
                            string updatedLine1 = ItaliciseWordsRegex.Replace(line1, "$1</em>$2<em>$3");
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
                        line1 = line1.Replace("</em></em>", "</em>", StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        line1 = line1.StripItalics();
                    }
                }

                if (this.Providers.FirstOrDefault(p => p.Id == parameters.SecondaryProvider)?.SupportsItalics ?? false)
                {
                    if (parameters.RenderItalics)
                    {
                        line2 = line2.RenderItalics();

                        // Only run a maximum for the number of spaces in the line
                        for (int i = 0; i < line2.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length; i++)
                        {
                            string updatedLine2 = ItaliciseWordsRegex.Replace(line2, "$1</em>$2<em>$3");
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
                        line2 = line2.Replace("</em></em>", "</em>", StringComparison.OrdinalIgnoreCase);
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
                    string verseNumber1 = line1.Substring(0, line1.IndexOf(' ')).Trim();
                    string verseNumber2 = line2.Substring(0, line2.IndexOf(' ')).Trim();

                    // Ensure that the verse number is a number
                    if (!((verseNumber1.Length > 0 && char.IsDigit(verseNumber1[0])) || (verseNumber1.Length > 1 && char.IsDigit(verseNumber1[1]))))
                    {
                        verseNumber1 = string.Empty;
                    }

                    if (!((verseNumber2.Length > 0 && char.IsDigit(verseNumber2[0])) || (verseNumber2.Length > 1 && char.IsDigit(verseNumber2[1]))))
                    {
                        verseNumber2 = string.Empty;
                    }

                    // Prepare the lines
                    line1 = line1[(line1.IndexOf(' ') + 1)..].Trim();
                    line2 = line2[(line2.IndexOf(' ') + 1)..].Trim();

                    // Split the words
                    List<string> words1 = line1.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<string> words2 = line2.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

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

                        if (string.Compare(word1, word2, CultureInfo.InvariantCulture, parameters.AsCompareOptions()) == 0)
                        {
                            // If we are in interlinear mode, return to non-interlinear mode
                            if (interlinear)
                            {
                                // Render interlinear lines
                                RenderInterlinearLineSegmentsAsHtml(sb, interlinear1, interlinear2, parameters, reverseScan ? 0 : -1);

                                // Reset interlinear
                                interlinear = false;
                                interlinear1 = string.Empty;
                                interlinear2 = string.Empty;

                                // Record as a divergent phrase
                                renderedVerse.DivergentPhrases++;
                            }

                            // The following word is not divergent
                            renderedVerse.WordsInCommon++;
                            if (reverseScan)
                            {
                                sb.Insert(0, $"{word1} ");
                            }
                            else
                            {
                                sb.Append($"{word1} ");
                            }
                        }
                        else
                        {
                            // If word1 or word2 is empty, then we have already aligned these line segments
                            if (!string.IsNullOrWhiteSpace(word1) && !string.IsNullOrWhiteSpace(word2))
                            {
                                // Get the word matches on the rest of the line
                                Dictionary<int, int> matches = new Dictionary<int, int>();
                                for (int k = i; k < words1.Count; k++)
                                {
                                    for (int l = i; l < words2.Count; l++)
                                    {
                                        if (string.Compare(words1[k], words2[l], CultureInfo.InvariantCulture, parameters.AsCompareOptions()) == 0)
                                        {
                                            matches.Add(k, l);
                                            break;
                                        }
                                    }
                                }

                                // If there is any match in the next few words where key and value are the same, skip
                                if (matches.Any() && !matches.Any(m => m.Key == m.Value && m.Key < i + 7))
                                {
                                    KeyValuePair<int, int> closest1 = matches.OrderBy(m => m.Key).First();
                                    KeyValuePair<int, int> closest2 = matches.OrderBy(m => m.Value).First();

                                    // If there is a closest match where key and value are the same, skip
                                    if (closest1.Key != closest1.Value && closest2.Key != closest2.Value)
                                    {
                                        if (closest1.Key == closest2.Key && closest1.Value == closest2.Value)
                                        {
                                            // If they are the same, just do it
                                            if (closest1.Key < closest1.Value)
                                            {
                                                words1.InsertRange(i, Enumerable.Repeat(string.Empty, closest1.Value - closest1.Key));
                                            }
                                            else
                                            {
                                                words2.InsertRange(i, Enumerable.Repeat(string.Empty, closest1.Key - closest1.Value));
                                            }
                                        }
                                        else if (Math.Abs(closest1.Key - closest1.Value) < Math.Abs(closest2.Value - closest2.Key))
                                        {
                                            if (Math.Abs(closest1.Key - closest1.Value) == closest1.Key - closest1.Value)
                                            {
                                                if (closest1.Key < closest1.Value)
                                                {
                                                    words1.InsertRange(i, Enumerable.Repeat(string.Empty, closest1.Value - closest1.Key));
                                                }
                                                else
                                                {
                                                    words2.InsertRange(i, Enumerable.Repeat(string.Empty, closest1.Key - closest1.Value));
                                                }
                                            }
                                            else
                                            {
                                                if (closest1.Value < closest1.Key)
                                                {
                                                    words2.InsertRange(i, Enumerable.Repeat(string.Empty, closest1.Key - closest1.Value));
                                                }
                                                else
                                                {
                                                    words1.InsertRange(i, Enumerable.Repeat(string.Empty, closest1.Value - closest1.Key));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Math.Abs(closest2.Value - closest2.Key) == closest2.Value - closest2.Key)
                                            {
                                                if (closest2.Key < closest2.Value)
                                                {
                                                    words1.InsertRange(i, Enumerable.Repeat(string.Empty, closest2.Value - closest2.Key));
                                                }
                                                else
                                                {
                                                    words2.InsertRange(i, Enumerable.Repeat(string.Empty, closest2.Key - closest2.Value));
                                                }
                                            }
                                            else
                                            {
                                                if (closest2.Value < closest2.Key)
                                                {
                                                    words2.InsertRange(i, Enumerable.Repeat(string.Empty, closest2.Key - closest2.Value));
                                                }
                                                else
                                                {
                                                    words1.InsertRange(i, Enumerable.Repeat(string.Empty, closest2.Value - closest2.Key));
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
                        RenderInterlinearLineSegmentsAsHtml(sb, interlinear1, interlinear2, parameters, reverseScan ? 0 : -1);

                        // Record as a divergent phrase
                        renderedVerse.DivergentPhrases++;
                    }

                    // Finish
                    sb.Append("</span>");

                    // Add the verse number
                    if (verseNumber1 == verseNumber2)
                    {
                        sb.Insert(0, $"<sup>{verseNumber1}</sup>  <span>");
                    }
                    else
                    {
                        sb.Insert(0, $"<span class=\"supsub\"><span class=\"sup\">{verseNumber1}</span><span class=\"sup\">{verseNumber2}</span></span>  <span> ");
                    }
                }

                // Add a HTML and text new line
                renderedVerse.Content = $"{sb} <br>\r\n";
                return renderedVerse;
            }
            else if (!string.IsNullOrWhiteSpace(line1))
            {
                // Extract the verse number
                if (line1.Contains(' '))
                {
                    // If we have a verse number, display it as such
                    string verseNumber = line1.Substring(0, line1.IndexOf(' ')).Trim();
                    if ((verseNumber.Length > 0 && char.IsDigit(verseNumber[0]))
                        || (verseNumber.Length > 1 && char.IsDigit(verseNumber[1])))
                    {
                        line1 = $"<sup>{verseNumber}</sup>  <span class=\"supsub\"><span title=\"{parameters.PrimaryTranslation}\">" + line1[(line1.IndexOf(' ') + 1)..].Trim() + "</span><span>&nbsp;</span></span>";
                    }
                    else
                    {
                        line1 = $"<span class=\"supsub\"><span title=\"{parameters.PrimaryTranslation}\">" + line1.Trim() + "</span><span>&nbsp;</span></span>";
                    }
                }

                // Add a HTML and text new line
                renderedVerse.Content = $"{line1} <br>\r\n";
                return renderedVerse;
            }
            else if (!string.IsNullOrWhiteSpace(line2))
            {
                // Extract the verse number
                if (line2.Contains(' '))
                {
                    // If we have a verse number, display it as such
                    string verseNumber = line2.Substring(0, line2.IndexOf(' ')).Trim();
                    if ((verseNumber.Length > 0 && char.IsDigit(verseNumber[0]))
                        || (verseNumber.Length > 1 && char.IsDigit(verseNumber[1])))
                    {
                        line2 = $"<sup>{verseNumber}</sup>  <span class=\"supsub\"><span>&nbsp;</span><span title=\"{parameters.SecondaryTranslation}\">" + line2[(line2.IndexOf(' ') + 1)..].Trim() + "</span></span>";
                    }
                    else
                    {
                        line2 = $"<span class=\"supsub\"><span title=\"{parameters.PrimaryTranslation}\">" + line2.Trim() + "</span><span>&nbsp;</span></span>";
                    }
                }

                // Add a HTML and text new line
                renderedVerse.Content = $"{line2} <br>\r\n";
                return renderedVerse;
            }
            else
            {
                renderedVerse.Content = string.Empty;
                return renderedVerse;
            }
        }
    }
}
