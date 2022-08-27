// -----------------------------------------------------------------------
// <copyright file="RenderedVerse.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Engine;

/// <summary>
/// A rendered verse with rendering statistics.
/// </summary>
/// <remarks>
/// This is useful for interlinear mode to determine or debug the best rendering.
/// </remarks>
internal class RenderedVerse
{
    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>
    /// The content.
    /// </value>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of divergent phrases.
    /// </summary>
    /// <value>
    /// The number of divergent phrases.
    /// </value>
    public int DivergentPhrases { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total number of words in line one.
    /// </summary>
    /// <value>
    /// The total number of words in line one.
    /// </value>
    public int TotalWordsLine1 { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total number of words in line two.
    /// </summary>
    /// <value>
    /// The total number of words in line two.
    /// </value>
    public int TotalWordsLine2 { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of words in common.
    /// </summary>
    /// <value>
    /// The number of words in common.
    /// </value>
    public int WordsInCommon { get; set; } = 0;
}
