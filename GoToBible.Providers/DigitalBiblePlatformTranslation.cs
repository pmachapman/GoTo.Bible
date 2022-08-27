// -----------------------------------------------------------------------
// <copyright file="DigitalBiblePlatformTranslation.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System.Collections.Generic;
using GoToBible.Model;

/// <summary>
/// A translation from the Digital Bible Platform.
/// </summary>
/// <seealso cref="GoToBible.Model.Translation" />
public class DigitalBiblePlatformTranslation : Translation
{
    /// <summary>
    /// Gets or sets the DAM Ids for each volume.
    /// </summary>
    /// <value>
    /// The DAM Ids.
    /// </value>
    public IReadOnlyCollection<string> DamIds { get; set; } = new List<string>();
}
