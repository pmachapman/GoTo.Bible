// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using GoToBible.Engine;
using GoToBible.Model;
using GoToBible.Providers;

/// <summary>
/// Extension methods.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Converts a <see cref="DataTable"/> to a CSV string.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="separator">The separator. Defaults to a comma.</param>
    /// <returns>The CSV data.</returns>
    public static string AsCsvData(this DataTable dataTable, char separator = ',')
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Join(separator, dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName)));
        foreach (DataRow row in dataTable.Rows)
        {
            IEnumerable<string> fields = row.ItemArray
                .Select(field => field?.ToString()?.EncodeCsvField(separator) ?? string.Empty);
            sb.AppendLine(string.Join(separator, fields));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts CSV data to a DataTable.
    /// </summary>
    /// <param name="csvData">The CSV data.</param>
    /// <param name="columnTypes">The suggested column types. The last type will be recurring.</param>
    /// <returns>A data table.</returns>
    /// <exception cref="ArgumentException">There was an error parsing the CSV data.</exception>
    public static DataTable AsDataTable(this string csvData, params Type[] columnTypes)
    {
        string[] lines = csvData.NormaliseLineEndings().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines[0] = lines[0].TrimEnd(',');
        string[] fields = lines[0].Split(',');
        int cols = fields.Length;
        DataTable dt = new DataTable();

        // The first row must be column names
        for (int i = 0; i < cols; i++)
        {
            dt.Columns.Add(fields[i].Trim('"'), i < columnTypes.Length ? columnTypes[i] : columnTypes.Last());
        }

        int rowCount = 0;
        try
        {
            string[] toBeContinued = Array.Empty<string>();
            bool lineToBeContinued = false;
            for (int i = 1; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    fields = lines[i].Split(',');
                    int quoteCount = string.Join(string.Empty, fields)
                        .Replace("\"\"", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Count(c => c == '"');
                    if (fields.Length < cols || lineToBeContinued || quoteCount % 2 != 0)
                    {
                        if (toBeContinued.Length > 0)
                        {
                            toBeContinued[^1] += "\n" + fields[0];
                            fields = fields.Skip(1).ToArray();
                        }

                        string[] newArray = new string[toBeContinued.Length + fields.Length];
                        Array.Copy(toBeContinued, newArray, toBeContinued.Length);
                        Array.Copy(fields, 0, newArray, toBeContinued.Length, fields.Length);
                        toBeContinued = newArray;
                        quoteCount = string.Join(string.Empty, toBeContinued)
                            .Replace("\"\"", string.Empty, StringComparison.OrdinalIgnoreCase)
                            .Count(c => c == '"');
                        if (toBeContinued.Length >= cols && quoteCount % 2 == 0)
                        {
                            fields = toBeContinued;
                            toBeContinued = Array.Empty<string>();
                            lineToBeContinued = false;
                        }
                        else
                        {
                            lineToBeContinued = true;
                            continue;
                        }
                    }

                    // Deserialize CSV following Excel's rule:
                    // 1: If there are commas in a field, quote the field.
                    // 2: Two consecutive quotes indicate a user's quote.
                    List<int> singleLeftQuota = new List<int>();
                    List<int> singleRightQuota = new List<int>();

                    // Combine fields if the number of commas match
                    if (fields.Length > cols)
                    {
                        bool lastSingleQuoteIsLeft = true;
                        for (int j = 0; j < fields.Length; j++)
                        {
                            bool leftOddQuote = false;
                            bool rightOddQuote = false;
                            if (fields[j].StartsWith("\"", StringComparison.OrdinalIgnoreCase))
                            {
                                // Start with how many double quotes
                                int numberOfConsecutiveQuotes = 0;
                                foreach (char c in fields[j])
                                {
                                    if (c == '"')
                                    {
                                        numberOfConsecutiveQuotes++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                // Start with an odd number of quotes indicates a system quote
                                if (numberOfConsecutiveQuotes % 2 == 1)
                                {
                                    leftOddQuote = true;
                                }
                            }

                            if (fields[j].EndsWith("\"", StringComparison.OrdinalIgnoreCase))
                            {
                                int numberOfConsecutiveQuotes = 0;
                                for (int k = fields[j].Length - 1; k >= 0; k--)
                                {
                                    // End with how many double quotes
                                    if (fields[j].Substring(k, 1) == "\"")
                                    {
                                        numberOfConsecutiveQuotes++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                // End with odd number of quotes indicate system quote
                                if (numberOfConsecutiveQuotes % 2 == 1)
                                {
                                    rightOddQuote = true;
                                }
                            }

                            if (leftOddQuote && !rightOddQuote)
                            {
                                singleLeftQuota.Add(j);
                                lastSingleQuoteIsLeft = true;
                            }
                            else if (!leftOddQuote && rightOddQuote)
                            {
                                singleRightQuota.Add(j);
                                lastSingleQuoteIsLeft = false;
                            }
                            else if (fields[j] == "\"")
                            {
                                // Only one quote in a field
                                if (lastSingleQuoteIsLeft)
                                {
                                    singleRightQuota.Add(j);
                                }
                                else
                                {
                                    singleLeftQuota.Add(j);
                                }
                            }
                        }

                        if (singleLeftQuota.Count == singleRightQuota.Count)
                        {
                            int insideCommas = 
                                singleLeftQuota.Select((t, indexN) => singleRightQuota[indexN] - t).Sum();

                            // Probably matched
                            if (fields.Length - cols >= insideCommas)
                            {
                                // (fields.Length - insideCommas) may be exceed the Cols
                                int validFieldsCount = insideCommas + cols;
                                string[] temp = new string[validFieldsCount];
                                int totalOffSet = 0;
                                for (int j = 0; j < validFieldsCount - totalOffSet; j++)
                                {
                                    bool combine = false;
                                    int storedIndex = 0;
                                    for (int iInLeft = 0; iInLeft < singleLeftQuota.Count; iInLeft++)
                                    {
                                        if (j + totalOffSet == singleLeftQuota[iInLeft])
                                        {
                                            combine = true;
                                            storedIndex = iInLeft;
                                            break;
                                        }
                                    }

                                    if (combine)
                                    {
                                        int offset = singleRightQuota[storedIndex] - singleLeftQuota[storedIndex];
                                        for (int combineI = 0; combineI <= offset; combineI++)
                                        {
                                            temp[j] += fields[j + totalOffSet + combineI] + ",";
                                        }

                                        temp[j] = temp[j].Remove(temp[j].Length - 1, 1);
                                        totalOffSet += offset;
                                    }
                                    else
                                    {
                                        temp[j] = fields[j + totalOffSet];
                                    }
                                }

                                fields = temp;
                            }
                        }
                    }

                    DataRow row = dt.NewRow();
                    for (int f = 0; f < cols; f++)
                    {
                        // Two consecutive quotes indicate a user's quote
                        fields[f] = fields[f].Replace("\"\"", "\"", StringComparison.OrdinalIgnoreCase);
                        if (fields[f].StartsWith("\"", StringComparison.OrdinalIgnoreCase)
                            && fields[f].EndsWith("\"", StringComparison.OrdinalIgnoreCase))
                        {
                            fields[f] = fields[f].Remove(0, 1);
                            if (fields[f].Length > 0)
                            {
                                fields[f] = fields[f].Remove(fields[f].Length - 1, 1);
                            }
                        }

                        row[f] = fields[f];
                    }

                    dt.Rows.Add(row);
                    rowCount++;
                }
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Error on row: " + (rowCount + 2) + "; " + ex.Message);
        }

        return dt;
    }

    /// <summary>
    /// Returns a <see cref="RenderFont" /> as a <see cref="Font" />.
    /// </summary>
    /// <param name="renderFont">The render font.</param>
    /// <returns>
    /// The <see cref="Font" /> corresponding to the <see cref="RenderFont" />.
    /// </returns>
    [SupportedOSPlatform("windows")]
    public static Font AsFont(this RenderFont renderFont)
        => new Font(
            renderFont.FamilyName,
            renderFont.SizeInPoints,
            (renderFont.Bold ? FontStyle.Bold : FontStyle.Regular) & (renderFont.Italic ? FontStyle.Italic : FontStyle.Regular) & (renderFont.Strikeout ? FontStyle.Strikeout : FontStyle.Regular) & (renderFont.Underline ? FontStyle.Underline : FontStyle.Regular),
            GraphicsUnit.Point);

    /// <summary>
    /// Generates an HTML apparatus from a DataTable.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="parameters">The apparatus rendering parameters.</param>
    /// <returns>The HTML code for the apparatus.</returns>
    public static string AsHtmlApparatus(this DataTable dataTable, ApparatusRenderingParameters parameters)
    {
        // Set upp the page
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine(
            "<html><head><meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />");
        sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
        sb.Append("<style>");
        sb.Append(parameters.RenderCss());
        sb.AppendLine("</style></head><body>");

        // Show the variants
        ChapterReference lastChapter = new ChapterReference();
        string lastVerse = string.Empty;
        foreach (DataRow row in dataTable.Rows)
        {
            // Separate from the previous phrase if we are in the same chapter/verse
            ChapterReference thisChapter = new ChapterReference((string)row["Book"], (int)row["Chapter"]);
            string thisVerse = (string)row["Verse"];
            if (thisVerse == lastVerse && thisChapter == lastChapter)
            {
                // We are in the same verse
                sb.Append(" | ");
            }
            else if (thisChapter.Book != lastChapter.Book)
            {
                // We are in a new book
                if (!string.IsNullOrEmpty(lastChapter.Book))
                {
                    sb.AppendLine("</p>");
                }

                sb.AppendLine($"<h1>{thisChapter.Book}</h1><p>");
            }
            else
            {
                sb.Append(' ');
            }

            if (thisChapter.ChapterNumber != lastChapter.ChapterNumber
                || thisChapter.Book != lastChapter.Book)
            {
                // We are in a new chapter number
                sb.Append("<sup>");
                if (thisChapter.Book is not ("Obadiah" or "Philemon" or "2 John" or "3 John" or "Jude"))
                {
                    // We don't show the chapter number in one chapter books
                    sb.Append($"{thisChapter.ChapterNumber}:");
                }

                sb.Append($"{thisVerse}</sup>");
            }
            else if (thisVerse != lastVerse)
            {
                // We are in a new verse
                sb.Append($"<sup>{thisVerse}</sup>");
            }

            // Show the word
            sb.Append($"<strong>{row["Phrase"]}</strong>");

            // Show the occurrence
            int occurrence = (int)row["Occurrence"];
            if (occurrence > 0)
            {
                // There is more than one occurrence in this line
                sb.Append(parameters.OccurrenceMarker.Replace("%OCCURRENCE%", occurrence.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            sb.Append(' ');

            // Get the variants by text
            Dictionary<string, string> variants = new Dictionary<string, string>();
            for (int i = 5; i < dataTable.Columns.Count; i++)
            {
                // Format based on interlinear parameters
                string? variant = row[i].ToString();
                if (!string.IsNullOrEmpty(variant))
                {
                    if (parameters.InterlinearIgnoresCase)
                    {
                        variant = variant.ToLowerInvariant();
                    }

                    if (parameters.InterlinearIgnoresDiacritics)
                    {
                        variant = variant.Normalize(NormalizationForm.FormD);
                        char[] chars = variant.Where(c =>
                            CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
                        variant = new string(chars).Normalize(NormalizationForm.FormC);
                    }

                    if (parameters.InterlinearIgnoresPunctuation)
                    {
                        variant = new string(variant.Where(c => !char.IsPunctuation(c)).ToArray());
                    }

                    // Final clean up
                    variant = variant.NormaliseWhitespace().Trim();

                    // Add the variant
                    bool variantFound = false;
                    string? key = variants.Keys.FirstOrDefault(k =>
                        string.Compare(k.Clean(), variant.Clean(), CultureInfo.InvariantCulture, parameters.AsCompareOptions()) == 0);
                    if (key is not null)
                    {
                        variants[key] += $" {dataTable.Columns[i].ColumnName}";
                        variantFound = true;
                    }

                    if (!variantFound)
                    {
                        variants.Add(variant, dataTable.Columns[i].ColumnName);
                    }
                }
            }

            // Show the variants by text
            bool firstVariant = true;
            foreach ((string variant, string texts) in variants)
            {
                if (!firstVariant)
                {
                    sb.Append("; ");
                }

                sb.Append($"{variant} {texts}");
                firstVariant = false;
            }

            lastChapter = thisChapter;
            lastVerse = thisVerse;
        }

        if (dataTable.Rows.Count > 0)
        {
            sb.Append("</p>");
        }

        // End the document
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    /// <summary>
    /// Returns a <see cref="Color" /> as a <see cref="RenderColour" />.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>
    /// The <see cref="RenderColour" /> corresponding to the <see cref="Color" />.
    /// </returns>
    public static RenderColour AsRenderColour(this Color color)
        => new RenderColour { R = color.R, G = color.G, B = color.B };

    /// <summary>
    /// Returns a <see cref="Font" /> as a <see cref="RenderFont" />.
    /// </summary>
    /// <param name="font">The font.</param>
    /// <returns>
    /// The <see cref="RenderFont" /> corresponding to the <see cref="Font" />.
    /// </returns>
    [SupportedOSPlatform("windows")]
    public static RenderFont AsRenderFont(this Font font) => new RenderFont
    {
        FamilyName = font.FontFamily.Name,
        Bold = font.Bold,
        Italic = font.Italic,
        SizeInPoints = font.SizeInPoints,
        Strikeout = font.Strikeout,
        Underline = font.Underline,
    };

    /// <summary>
    /// Removes extra whitespace, and changes all white space to a simple space.
    /// </summary>
    /// <param name="value">The value to normalise.</param>
    /// <returns>The normalised value.</returns>
    public static string NormaliseWhitespace(this string value)
    {
        StringBuilder sb = new StringBuilder();
        bool previousIsWhitespace = false;
        foreach (char character in value)
        {
            bool isWhitespace = char.IsWhiteSpace(character);
            if (previousIsWhitespace && isWhitespace)
            {
                continue;
            }
            else if (isWhitespace)
            {
                sb.Append(' ');
            }
            else
            {
                sb.Append(character);
            }

            previousIsWhitespace = isWhitespace;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the unique key for the translation.
    /// </summary>
    /// <param name="translation">The translation.</param>
    /// <returns>
    /// The unique key.
    /// </returns>
    public static string UniqueKey(this Translation translation) => $"{translation.Provider}-{translation.Code}";
}
