﻿// -----------------------------------------------------------------------
// <copyright file="NltBible.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

/// <summary>
/// The NLT Bible Provider.
/// </summary>
/// <seealso cref="WebApiProvider" />
public class NltBible : WebApiProvider
{
    /// <summary>
    /// The NLT copyright message.
    /// </summary>
    private const string NltCopyright =
        "Scripture quotations are taken from the Holy Bible, New Living Translation, copyright &copy;1996, 2004, 2015 by Tyndale House Foundation. Used by permission of Tyndale House Publishers, Carol Stream, Illinois 60188. All rights reserved.";

    /// <summary>
    /// The NTV copyright message.
    /// </summary>
    private const string NtvCopyright =
        "El texto bíblico indicado con NTV ha sido tomado de la Santa Biblia, Nueva Traducción Viviente, &copy; Tyndale House Foundation, 2010. Usado con permiso de Tyndale House Publishers, Inc., Carol Stream, IL 60188, Estados Unidos de América. Todos los derechos reservados.";

    /// <summary>
    /// The canon.
    /// </summary>
    private static readonly BookHelper Canon = new ProtestantCanon();

    /// <summary>
    /// The options.
    /// </summary>
    private readonly NltBibleOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NltBible" /> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cache">The cache.</param>
    public NltBible(IOptions<NltBibleOptions> options, IDistributedCache cache)
        : base(cache)
    {
        this.options = options.Value;
        this.HttpClient.BaseAddress = new Uri("http://api.nlt.to/api/", UriKind.Absolute);
    }

    /// <inheritdoc/>
    public override string Id => nameof(NltBible);

    /// <inheritdoc/>
    public override string Name => "NLT API";

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Book> GetBooksAsync(
        string translation,
        bool includeChapters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (Book book in Canon.GetBooks(includeChapters))
        {
            yield return await Task.FromResult(book);
        }
    }

    /// <inheritdoc/>
    public override async Task<Chapter> GetChapterAsync(
        string translation,
        string book,
        int chapterNumber,
        CancellationToken cancellationToken = default
    )
    {
        // Set up the chapter
        Chapter chapter = new Chapter
        {
            Book = book,
            ChapterNumber = chapterNumber,
            NextChapterReference = new ChapterReference(),
            PreviousChapterReference = new ChapterReference(),
            SupportsItalics = true,
            Translation = translation,
        };

        // Clean input
        string queryBook = book;
        if (string.Equals(queryBook, "SONG OF SOLOMON", StringComparison.OrdinalIgnoreCase))
        {
            queryBook = "Song of Songs";
        }

        // Load the book
        string url =
            $"passages?ref={queryBook}+{chapterNumber}&key={this.options.ApiKey}&version={translation}";
        string cacheKey = this.GetCacheKey(url);
        string? html = await this.Cache.GetStringAsync(cacheKey, cancellationToken);

        // The NLT will return the first chapter for any invalid references
        if (!Canon.IsValidChapter(book, chapterNumber))
        {
            return chapter;
        }

        if (string.IsNullOrWhiteSpace(html))
        {
            Debug.WriteLine($"GET: {this.HttpClient.BaseAddress}{url}");
            using HttpResponseMessage response = await this.HttpClient.GetAsync(
                url,
                cancellationToken
            );
            if (response.IsSuccessStatusCode)
            {
                html = await response.Content.ReadAsStringAsync(cancellationToken);
                await this.Cache.SetStringAsync(
                    cacheKey,
                    html,
                    CacheEntryOptions,
                    cancellationToken
                );
            }
            else
            {
                Debug.WriteLine($"{response.StatusCode} error in NltBible.GetChapterAsync()");
                return chapter;
            }
        }

        if (!string.IsNullOrWhiteSpace(html))
        {
            // Clean up the HTML
            StringBuilder sb = new StringBuilder();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Strip out content we do not want
            bool endItalics = false;
            StringBuilder strippedNode = new StringBuilder();
            foreach (
                HtmlNode node in doc
                    .DocumentNode.SelectNodes(
                        "//span[@class='vn']|//span[@class='tn']|//p[@class='psa-hebrew']|//hr[@class='text-critical']|//p[@class='text-critical']|//p[@class='psa-title']|//p[@class='chapter-number']|//p[@class='subhead']|//a[@class='a-tn']|//p[@class='poet1']|//p[@class='poet2']|//p[@class='sos-speaker']|//p[@class='selah']|//h1|//h2|//h3|//h4"
                    )?
                    .ToArray() ?? []
            )
            {
                strippedNode.Clear();

                // Fix any unusual nodes
                if (
                    node.Name == "hr"
                    && string.Equals(book, "MARK", StringComparison.OrdinalIgnoreCase)
                )
                {
                    // This is for the shorter ending in Mark
                    endItalics = true;
                    strippedNode.Append(" [");
                }
                else if (node.HasClass("selah"))
                {
                    // Show "Interlude." in italics
                    foreach (
                        HtmlNode innerNode in node.ChildNodes.Where(n =>
                            n.NodeType == HtmlNodeType.Text
                        )
                    )
                    {
                        // Strip any HTML nodes (i.e. footnotes)
                        strippedNode.Append('[');
                        strippedNode.Append(innerNode.InnerText.Trim());
                        strippedNode.Append(']');
                    }
                }
                else if (node.HasClass("poet1") || node.HasClass("poet2"))
                {
                    // This is for the Song of Solomon poetry lines
                    foreach (
                        HtmlNode innerNode in node.ChildNodes.Where(n =>
                            n.NodeType == HtmlNodeType.Text || n.HasClass("sc")
                        )
                    )
                    {
                        // Strip any HTML nodes (i.e. footnotes)
                        strippedNode.Append(' ');
                        strippedNode.Append(innerNode.InnerText.Trim());
                        strippedNode.Append(' ');
                    }
                }

                HtmlTextNode replacement = doc.CreateTextNode(strippedNode.ToString());
                node.ParentNode.ReplaceChild(replacement, node);
            }

            // Output the nodes
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//verse_export")?.ToArray() ?? [])
            {
                string verse = node.Attributes["vn"].Value;
                string text = node.InnerText.NormaliseLineEndings().Replace("\n", string.Empty);
                if (endItalics)
                {
                    text += "]";
                }

                sb.AppendLine(verse + "  " + text);
            }

            chapter.Text = sb.ToString();
            chapter.PreviousChapterReference = Canon.GetPreviousChapter(book, chapterNumber);
            chapter.NextChapterReference = Canon.GetNextChapter(book, chapterNumber);
            return chapter;
        }
        else
        {
            return chapter;
        }
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Translation> GetTranslationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (!string.IsNullOrWhiteSpace(this.options.ApiKey))
        {
            yield return await Task.FromResult(
                new Translation
                {
                    Code = "NLT",
                    Copyright = NltCopyright,
                    Language = "English",
                    Name = "New Living Translation", // (American English)
                    Provider = this.Id,
                    Year = 2015,
                }
            );

            // The next two translations do not have the verse_export tags
            /*
            yield return await Task.FromResult(new Translation
            {
                Code = "NLTUK",
                Copyright = NltCopyright,
                Language = "English",
                Name = "New Living Translation (British English)",
                Provider = this.Id,
                Year = 2015,
            });
            yield return await Task.FromResult(new Translation
            {
                Code = "KJV",
                Copyright = "Public Domain",
                Language = "English",
                Name = "King James Version",
                Provider = this.Id,
                Year = 1769,
            });*/
            yield return await Task.FromResult(
                new Translation
                {
                    Code = "NTV",
                    Copyright = NtvCopyright,
                    Language = "Spanish",
                    Name = "Nueva Traducción Viviente",
                    Provider = this.Id,
                    Year = 2010,
                }
            );
        }
    }
}
