// -----------------------------------------------------------------------
// <copyright file="Book.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// A book in a translation.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Gets the chapters.
        /// </summary>
        /// <value>
        /// The chapters.
        /// </value>
        public ReadOnlyCollection<ChapterReference> Chapters { get; init; } = new List<ChapterReference>().AsReadOnly();

        /// <summary>
        /// Gets or sets the name of the book.
        /// </summary>
        /// <value>
        /// The name of the book.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public override string ToString() => this.Name;
    }
}
