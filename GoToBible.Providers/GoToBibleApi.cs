// -----------------------------------------------------------------------
// <copyright file="GoToBibleApi.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GoToBible.Model;
    using Microsoft.Extensions.Caching.Distributed;

    /// <summary>
    /// The GoTo.Bible API Provider.
    /// </summary>
    /// <seealso cref="GoToBible.Providers.ApiProvider" />
    /// <remarks>The GoTo.Bible API only provides translations via this class at this time.</remarks>
    public class GoToBibleApi : ApiProvider
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="GoToBibleApi" /> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        public GoToBibleApi(IDistributedCache cache)
            : base(cache)
        {
            this.HttpClient.BaseAddress = new Uri("https://goto.bible/", UriKind.Absolute);
        }

        /// <inheritdoc/>
        public override string Id => nameof(GoToBibleApi);

        /// <inheritdoc/>
        public override string Name => "GoTo.Bible API";

        /// <inheritdoc/>
        /// <remarks>This method is not implemented, and only for use with the GotoBibleApiRenderer.</remarks>
        public override async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
        {
            // TODO: Implement this for auto-correct
            await Task.CompletedTask;
            yield break;
        }

        /// <inheritdoc/>
        /// <remarks>This method is not implemented, and only for use with the GotoBibleApiRenderer.</remarks>
        public override async Task<Chapter> GetChapterAsync(string translation, string book, int chapterNumber) => await Task.FromResult(new Chapter());

        /// <inheritdoc/>
        public override async IAsyncEnumerable<Translation> GetTranslationsAsync()
        {
            string url = "Translations";
            string cacheKey = this.GetCacheKey(url);
            string json = await this.Cache.GetStringAsync(cacheKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                using HttpResponseMessage response = await this.HttpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync();
                    await this.Cache.SetStringAsync(cacheKey, json, CacheEntryOptions);
                }
                else
                {
                    Debug.Print($"{response.StatusCode} error in GoToBibleApi.GetTranslationsAsync()");
                    yield break;
                }
            }

            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            foreach (Translation translation in JsonSerializer.Deserialize<Translation[]>(json, options) ?? Array.Empty<Translation>())
            {
                yield return translation;
            }
        }
    }
}
