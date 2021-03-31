// -----------------------------------------------------------------------
// <copyright file="LanguageComparer.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A comparer to sort the languages with English then Greek at the top.
    /// </summary>
    public class LanguageComparer : IComparer<string?>
    {
        /// <inheritdoc/>
        public int Compare(string? x, string? y)
        {
            if (x?.ToUpperInvariant() == "ENGLISH" && y?.ToUpperInvariant() == "ENGLISH")
            {
                return 0;
            }
            else if (x?.ToUpperInvariant() == "ENGLISH" && y?.ToUpperInvariant() != "ENGLISH")
            {
                return -1;
            }
            else if (x?.ToUpperInvariant() != "ENGLISH" && y?.ToUpperInvariant() == "ENGLISH")
            {
                return 1;
            }
            else if (x?.ToUpperInvariant() == "GREEK" && y?.ToUpperInvariant() == "GREEK")
            {
                return 0;
            }
            else if (x?.ToUpperInvariant() == "GREEK" && y?.ToUpperInvariant() != "ENGLISH")
            {
                return -1;
            }
            else if (x?.ToUpperInvariant() != "ENGLISH" && y?.ToUpperInvariant() == "GREEK")
            {
                return 1;
            }
            else
            {
                return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}
