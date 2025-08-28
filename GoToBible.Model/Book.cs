// -----------------------------------------------------------------------
// <copyright file="Book.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

using System.Collections.Generic;

/// <summary>
/// A book in a translation.
/// </summary>
public class Book
{
    /// <summary>
    /// Gets the chapters.
    /// </summary>
    /// <value>
    /// The chapters.
    /// </value>
    public IReadOnlyCollection<ChapterReference> Chapters { get; init; } =
        new List<ChapterReference>();

    /// <summary>
    /// Gets or sets the name of the book.
    /// </summary>
    /// <value>
    /// The name of the book.
    /// </value>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override string ToString() => this.Name;
}
