// -----------------------------------------------------------------------
// <copyright file="LanguageComparer.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

using System;
using System.Collections.Generic;

/// <summary>
/// A comparer to sort the languages with English then Greek at the top.
/// </summary>
public class LanguageComparer : IComparer<string?>
{
    /// <inheritdoc/>
    public int Compare(string? x, string? y) =>
        string.Compare(
            GetCustomSortValue(x),
            GetCustomSortValue(y),
            StringComparison.InvariantCultureIgnoreCase
        );

    /// <summary>
    /// Gets the custom sort value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The custom sort value.</returns>
    private static string GetCustomSortValue(string? value) =>
        value?.ToUpperInvariant() switch
        {
            "ENGLISH" => $"!1-{value}",
            "GREEK" => $"!2-{value}",
            "HEBREW" => $"!3-{value}",
            "LATIN" => $"!4-{value}",
            null => "!5",
            _ => value,
        };
}
