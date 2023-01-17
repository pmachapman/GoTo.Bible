// -----------------------------------------------------------------------
// <copyright file="OldTestamentCanon.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System.Collections.Specialized;

/// <summary>
/// Old Testament Canon helper functions.
/// </summary>
internal class OldTestamentCanon : BookHelper
{
    /// <inheritdoc />
    protected override OrderedDictionary BookChapters => new OrderedDictionary
    {
        ["genesis"] = 50,
        ["exodus"] = 40,
        ["leviticus"] = 27,
        ["numbers"] = 36,
        ["deuteronomy"] = 34,
        ["joshua"] = 24,
        ["judges"] = 21,
        ["ruth"] = 4,
        ["1 samuel"] = 31,
        ["2 samuel"] = 24,
        ["1 kings"] = 22,
        ["2 kings"] = 25,
        ["1 chronicles"] = 29,
        ["2 chronicles"] = 36,
        ["ezra"] = 10,
        ["nehemiah"] = 13,
        ["esther"] = 10,
        ["job"] = 42,
        ["psalm"] = 150,
        ["proverbs"] = 31,
        ["ecclesiastes"] = 12,
        ["song of solomon"] = 8,
        ["isaiah"] = 66,
        ["jeremiah"] = 52,
        ["lamentations"] = 5,
        ["ezekiel"] = 48,
        ["daniel"] = 12,
        ["hosea"] = 14,
        ["joel"] = 3,
        ["amos"] = 9,
        ["obadiah"] = 1,
        ["jonah"] = 4,
        ["micah"] = 7,
        ["nahum"] = 3,
        ["habakkuk"] = 3,
        ["zephaniah"] = 3,
        ["haggai"] = 2,
        ["zechariah"] = 14,
        ["malachi"] = 4,
    };
}
