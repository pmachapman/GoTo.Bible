// -----------------------------------------------------------------------
// <copyright file="OneBookHelper.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System.Collections.Specialized;

    /// <summary>
    /// Helper functions for providers with only one book.
    /// </summary>
    internal class OneBookHelper : BookHelper
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="OneBookHelper"/> class.
        /// </summary>
        /// <param name="bookName">Name of the book.</param>
        /// <param name="chapters">The chapters.</param>
        public OneBookHelper(string bookName, int chapters)
        {
            this.BookChapters = new OrderedDictionary { [bookName.ToLowerInvariant()] = chapters };
        }

        /// <inheritdoc />
        protected override OrderedDictionary BookChapters { get; }
    }
}
