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
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Web;
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
            this.HttpClient.BaseAddress = new Uri("https://api.goto.bible/v1/", UriKind.Absolute);
        }

        /// <inheritdoc/>
        public override string Id => nameof(GoToBibleApi);

        /// <inheritdoc/>
        public override string Name => "GoTo.Bible API";

        /// <inheritdoc/>
        public override async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
        {
            await foreach (Translation providerTranslation in this.GetTranslationsAsync())
            {
                if (providerTranslation.Code == translation)
                {
                    string urlProvider = HttpUtility.UrlEncode(providerTranslation.Provider);
                    string urlTranslation = HttpUtility.UrlEncode(providerTranslation.Code);
                    string url = $"Books?provider={urlProvider}&translation={urlTranslation}&includeChapters=false";
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
                            Debug.Print($"{response.StatusCode} error in GoToBibleApi.GetBooksAsync()");
                            yield break;
                        }
                    }

                    JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    foreach (Book book in JsonSerializer.Deserialize<Book[]>(json, options) ?? Array.Empty<Book>())
                    {
                        yield return book;
                    }

                    yield break;
                }
            }
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
