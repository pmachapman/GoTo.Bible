// -----------------------------------------------------------------------
// <copyright file="ChapterReference.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    /// <summary>
    /// A chapter reference.
    /// </summary>
    public class ChapterReference
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ChapterReference" /> class.
        /// </summary>
        /// <remarks>
        /// This returns an invalid chapter reference.
        /// </remarks>
        public ChapterReference()
        {
            this.Book = string.Empty;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ChapterReference"/> class.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="chapter">The chapter.</param>
        public ChapterReference(string book, int chapter)
        {
            this.Book = book;
            this.ChapterNumber = chapter;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ChapterReference"/> class.
        /// </summary>
        /// <param name="bookAndChapter">The book and chapter number.</param>
        public ChapterReference(string bookAndChapter)
        {
            if (!string.IsNullOrWhiteSpace(bookAndChapter))
            {
                int lastSpaceIndex = bookAndChapter.LastIndexOf(' ');
                if (lastSpaceIndex > -1)
                {
                    this.Book = bookAndChapter.Substring(0, lastSpaceIndex);
                    if (int.TryParse(bookAndChapter[(lastSpaceIndex + 1)..], out int chapter))
                    {
                        this.ChapterNumber = chapter;
                    }

                    return;
                }
            }

            this.Book = bookAndChapter;
        }

        /// <summary>
        /// Gets the book.
        /// </summary>
        /// <value>
        /// The book.
        /// </value>
        public string Book { get; }

        /// <summary>
        /// Gets the chapter number.
        /// </summary>
        /// <value>
        /// The chapter number.
        /// </value>
        public int ChapterNumber { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Chapter 0 refers to the introduction.
        /// </remarks>
        public bool IsValid => !string.IsNullOrWhiteSpace(this.Book);

        /// <inheritdoc/>
        public override string ToString() => $"{this.Book} {this.ChapterNumber}";
    }
}
