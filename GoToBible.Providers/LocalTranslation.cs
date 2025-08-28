// -----------------------------------------------------------------------
// <copyright file="LocalTranslation.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using GoToBible.Model;

/// <summary>
/// A locally available translation.
/// </summary>
/// <seealso cref="Translation" />
public class LocalTranslation : Translation
{
    /// <summary>
    /// Gets or sets the filename.
    /// </summary>
    /// <value>
    /// The filename.
    /// </value>
    public string Filename { get; set; } = string.Empty;
}
