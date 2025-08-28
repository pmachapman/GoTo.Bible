// -----------------------------------------------------------------------
// <copyright file="VerseComparer.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// A verse string comparer.
/// </summary>
/// <remarks>This handles numeric, hyphenated, and sub-verse numbers.</remarks>
public partial class VerseComparer : IComparer<string>
{
    /// <inheritdoc />
    public int Compare(string? x, string? y)
    {
        if (int.TryParse(x, out int a) && int.TryParse(y, out int b))
        {
            // Numeric verses
            return a.CompareTo(b);
        }
        else if (x is null && y is null)
        {
            return 0;
        }
        else if (x is not null && y is null)
        {
            return 1;
        }
        else if (x is null && y is not null)
        {
            return -1;
        }
        else if (int.TryParse(x, out a))
        {
            if (VerseNumberWithLetterRegex().IsMatch(y!))
            {
                // Get the digit
                if (!int.TryParse(y![..^1], out b))
                {
                    return -1;
                }
                else if (a == b)
                {
                    return -1;
                }
                else
                {
                    return a.CompareTo(b);
                }
            }
            else if (VerseRangeRegex().IsMatch(y!))
            {
                // Get the first digit
                if (!int.TryParse(new string([.. y!.TakeWhile(char.IsDigit)]), out b))
                {
                    return -1;
                }
                else if (a == b)
                {
                    return -1;
                }
                else
                {
                    return a.CompareTo(b);
                }
            }
            else
            {
                // Unknown verse format
                return -1;
            }
        }
        else if (int.TryParse(y, out b))
        {
            if (VerseNumberWithLetterRegex().IsMatch(x!))
            {
                // Get the digit
                if (!int.TryParse(x![..^1], out a))
                {
                    return 1;
                }
                else if (a == b)
                {
                    return 1;
                }
                else
                {
                    return a.CompareTo(b);
                }
            }
            else if (VerseRangeRegex().IsMatch(x!))
            {
                // Get the first digit
                if (!int.TryParse(new string([.. x!.TakeWhile(char.IsDigit)]), out a))
                {
                    return 1;
                }
                else if (a == b)
                {
                    return 1;
                }
                else
                {
                    return a.CompareTo(b);
                }
            }
            else
            {
                // Unknown verse format
                return -1;
            }
        }
        else if (
            VerseNumberWithLetterRegex().IsMatch(x!) && VerseNumberWithLetterRegex().IsMatch(y!)
        )
        {
            if (!int.TryParse(x![..^1], out a) || !int.TryParse(y![..^1], out b))
            {
                // This should not occur, based on the regex
                return string.Compare(x, y, StringComparison.Ordinal);
            }
            else if (a == b)
            {
                return x[^1].CompareTo(y[^1]);
            }
            else
            {
                return a.CompareTo(b);
            }
        }
        else if (VerseRangeRegex().IsMatch(x!) && VerseRangeRegex().IsMatch(y!))
        {
            // Get the first digits
            if (
                !int.TryParse(new string([.. x!.TakeWhile(char.IsDigit)]), out a)
                || !int.TryParse(new string([.. y!.TakeWhile(char.IsDigit)]), out b)
            )
            {
                // This should not occur, based on the regex
                return string.Compare(x, y, StringComparison.Ordinal);
            }
            else if (a == b)
            {
                // They are both the same verse number
                return string.Compare(x, y, StringComparison.Ordinal);
            }
            else
            {
                return a.CompareTo(b);
            }
        }
        else
        {
            // Unknown verse format
            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Gets the verse number with letter regular expression.
    /// </summary>
    /// <returns>The regular expression for a verse number with a letter.</returns>
    [GeneratedRegex("^[0-9]+[a-z]$", RegexOptions.Compiled)]
    private static partial Regex VerseNumberWithLetterRegex();

    /// <summary>
    /// Gets the verse range regular expression.
    /// </summary>
    /// <returns>The regular expression for a verse range.</returns>
    [GeneratedRegex("^[0-9]+[a-z]?-[0-9]+[a-z]?$", RegexOptions.Compiled)]
    private static partial Regex VerseRangeRegex();
}
