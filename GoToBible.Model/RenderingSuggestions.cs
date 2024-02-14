// -----------------------------------------------------------------------
// <copyright file="RenderingSuggestions.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

/// <summary>
/// Rendering suggestions from the renderer.
/// </summary>
/// <remarks>
/// The client can suggest to the user different settings or translations from these suggestions.
/// </remarks>
public class RenderingSuggestions
{
    /// <summary>
    /// Gets or sets a value indicating whether the client should suggest to ignore case, diacritics and punctuation.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the client should ignore case diacritics and punctuation; otherwise, <c>false</c>.
    /// </value>
    public bool IgnoreCaseDiacriticsAndPunctuation { get; set; }

    /// <summary>
    /// Gets or sets the suggested chapter to navigate to.
    /// </summary>
    /// <value>
    /// The suggested chapter to navigate to, or null if no suggestions.
    /// </value>
    /// <remarks>
    /// This is specified if the chapter does not exist.
    /// </remarks>
    public ChapterReference? NavigateToChapter { get; set; }
}
