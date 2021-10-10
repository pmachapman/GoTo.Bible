// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using GoToBible.Model;

    /// <summary>
    /// General Extension Methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// The valid verse number regular expression.
        /// </summary>
        /// <remarks>
        /// This includes support for verses with letters, hypenated verses, and verses enclosed in brackets.
        /// </remarks>
        private static readonly Regex VerseNumberRegex = new Regex(@"([\[]((([0-9]+[a-z]?)-([0-9]+[a-z]?))|([0-9]+[a-z]?))[\]])|((([0-9]+[a-z]?)-([0-9]+[a-z]?))|([0-9]+[a-z]?))", RegexOptions.Compiled);

        /// <summary>
        /// Returns the <see cref="CompareOptions"/> matching the <see cref="RenderingParameters"/>.
        /// </summary>
        /// <param name="parameters">The rendering parameters.</param>
        /// <returns>The compare options.</returns>
        public static CompareOptions AsCompareOptions(this RenderingParameters parameters)
        {
            CompareOptions compareOptions = CompareOptions.None;
            if (parameters.InterlinearIgnoresCase)
            {
                compareOptions |= CompareOptions.IgnoreCase;
            }

            if (parameters.InterlinearIgnoresDiacritics)
            {
                compareOptions |= CompareOptions.IgnoreNonSpace;
            }

            if (parameters.InterlinearIgnoresPunctuation)
            {
                compareOptions |= CompareOptions.IgnoreSymbols;
            }

            return compareOptions;
        }

        /// <summary>
        /// Gets the verse number from a line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>
        /// The verse number, with any:
        ///  - letters, see https://goto.bible/1.Kings.12_24b/65bfdebd704a8324-01 (Brenton's English Septuagtin)
        ///  - hyphens, see https://goto.bible/Matthew.23_13-14/5e51f89e89947acb-01 (Aramu New Testament)
        ///  - brackets, see https://goto.bible/Acts.19_41/SBLGNT (SBL Greek New Testament)
        /// NOTE: The input must be directly from a translation.
        /// </returns>
        public static string GetVerseNumber(this string line) => line.Contains(' ') ? line.Substring(0, line.IndexOf(' ')).Trim() : string.Empty;

        /// <summary>
        /// Determines whether this is a valid verse number.
        /// </summary>
        /// <param name="verseNumber">The verse number.</param>
        /// <returns>
        ///   <c>true</c> if the specified verse number is a valid verse number; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidVerseNumber(this string verseNumber)
        {
            Match isValidVerseNumber = VerseNumberRegex.Match(verseNumber);
            return isValidVerseNumber.Success && isValidVerseNumber.Value.Length == verseNumber.Length;
        }

        /// <summary>
        /// Determines whether or not a verse matches the list of verse to highlight.
        /// </summary>
        /// <param name="verseNumber">The verse number.</param>
        /// <param name="highlightedVerses">The highlighted verses array from the <see cref="PassageReference" />.</param>
        /// <returns>
        ///   <c>true</c> if the specified verse should be highlighted; otherwise, <c>false</c>.
        /// </returns>
        public static bool MatchesHighlightedVerses(this string verseNumber, string[] highlightedVerses)
        {
            // Ensure that the verse number is valid, and there are highlighted verses
            if (string.IsNullOrWhiteSpace(verseNumber) || !highlightedVerses.Any() || !verseNumber.IsValidVerseNumber())
            {
                return false;
            }

            // Strip any brackets
            verseNumber = verseNumber.Trim('[', ']');

            // Split any hypenated verses
            if (verseNumber.Contains('-'))
            {
                string[] verseNumbers = verseNumber.Split('-', StringSplitOptions.RemoveEmptyEntries);
                return verseNumbers.First().MatchesHighlightedVerses(highlightedVerses)
                    || verseNumbers.Last().MatchesHighlightedVerses(highlightedVerses);
            }

            // We need to pad the numbers to account for letters
            if (int.TryParse(verseNumber, out int _))
            {
                verseNumber += "-"; // A hyphen is used in versification.vrs, and as lower in the ASCII table
            }

            // We pad the verse to allow correct calculation, accounting for tens and hundreds
            verseNumber = verseNumber.PadLeft(4, '0');

            for (int i = 0; i < highlightedVerses.Length; i++)
            {
                if (int.TryParse(highlightedVerses[i], out int _))
                {
                    highlightedVerses[i] += "-";
                }

                // Pad verses that are not a hyphen only
                if (highlightedVerses[i] != "-")
                {
                    highlightedVerses[i] = highlightedVerses[i].PadLeft(4, '0');
                }
            }

            // See if the verse number is within the range
            for (int i = 0; i < highlightedVerses.Length; i++)
            {
                if (verseNumber == highlightedVerses[i])
                {
                    // Direct hit
                    return true;
                }
                else if (highlightedVerses[i] == "-" && i > 0 && i < (highlightedVerses.Length - 1))
                {
                    // Check inside this range
                    bool matches = string.CompareOrdinal(verseNumber, highlightedVerses[i - 1]) > 0 && string.CompareOrdinal(verseNumber, highlightedVerses[i + 1]) < 0;
                    if (matches)
                    {
                        return matches;
                    }
                }
                else if (highlightedVerses[i] == "-" && i > 0 && i == (highlightedVerses.Length - 1))
                {
                    // Check from this range, as the last highlighted verse value is a hyphen
                    return string.CompareOrdinal(verseNumber, highlightedVerses[i - 1]) > 0;
                }
            }

            // Default to not highlighting the verse
            return false;
        }

        /// <summary>
        /// Renders the supplied words in italics.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="italicsTag">The italics tag. This may either by "i" or "em" (default).</param>
        /// <returns>
        /// The line with the supplied words rendered in italics.
        /// </returns>
        /// <remarks>
        /// Full-width square brakets are replaced with regular square brackets.
        /// </remarks>
        public static string RenderItalics(this string line, string italicsTag = "em")
            => line.Replace("[[", "<pre>").Replace("]]", "</pre>")
            .Replace("[", $"<{italicsTag}>").Replace("]", $"</{italicsTag}>")
            .Replace("<pre>", "[[").Replace("</pre>", "]]")
            .Replace("［", "[").Replace("］", "]");

        /// <summary>
        /// Renders the supplied words in normal type.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>
        /// The line with the supplied words rendered in normal type.
        /// </returns>
        /// <remarks>
        /// Full-width square brakets are replaced with regular square brackets.
        /// </remarks>
        public static string StripItalics(this string line)
            => line.Replace("[", string.Empty).Replace("]", string.Empty)
            .Replace("［", "[").Replace("］", "]");

        /// <summary>
        /// Gets a unique name for the translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="translations">The translations.</param>
        /// <returns>
        /// The unique name.
        /// </returns>
        public static string UniqueName(this Translation translation, IEnumerable<Translation> translations)
        {
            IEnumerable<Translation> enumerable = translations as Translation[] ?? translations.ToArray();
            if (enumerable.Count(t => string.Equals(t.Name, translation.Name, StringComparison.OrdinalIgnoreCase)) <= 1)
            {
                return translation.Name;
            }
            else if (enumerable.Count(t => string.Equals(t.Name, translation.Name, StringComparison.OrdinalIgnoreCase) && t.Year == translation.Year && t.Year > 0) == 1)
            {
                return $"{translation.Name} ({translation.Year})";
            }
            else if (enumerable.Count(t => string.Equals(t.Name, translation.Name, StringComparison.OrdinalIgnoreCase) && t.Language == translation.Language) == 1)
            {
                // English translation names (i.e. King James Version) have priority
                if (translation.Language == "English")
                {
                    return translation.Name;
                }
                else
                {
                    return $"{translation.Language} {translation.Name}";
                }
            }
            else if (enumerable.Count(t => string.Equals(t.Name, translation.Name, StringComparison.OrdinalIgnoreCase)
                                           && t.Dialect == translation.Dialect && !string.IsNullOrWhiteSpace(t.Dialect)) == 1)
            {
                return $"{translation.Name} ({translation.Dialect})";
            }
            else
            {
                return $"{translation.Name} ({translation.Provider})";
            }
        }
    }
}
