// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.

namespace GoToBible.Providers;

using GoToBible.Client;
using Microsoft.Extensions.Caching.Distributed;
using System;

/// <summary>
/// An API Provider that supports caching reponses.
/// </summary>
/// <see cref="WebApiProvider"/>
public abstract class CachedApiProvider : WebApiProvider
{
    /// <summary>
    /// The cache entry options.
    /// </summary>
    protected static readonly DistributedCacheEntryOptions CacheEntryOptions =
        new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(24) };

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedApiProvider" /> class.
    /// </summary>
    /// <param name="cache">The cache.</param>
    protected CachedApiProvider(IDistributedCache cache) => this.Cache = cache;

    /// <summary>
    /// Gets the request cache.
    /// </summary>
    /// <value>
    /// The request cache.
    /// </value>
    protected IDistributedCache Cache { get; }

    /// <summary>
    /// Gets the cache key.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>
    /// The cache key.
    /// </returns>
    protected string GetCacheKey(string url) => this.HttpClient.BaseAddress + url;
}
