// -----------------------------------------------------------------------
// <copyright file="BibliaApiOptions.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

/// <summary>
/// The Biblia API Provider Options.
/// </summary>
public class BibliaApiOptions
{
    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    /// <value>
    /// The API key.
    /// </value>
    public string ApiKey { get; set; } = string.Empty;
}
