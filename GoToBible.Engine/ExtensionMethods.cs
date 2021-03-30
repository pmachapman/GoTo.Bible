// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Engine
{
    using System;
    using System.Globalization;
    using GoToBible.Model;

    /// <summary>
    /// General Extension Methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the <see cref="CompareOptions"/> matching the <see cref="RenderingParameters"/>.
        /// </summary>
        /// <param name="parameters">The renderign parameters.</param>
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
        public static string RenderItalics(this string line, string italicsTag = "em")
        {
            if (line.Contains("{cf15i ", StringComparison.InvariantCulture))
            {
                return line.Replace("{cf15i ", $"<{italicsTag}>").Replace("}", $"</{italicsTag}>");
            }
            else
            {
                line = line.Replace("[[", "<pre>").Replace("]]", "</pre>");
                line = line.Replace("[", $"<{italicsTag}>").Replace("]", $"</{italicsTag}>");
                line = line.Replace("<pre>", "[[").Replace("</pre>", "]]");
                return line;
            }
        }

        /// <summary>
        /// Renders the supplied words in normal type.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>
        /// The line with the supplied words rendered in normal type.
        /// </returns>
        public static string StripItalics(this string line)
        {
            if (line.Contains("{cf15i ", StringComparison.InvariantCulture))
            {
                return line.Replace("{cf15i ", string.Empty).Replace("}", string.Empty);
            }
            else
            {
                return line.Replace("[", string.Empty).Replace("]", string.Empty);
            }
        }
    }
}
