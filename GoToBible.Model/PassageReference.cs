// -----------------------------------------------------------------------
// <copyright file="PassageReference.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    using System;

    /// <summary>
    /// A passage reference.
    /// </summary>
    public class PassageReference
    {
        /// <summary>
        /// Gets or sets the chapter reference.
        /// </summary>
        /// <value>
        /// The chapter reference.
        /// </value>
        public ChapterReference ChapterReference { get; set; } = new ChapterReference();

        /// <summary>
        /// Gets or sets the passage reference to display.
        /// </summary>
        /// <value>
        /// The passage reference.
        /// </value>
        public string Display { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the highlighted verses.
        /// </summary>
        /// <value>
        /// The verses to highlight.
        /// </value>
        public int[] HighlightedVerses { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// We only require the chapter reference. This method assumes you used <c>AsPassageReference()</c> to generate this object.
        /// </remarks>
        public bool IsValid => this.ChapterReference.IsValid;
    }
}
