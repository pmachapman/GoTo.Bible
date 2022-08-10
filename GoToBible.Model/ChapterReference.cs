// -----------------------------------------------------------------------
// <copyright file="ChapterReference.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    using System;

    /// <summary>
    /// A chapter reference.
    /// </summary>
    public record ChapterReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChapterReference" /> class.
        /// </summary>
        /// <remarks>
        /// This returns an invalid chapter reference.
        /// </remarks>
        public ChapterReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChapterReference"/> class.
        /// </summary>
        /// <param name="book">The book name.</param>
        /// <param name="chapter">The chapter.</param>
        public ChapterReference(string book, int chapter)
        {
            // Special case for Psalm 151, as it is often a separate book
            if (book.Replace(" ", string.Empty).ToLowerInvariant() == "psalm151")
            {
                this.Book = "Psalm";
                this.ChapterNumber = 151;
            }
            else
            {
                this.Book = book;
                this.ChapterNumber = chapter;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChapterReference"/> class.
        /// </summary>
        /// <param name="bookAndChapter">The book name and chapter number.</param>
        public ChapterReference(string bookAndChapter)
        {
            if (!string.IsNullOrWhiteSpace(bookAndChapter))
            {
                int lastSpaceIndex = bookAndChapter.LastIndexOf(" ", StringComparison.OrdinalIgnoreCase);
                if (lastSpaceIndex > -1)
                {
                    this.Book = bookAndChapter[..lastSpaceIndex];
                    string chapter = bookAndChapter[(lastSpaceIndex + 1)..];
                    if (chapter.Contains(':'))
                    {
                        chapter = chapter[..chapter.IndexOf(":", StringComparison.OrdinalIgnoreCase)];
                    }

                    if (int.TryParse(chapter, out int chapterNumber))
                    {
                        this.ChapterNumber = chapterNumber;
                    }
                    else if (int.TryParse(this.Book, out int _))
                    {
                        // Handle the case for one chapter books that start with a number
                        this.Book = bookAndChapter;
                    }

                    return;
                }
            }

            this.Book = bookAndChapter;
        }

        /// <summary>
        /// Gets or sets the name of the book.
        /// </summary>
        /// <value>
        /// The book name.
        /// </value>
        public string Book { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the chapter number.
        /// </summary>
        /// <value>
        /// The chapter number.
        /// </value>
        /// <remarks>
        /// Chapter 0 refers to the introduction.
        /// </remarks>
        public int ChapterNumber { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid => !string.IsNullOrWhiteSpace(this.Book);

        /// <inheritdoc/>
        public override string ToString() => this.IsValid ? $"{this.Book} {this.ChapterNumber}" : string.Empty;
    }
}
