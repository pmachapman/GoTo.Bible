// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
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
        /// <param name="dt">The data table.</param>
        /// <param name="separator">The separator. Defaults to a comma.</param>
        /// <returns>The CSV data.</returns>
        public static string AsCsvData(this DataTable dt, char separator = ',')
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(separator, dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName)));
            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray
                    .Select(field => field?.ToString()?.EncodeCsvField(separator) ?? string.Empty);
                sb.AppendLine(string.Join(separator, fields));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts CSV data to a Datatable.
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

            int rowcount = 0;
            try
            {
                string[] toBeContinued = Array.Empty<string>();
                bool linetoBeContinued = false;
                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        fields = lines[i].Split(',');
                        int quoteCount = string.Join(string.Empty, fields).Replace("\"\"", string.Empty).Count(c => c == '"');
                        if (fields.Length < cols || linetoBeContinued || quoteCount % 2 != 0)
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
                            quoteCount = string.Join(string.Empty, toBeContinued).Replace("\"\"", string.Empty).Count(c => c == '"');
                            if (toBeContinued.Length >= cols && quoteCount % 2 == 0)
                            {
                                fields = toBeContinued;
                                toBeContinued = Array.Empty<string>();
                                linetoBeContinued = false;
                            }
                            else
                            {
                                linetoBeContinued = true;
                                continue;
                            }
                        }

                        // Deserialize CSV following Excel's rule:
                        // 1: If there are commas in a field, quote the field.
                        // 2: Two consecutive quotes indicate a user's quote.
                        List<int> singleLeftquota = new List<int>();
                        List<int> singleRightquota = new List<int>();

                        // Combine fields if the number of commas match
                        if (fields.Length > cols)
                        {
                            bool lastSingleQuoteIsLeft = true;
                            for (int j = 0; j < fields.Length; j++)
                            {
                                bool leftOddQuote = false;
                                bool rightOddQuote = false;
                                if (fields[j].StartsWith("\""))
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

                                if (fields[j].EndsWith("\""))
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
                                    singleLeftquota.Add(j);
                                    lastSingleQuoteIsLeft = true;
                                }
                                else if (!leftOddQuote && rightOddQuote)
                                {
                                    singleRightquota.Add(j);
                                    lastSingleQuoteIsLeft = false;
                                }
                                else if (fields[j] == "\"")
                                {
                                    // Only one quote in a field
                                    if (lastSingleQuoteIsLeft)
                                    {
                                        singleRightquota.Add(j);
                                    }
                                    else
                                    {
                                        singleLeftquota.Add(j);
                                    }
                                }
                            }

                            if (singleLeftquota.Count == singleRightquota.Count)
                            {
                                int insideCommas = 0;
                                for (int indexN = 0; indexN < singleLeftquota.Count; indexN++)
                                {
                                    insideCommas += singleRightquota[indexN] - singleLeftquota[indexN];
                                }

                                // Probabaly matched
                                if (fields.Length - cols >= insideCommas)
                                {
                                    // (fields.Length - insideCommas) may be exceed the Cols
                                    int validFildsCount = insideCommas + cols;
                                    string[] temp = new string[validFildsCount];
                                    int totalOffSet = 0;
                                    for (int j = 0; j < validFildsCount - totalOffSet; j++)
                                    {
                                        bool combine = false;
                                        int storedIndex = 0;
                                        for (int iInLeft = 0; iInLeft < singleLeftquota.Count; iInLeft++)
                                        {
                                            if (j + totalOffSet == singleLeftquota[iInLeft])
                                            {
                                                combine = true;
                                                storedIndex = iInLeft;
                                                break;
                                            }
                                        }

                                        if (combine)
                                        {
                                            int offset = singleRightquota[storedIndex] - singleLeftquota[storedIndex];
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
                            fields[f] = fields[f].Replace("\"\"", "\"");
                            if (fields[f].StartsWith("\""))
                            {
                                if (fields[f].EndsWith("\""))
                                {
                                    fields[f] = fields[f].Remove(0, 1);
                                    if (fields[f].Length > 0)
                                    {
                                        fields[f] = fields[f].Remove(fields[f].Length - 1, 1);
                                    }
                                }
                            }

                            row[f] = fields[f];
                        }

                        dt.Rows.Add(row);
                        rowcount++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error on row: " + (rowcount + 2) + "; " + ex.Message);
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
        /// Gets the unique key for the translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <returns>
        /// The unique key.
        /// </returns>
        public static string UniqueKey(this Translation translation) => $"{translation.Provider}-{translation.Code}";
    }
}
