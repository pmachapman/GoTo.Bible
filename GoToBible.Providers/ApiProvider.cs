// -----------------------------------------------------------------------
// <copyright file="ApiProvider.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GoToBible.Model;
    using Microsoft.Extensions.Caching.Distributed;

    /// <summary>
    /// The API Provider base class.
    /// </summary>
    /// <seealso cref="IProvider" />
    public abstract class ApiProvider : IProvider
    {
        /// <summary>
        /// The cache entry options.
        /// </summary>
        protected static readonly DistributedCacheEntryOptions CacheEntryOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromDays(365),
        };

        /// <summary>
        /// A value indicating whether or not this instance has been disposed.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Initialises a new instance of the <see cref="ApiProvider" /> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        protected ApiProvider(IDistributedCache cache)
        {
            this.Cache = cache;
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Finalises an instance of the <see cref="ApiProvider"/> class.
        /// </summary>
        ~ApiProvider()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public abstract string Id { get; }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract bool SupportsItalics { get; }

        /// <summary>
        /// Gets the request cache.
        /// </summary>
        /// <value>
        /// The request cache.
        /// </value>
        protected IDistributedCache Cache { get; }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <value>
        /// The HTTP client.
        /// </value>
        protected HttpClient HttpClient { get; } = new HttpClient();

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters);

        /// <inheritdoc/>
        public abstract Task<Chapter> GetChapterAsync(string translation, string book, int chapterNumber);

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<Translation> GetTranslationsAsync();

        /// <summary>
        /// Deserializes the anonymous type.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="json">The json.</param>
        /// <param name="anonymousTypeObject">The anonymous type object.</param>
        /// <param name="options">The options.</param>
        /// <returns>The value.</returns>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The parameter is required for the type.")]
        protected static TValue? DeserializeAnonymousType<TValue>(string json, TValue anonymousTypeObject, JsonSerializerOptions? options = default)
            => JsonSerializer.Deserialize<TValue>(json, options);

        /// <summary>
        /// Creates an empty list of an anonymous object.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="anonymousTypeObject">The anonymous type object.</param>
        /// <returns>
        /// An empty list of the anonymous object.
        /// </returns>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The parameter is required for the type.")]
        protected static List<TValue> EmptyListOf<TValue>(TValue anonymousTypeObject) => new List<TValue>();

        /// <summary>
        /// Creates a nullable type of the specified anonymous object.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="anonymousTypeObject">The anonymous type object.</param>
        /// <returns>
        /// The anonymous object as a nullable type.
        /// </returns>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The parameter is required for the type.")]
        protected static TValue? Nullable<TValue>(TValue anonymousTypeObject) => default;

        /// <inheritdoc cref="IDisposable" />
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    this.HttpClient.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        /// The cache key.
        /// </returns>
        protected string GetCacheKey(string url)
            => this.HttpClient.BaseAddress + url;

        /// <summary>
        /// Gets all of the books and chapters in a translation asynchronously.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <returns>
        /// The list of chapters.
        /// </returns>
        protected async IAsyncEnumerable<string> GetChaptersAsync(string translation)
        {
            await foreach (Book book in this.GetBooksAsync(translation, true))
            {
                foreach (ChapterReference chapter in book.Chapters)
                {
                    yield return chapter.ToString();
                }
            }
        }
    }
}
