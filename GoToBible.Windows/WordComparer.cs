// -----------------------------------------------------------------------
// <copyright file="WordComparer.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
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
public class WordComparer : IComparer<string>
{
    private readonly RenderingParameters parameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="WordComparer" /> class.
    /// </summary>
    /// <param name="parameters">The rendering parameters.</param>
    public WordComparer(RenderingParameters parameters) => this.parameters = parameters;

    /// <inheritdoc />
    public int Compare(string? x, string? y)
        => string.Compare(x?.Clean(), y?.Clean(), CultureInfo.InvariantCulture, this.parameters.AsCompareOptions());
}
