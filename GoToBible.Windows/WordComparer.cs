// -----------------------------------------------------------------------
// <copyright file="WordComparer.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System.Collections.Generic;
using System.Globalization;
using GoToBible.Engine;
using GoToBible.Model;

/// <summary>
/// A word/phrase string comparer.
/// </summary>
public class WordComparer(RenderingParameters parameters) : IComparer<string>
{
    /// <summary>
    /// The rendering parameters.
    /// </summary>
    private readonly RenderingParameters parameters = parameters;

    /// <inheritdoc />
    public int Compare(string? x, string? y) =>
        string.Compare(
            x?.Clean(),
            y?.Clean(),
            CultureInfo.InvariantCulture,
            this.parameters.AsCompareOptions()
        );
}
