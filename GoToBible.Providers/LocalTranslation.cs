// -----------------------------------------------------------------------
// <copyright file="LocalTranslation.cs" company="Conglomo">
// Copyright 2020-2026 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using CsvHelper;
using GoToBible.Model;

/// <summary>
/// A locally available translation.
/// </summary>
/// <seealso cref="Translation" />
[CsvRecord]
public partial class LocalTranslation : Translation
{
    /// <summary>
    /// Gets or sets the filename.
    /// </summary>
    /// <value>
    /// The filename.
    /// </value>
    public string Filename { get; set; } = string.Empty;
}
