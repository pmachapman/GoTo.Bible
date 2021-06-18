// -----------------------------------------------------------------------
// <copyright file="RenderingSuggestions.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
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
    }
}
