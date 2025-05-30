﻿// -----------------------------------------------------------------------
// <copyright file="IProvider.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The provider interface.
/// </summary>
public interface IProvider : IDisposable
{
    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    /// <value>
    /// The provider identifier.
    /// </value>
    string Id { get; }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    /// <value>
    /// The provider name.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Gets the books for a translation.
    /// </summary>
    /// <param name="translation">The translation code.</param>
    /// <param name="includeChapters">If set to <c>true</c> include chapters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The books.
    /// </returns>
    /// <remarks>
    /// Excluding chapters will often speed up the function.
    /// </remarks>
    IAsyncEnumerable<Book> GetBooksAsync(
        string translation,
        bool includeChapters,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a chapter from the bible.
    /// </summary>
    /// <param name="translation">The translation code.</param>
    /// <param name="book">The book name.</param>
    /// <param name="chapterNumber">The chapter number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The chapter for rendering.
    /// </returns>
    Task<Chapter> GetChapterAsync(
        string translation,
        string book,
        int chapterNumber,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a chapter from the bible.
    /// </summary>
    /// <param name="translation">The translation code.</param>
    /// <param name="chapterReference">The chapter reference.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The chapter for rendering.
    /// </returns>
    Task<Chapter> GetChapterAsync(
        string translation,
        ChapterReference chapterReference,
        CancellationToken cancellationToken = default
    ) =>
        this.GetChapterAsync(
            translation,
            chapterReference.Book,
            chapterReference.ChapterNumber,
            cancellationToken
        );

    /// <summary>
    /// Gets the translations available from the provider asynchronously.
    /// </summary>
    /// <returns>
    /// The available translations.
    /// </returns>
    IAsyncEnumerable<Translation> GetTranslationsAsync(
        CancellationToken cancellationToken = default
    );
}
