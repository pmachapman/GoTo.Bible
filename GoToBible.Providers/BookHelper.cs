// -----------------------------------------------------------------------
// <copyright file="BookHelper.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using GoToBible.Model;

/// <summary>
/// Bible Book helper functions.
/// </summary>
internal class BookHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookHelper"/> class.
    /// </summary>
    public BookHelper()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BookHelper"/> class.
    /// </summary>
    /// <param name="bookChapters">The book chapters.</param>
    public BookHelper(OrderedDictionary bookChapters) => this.BookChapters = bookChapters;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookHelper"/> class.
    /// </summary>
    /// <param name="bookName">Name of the book.</param>
    /// <param name="chapters">The chapters.</param>
    public BookHelper(string bookName, int chapters) => this.BookChapters = new OrderedDictionary { [bookName.ToLowerInvariant()] = chapters };

    /// <summary>
    /// Gets the numbers of chapters in each book.
    /// </summary>
    protected virtual OrderedDictionary BookChapters { get; } = new OrderedDictionary();

    /// <summary>
    /// Gets the book names.
    /// </summary>
    private ReadOnlyCollection<string> BookNames => this.BookChapters.Keys.Cast<string>().ToList().AsReadOnly();

    /// <summary>
    /// Gets the book number.
    /// </summary>
    /// <param name="book">The name of the book.</param>
    /// <returns>The number of the book</returns>
    public int GetBookNum(string book) => this.BookNames.IndexOf(book.ToLowerInvariant()) + 1;

    /// <summary>
    /// Gets the books in the Protestant canon.
    /// </summary>
    /// <param name="includeChapters">If set to <c>true</c>, include chapters.</param>
    /// <returns>
    /// The books.
    /// </returns>
    public IEnumerable<Book> GetBooks(bool includeChapters)
    {
        foreach (string bookName in this.BookNames)
        {
            string capitalisedBookName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(bookName);
            if (!includeChapters)
            {
                // Create and return the book
                yield return new Book
                {
                    Name = capitalisedBookName,
                };
            }
            else if (this.BookChapters[bookName] is int lastChapter)
            {
                // We need to include the chapters
                List<ChapterReference> chapters = new List<ChapterReference>();
                for (int chapter = 1; chapter <= lastChapter; chapter++)
                {
                    chapters.Add(new ChapterReference(capitalisedBookName, chapter));
                }

                // Create and return the book
                yield return new Book
                {
                    Chapters = chapters.AsReadOnly(),
                    Name = capitalisedBookName,
                };
            }
        }
    }

    /// <summary>
    /// Gets the next chapter in sequence.
    /// </summary>
    /// <param name="book">The book name.</param>
    /// <param name="chapter">The chapter.</param>
    /// <returns>
    /// The chapter reference to the next chapter in sequence.
    /// </returns>
    public ChapterReference GetNextChapter(string book, int chapter)
    {
        string bookLower = book.ToLowerInvariant();

        // Psalm 151 clean up
        if (bookLower == "psalm" && chapter == 151)
        {
            bookLower = "psalm 151";
            chapter = 1;
        }

        if (this.BookChapters.Contains(bookLower) && this.BookChapters[bookLower] is int chapters)
        {
            if (chapters > chapter)
            {
                return new ChapterReference(book, ++chapter);
            }
            else if (bookLower == this.BookNames.Last())
            {
                return new ChapterReference();
            }
            else
            {
                return new ChapterReference(this.BookNames[this.BookNames.IndexOf(bookLower) + 1], 1);
            }
        }
        else
        {
            return new ChapterReference();
        }
    }

    /// <summary>
    /// Gets the number of chapters in the book.
    /// </summary>
    /// <param name="book">The book.</param>
    /// <returns>
    /// The number of chapters.
    /// </returns>
    public int GetNumberOfChapters(string book)
    {
        string bookLower = book.ToLowerInvariant();
        return this.BookChapters.Contains(bookLower) && this.BookChapters[bookLower] is int chapters ? chapters : 0;
    }

    /// <summary>
    /// Gets the previous chapter.
    /// </summary>
    /// <param name="book">The book name.</param>
    /// <param name="chapter">The chapter.</param>
    /// <returns>
    /// The chapter reference to the previous chapter in sequence.
    /// </returns>
    public ChapterReference GetPreviousChapter(string book, int chapter)
    {
        string bookLower = book.ToLowerInvariant();

        // Psalm 151 clean up
        if (bookLower == "psalm" && chapter == 151)
        {
            bookLower = "psalm 151";
            chapter = 1;
        }

        if (this.BookChapters.Contains(bookLower))
        {
            if (chapter > 1)
            {
                return new ChapterReference(book, --chapter);
            }
            else if (bookLower == this.BookNames.First())
            {
                return new ChapterReference();
            }
            else
            {
                string previousBook = this.BookNames[this.BookNames.IndexOf(bookLower) - 1];
                if (this.BookChapters[previousBook] is int chapters)
                {
                    return new ChapterReference(previousBook, chapters);
                }
            }
        }

        // Default to no previous chapter
        return new ChapterReference();
    }

    /// <summary>
    /// Determines if this canon has the specified book.
    /// </summary>
    /// <param name="bookName">The book name.</param>
    /// <returns><c>true</c> if this canon contains the book; otherwise, <c>false</c>.</returns>
    public bool HasBook(string bookName) => this.BookNames.Contains(bookName);

    /// <summary>
    /// Determines whether the specified book contains the specified chapter.
    /// </summary>
    /// <param name="book">The book name.</param>
    /// <param name="chapter">The chapter.</param>
    /// <returns>
    ///   <c>true</c> if this a is valid chapter in the specified book; otherwise, <c>false</c>.
    /// </returns>
    public bool IsValidChapter(string book, int chapter)
    {
        string bookLower = book.ToLowerInvariant();

        // Psalm 151 clean up
        if (bookLower == "psalm" && chapter == 151)
        {
            bookLower = "psalm 151";
            chapter = 1;
        }

        return this.BookChapters.Contains(bookLower) && this.BookChapters[bookLower] is int chapters && chapter <= chapters && chapter > 0;
    }
}
