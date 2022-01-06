// -----------------------------------------------------------------------
// <copyright file="RenderedPassage.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    /// <summary>
    /// A rendered passage.
    /// </summary>
    public class RenderedPassage
    {
        /// <summary>
        /// Gets or sets the passage content.
        /// </summary>
        /// <value>
        /// The passage content.
        /// </value>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the next passage.
        /// </summary>
        /// <value>
        /// The next passage.
        /// </value>
        public PassageReference NextPassage { get; set; } = new PassageReference();

        /// <summary>
        /// Gets or sets the previous passage.
        /// </summary>
        /// <value>
        /// The previous passage.
        /// </value>
        public PassageReference PreviousPassage { get; set; } = new PassageReference();

        /// <summary>
        /// Gets or sets the rendering suggestions.
        /// </summary>
        /// <value>
        /// The rendering suggestions.
        /// </value>
        public RenderingSuggestions Suggestions { get; set; } = new RenderingSuggestions();
    }
}
