// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Client
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Extensions Methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// The strip invalid characters regular expression.
        /// </summary>
        private static readonly Regex StripInvalidCharactersRegex = new Regex(@"[^a-zA-Z0-9\. _~:,-]", RegexOptions.Compiled);

        /// <summary>
        /// Encodes the passage for a URL.
        /// </summary>
        /// <param name="passage">The passage.</param>
        /// <returns>
        /// A string suitable for including in a page URL.
        /// </returns>
        /// <remarks>
        /// For example: "1 John 1:3,6-7" will be encoded as "1.John.1_3~6-7".
        /// </remarks>
        public static string EncodePassageForUrl(this string passage)
        {
            if (string.IsNullOrWhiteSpace(passage))
            {
                return string.Empty;
            }

            // Strip invalid characters
            string segment = StripInvalidCharactersRegex.Replace(passage, string.Empty);
            segment = segment.Replace(" ", ".", StringComparison.OrdinalIgnoreCase);
            segment = segment.Replace(":", "_", StringComparison.OrdinalIgnoreCase);
            segment = segment.Replace(",", "~", StringComparison.OrdinalIgnoreCase);
            return segment;
        }

        /// <summary>
        /// Decodes the passage from a URL.
        /// </summary>
        /// <param name="segment">The URL segment.</param>
        /// <returns>
        /// A string suitable for passing to the <c>RenderPassage</c> API.
        /// </returns>
        /// <remarks>
        /// For example: "1.John.1_3~6-7" will be decoded to "1 John 1:3,6-7".
        /// </remarks>
        public static string DecodePassageFromUrl(this string segment)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                return string.Empty;
            }

            // Strip invalid characters
            string passage = StripInvalidCharactersRegex.Replace(segment, string.Empty);
            passage = passage.Replace(".", " ", StringComparison.OrdinalIgnoreCase);
            passage = passage.Replace("_", ":", StringComparison.OrdinalIgnoreCase);
            passage = passage.Replace("~", ",", StringComparison.OrdinalIgnoreCase);
            return passage;
        }
    }
}
