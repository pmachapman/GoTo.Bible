// -----------------------------------------------------------------------
// <copyright file="LocalResourceProvider.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using GoToBible.Model;
using Microsoft.Extensions.Options;

/// <summary>
/// The Local Resource Provider.
/// </summary>
/// <seealso cref="ApiProvider" />
public abstract class LocalResourceProvider : ApiProvider
{
    /// <summary>
    /// Gets a value indicating whether or not the path to the Resource Directory is valid.
    /// </summary>
    /// <value><c>true</c> if the resource directory path is valid; otherwise, <c>false</c>.</value>
    private readonly bool isValidPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalResourceProvider"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentException">Invalid Resource Directory - options.</exception>
    protected LocalResourceProvider(IOptions<LocalResourceOptions> options)
    {
        // Set the options
        this.Options = options.Value;

        // Check the path
        this.isValidPath =
            Directory.Exists(this.Options.Directory)
            && File.Exists(Path.Combine(this.Options.Directory, "index.csv"));
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Translation> GetTranslationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (this.isValidPath)
        {
            using StreamReader reader = new StreamReader(
                Path.Combine(this.Options.Directory, "index.csv")
            );
            using CsvReader csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            bool initialiseCache = this.Translations.Count == 0;
            await foreach (
                LocalTranslation translation in csvReader.GetRecordsAsync<LocalTranslation>(
                    cancellationToken
                )
            )
            {
                if (translation.Provider == this.Id)
                {
                    translation.Provider = this.Id;
                    if (initialiseCache)
                    {
                        this.Translations.Add(translation);
                    }

                    yield return translation;
                }
            }
        }
    }

    /// <summary>
    /// Gets the Scripture cache.
    /// </summary>
    /// <value>The Scripure cache.</value>
    protected ConcurrentDictionary<string, Chapter> Cache { get; } =
        new ConcurrentDictionary<string, Chapter>();

    /// <summary>
    /// Gets the local resource options.
    /// </summary>
    /// <remarks>The options.</remarks>
    protected LocalResourceOptions Options { get; }

    /// <summary>
    /// Gets the translations.
    /// </summary>
    /// <value>The translations.</value>
    protected List<LocalTranslation> Translations { get; } = [];

    /// <summary>
    /// Ensures the translations are cached asynchronously.
    /// </summary>
    /// <returns>
    /// The task.
    /// </returns>
    protected async Task EnsureTranslationsAreCachedAsync(
        CancellationToken cancellationToken = default
    )
    {
        // Make sure we have the translations cache set up
        if (this.Translations.Count == 0)
        {
            // This is just so we can have some async code to cancel the error
            await foreach (Translation? _ in this.GetTranslationsAsync(cancellationToken)) { }
        }
    }
}
