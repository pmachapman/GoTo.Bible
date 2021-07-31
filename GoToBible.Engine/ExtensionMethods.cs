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
    using GoToBible.Model;

    /// <summary>
    /// General Extension Methods.
    /// </summary>
    public static class ExtensionMethods
    {
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
