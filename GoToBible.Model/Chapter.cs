// -----------------------------------------------------------------------
// <copyright file="Chapter.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    /// <summary>
    /// A chapter retrieved from a provider.
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// Gets or sets the book.
        /// </summary>
        /// <value>
        /// The book.
        /// </value>
        public string Book { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the chapter number.
        /// </summary>
        /// <value>
        /// The chapter number.
        /// </value>
        public int ChapterNumber { get; set; } = 0;

        /// <summary>
        /// Gets or sets the copyright message.
        /// </summary>
        /// <value>
        /// The copyright message.
        /// </value>
        /// <remarks>
        /// This is to be displayed after the text, and will be HTML.
        /// </remarks>
        public string Copyright { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the next chapter reference.
        /// </summary>
        /// <value>
        /// The next chapter reference.
        /// </value>
        public ChapterReference NextChapterReference { get; set; } = new ChapterReference();

        /// <summary>
        /// Gets or sets the previous chapter reference.
        /// </summary>
        /// <value>
        /// The previous chapter reference.
        /// </value>
        public ChapterReference PreviousChapterReference { get; set; } = new ChapterReference();

        /// <summary>
        /// Gets or sets a value indicating whether this chapter supports italics.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this chapter supports italics; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsItalics { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the translation.
        /// </summary>
        /// <value>
        /// The translation.
        /// </value>
        public string Translation { get; set; } = string.Empty;
    }
}
