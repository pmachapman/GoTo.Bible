// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;

/// <summary>
/// Extension methods.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Normalises the language name.
    /// </summary>
    /// <param name="language">The language</param>
    /// <returns>The language, suitable for display and grouping.</returns>
    public static string NormaliseLanguage(this string language)
        => language switch
        {
            "Arabic, Standard" => "Arabic",
            "German, Standard" => "German",
            "Greek, Ancient" => "Greek",
            _ => language,
        };

    /// <summary>
    /// Normalises the line endings.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="lineEnding">The line ending to normalise to.</param>
    /// <returns>
    /// The value with normalised line endings.
    /// </returns>
    public static string NormaliseLineEndings(this string value, string lineEnding = "\n")
        => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal).Replace("\n", lineEnding, StringComparison.Ordinal);
}
