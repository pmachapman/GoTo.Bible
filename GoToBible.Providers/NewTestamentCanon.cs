﻿// -----------------------------------------------------------------------
// <copyright file="NewTestamentCanon.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System.Collections.Specialized;

/// <summary>
/// New Testament Canon helper functions.
/// </summary>
internal class NewTestamentCanon : BookHelper
{
    /// <inheritdoc />
    protected override OrderedDictionary BookChapters =>
        new OrderedDictionary
        {
            ["matthew"] = 28,
            ["mark"] = 16,
            ["luke"] = 24,
            ["john"] = 21,
            ["acts"] = 28,
            ["romans"] = 16,
            ["1 corinthians"] = 16,
            ["2 corinthians"] = 13,
            ["galatians"] = 6,
            ["ephesians"] = 6,
            ["philippians"] = 4,
            ["colossians"] = 4,
            ["1 thessalonians"] = 5,
            ["2 thessalonians"] = 3,
            ["1 timothy"] = 6,
            ["2 timothy"] = 4,
            ["titus"] = 3,
            ["philemon"] = 1,
            ["hebrews"] = 13,
            ["james"] = 5,
            ["1 peter"] = 5,
            ["2 peter"] = 3,
            ["1 john"] = 5,
            ["2 john"] = 1,
            ["3 john"] = 1,
            ["jude"] = 1,
            ["revelation"] = 22,
        };
}
