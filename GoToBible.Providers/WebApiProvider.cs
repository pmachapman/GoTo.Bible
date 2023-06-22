// -----------------------------------------------------------------------
// <copyright file="WebApiProvider.cs" company="PlaceholderCompany">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

/// <summary>
/// The API Provider base class.
/// </summary>
/// <seealso cref="ApiProvider" />
public abstract class WebApiProvider : ApiProvider
{
    /// <summary>
    /// A map of the Bible book codes to the Passage Reference book names.
    /// </summary>
    protected static readonly IReadOnlyDictionary<string, string> BookCodeMap = new Dictionary<string, string>
    {
        // Old Testament
        ["genesis"] = "GEN",
        ["exodus"] = "EXO",
        ["leviticus"] = "LEV",
        ["numbers"] = "NUM",
        ["deuteronomy"] = "DEU",
        ["joshua"] = "JOS",
        ["judges"] = "JDG",
        ["ruth"] = "RUT",
        ["1 samuel"] = "1SA",
        ["2 samuel"] = "2SA",
        ["1 kings"] = "1KI",
        ["2 kings"] = "2KI",
        ["1 chronicles"] = "1CH",
        ["2 chronicles"] = "2CH",
        ["ezra"] = "EZR",
        ["nehemiah"] = "NEH",
        ["esther"] = "EST",
        ["job"] = "JOB",
        ["psalm"] = "PSA",
        ["proverbs"] = "PRO",
        ["ecclesiastes"] = "ECC",
        ["song of solomon"] = "SNG",
        ["isaiah"] = "ISA",
        ["jeremiah"] = "JER",
        ["lamentations"] = "LAM",
        ["ezekiel"] = "EZK",
        ["daniel"] = "DAN",
        ["hosea"] = "HOS",
        ["joel"] = "JOL",
        ["amos"] = "AMO",
        ["obadiah"] = "OBA",
        ["jonah"] = "JON",
        ["micah"] = "MIC",
        ["nahum"] = "NAM",
        ["habakkuk"] = "HAB",
        ["zephaniah"] = "ZEP",
        ["haggai"] = "HAG",
        ["zechariah"] = "ZEC",
        ["malachi"] = "MAL",

        // Old Testament Apocrypha
        ["1 esdras"] = "1ES",
        ["2 esdras"] = "2ES",
        ["tobit"] = "TOB",
        ["judith"] = "JDT",
        ["esther (greek)"] = "ESG",
        ["wisdom"] = "WIS",
        ["ecclesiasticus"] = "SIR",
        ["baruch"] = "BAR",
        ["epistle of jeremy"] = "LJE",
        ["daniel (greek)"] = "DAG",
        ["song of the three"] = "S3Y",
        ["susanna"] = "SUS",
        ["bel and the dragon"] = "BEL",
        ["manasseh"] = "MAN",
        ["1 maccabees"] = "1MA",
        ["2 maccabees"] = "2MA",
        ["3 maccabees"] = "3MA",
        ["4 maccabees"] = "4MA",
        ["psalm 151"] = "PS2",

        // New Testament
        ["matthew"] = "MAT",
        ["mark"] = "MRK",
        ["luke"] = "LUK",
        ["john"] = "JHN",
        ["acts"] = "ACT",
        ["romans"] = "ROM",
        ["1 corinthians"] = "1CO",
        ["2 corinthians"] = "2CO",
        ["galatians"] = "GAL",
        ["ephesians"] = "EPH",
        ["philippians"] = "PHP",
        ["colossians"] = "COL",
        ["1 thessalonians"] = "1TH",
        ["2 thessalonians"] = "2TH",
        ["1 timothy"] = "1TI",
        ["2 timothy"] = "2TI",
        ["titus"] = "TIT",
        ["philemon"] = "PHM",
        ["hebrews"] = "HEB",
        ["james"] = "JAS",
        ["1 peter"] = "1PE",
        ["2 peter"] = "2PE",
        ["1 john"] = "1JN",
        ["2 john"] = "2JN",
        ["3 john"] = "3JN",
        ["jude"] = "JUD",
        ["revelation"] = "REV",

        // New Testament Apocrypha
        ["laodiceans"] = "LAO",
    };

    /// <summary>
    /// The cache entry options.
    /// </summary>
    protected static readonly DistributedCacheEntryOptions CacheEntryOptions = new DistributedCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromHours(24),
    };

    /// <summary>
    /// The reverse book code map.
    /// </summary>
    protected static readonly IReadOnlyDictionary<string, string> ReverseBookCodeMap = BookCodeMap.ToDictionary(x => x.Value, x => x.Key);

    /// <summary>
    /// A value indicating whether or not this instance has been disposed.
    /// </summary>
    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApiProvider" /> class.
    /// </summary>
    /// <param name="cache">The cache.</param>
    protected WebApiProvider(IDistributedCache cache)
    {
        this.Cache = cache;
        this.HttpClient.DefaultRequestHeaders.Accept.Clear();
        this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="WebApiProvider"/> class.
    /// </summary>
    /// <remarks>Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method</remarks>
    ~WebApiProvider() => this.Dispose(false);

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
    public override void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Deserializes the anonymous type.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="json">The json.</param>
    /// <param name="_">The anonymous type object.</param>
    /// <param name="options">The options.</param>
    /// <returns>The value.</returns>
    protected static TValue? DeserializeAnonymousType<TValue>(string json, TValue _, JsonSerializerOptions? options = default)
        => JsonSerializer.Deserialize<TValue>(json, options);

    /// <summary>
    /// Creates an empty list of an anonymous object.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="_">The anonymous type object.</param>
    /// <returns>
    /// An empty list of the anonymous object.
    /// </returns>
    protected static List<TValue> EmptyListOf<TValue>(TValue _) => new List<TValue>();

    /// <summary>
    /// Creates a nullable type of the specified anonymous object.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="_">The anonymous type object.</param>
    /// <returns>
    /// The anonymous object as a nullable type.
    /// </returns>
    protected static TValue? Nullable<TValue>(TValue _) => default;

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
}
