// -----------------------------------------------------------------------
// <copyright file="DigitalBiblePlatformApiOptions.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

/// <summary>
/// The Digital Bible Platform API Provider Options.
/// </summary>
public class DigitalBiblePlatformApiOptions
{
    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    /// <value>
    /// The API key.
    /// </value>
    public string ApiKey { get; set; } = string.Empty;
}
