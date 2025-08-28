// -----------------------------------------------------------------------
// <copyright file="NltBibleOptions.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

/// <summary>
/// The NLT API Provider Options.
/// </summary>
public class NltBibleOptions
{
    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    /// <value>
    /// The API key.
    /// </value>
    public string ApiKey { get; set; } = string.Empty;
}
