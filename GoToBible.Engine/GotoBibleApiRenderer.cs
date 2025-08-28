// -----------------------------------------------------------------------
// <copyright file="GotoBibleApiRenderer.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Engine;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;

/// <summary>
/// The GoTo.Bible API Renderer.
/// </summary>
/// <seealso cref="GoToBible.Model.IRenderer" />
public sealed class GotoBibleApiRenderer : IRenderer
{
    /// <summary>
    /// The HTTP client.
    /// </summary>
    private readonly HttpClient httpClient;

    /// <summary>
    /// A value indicating whether or not this instance has been disposed.
    /// </summary>
    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="GotoBibleApiRenderer" /> class.
    /// </summary>
    public GotoBibleApiRenderer()
    {
        this.Providers = new List<IProvider>();
        this.httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://goto.bible/", UriKind.Absolute),
        };
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="GotoBibleApiRenderer"/> class.
    /// </summary>
    /// <remarks>Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method</remarks>
    ~GotoBibleApiRenderer() => this.Dispose(false);

    /// <inheritdoc/>
    public IReadOnlyCollection<IProvider> Providers { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task<RenderedPassage> RenderAsync(
        RenderingParameters parameters,
        bool renderCompleteHtmlPage,
        CancellationToken cancellationToken = default
    )
    {
        string url = $"RenderPassage?renderCompleteHtmlPage={renderCompleteHtmlPage}";
        Debug.WriteLine($"POST: {this.httpClient.BaseAddress}{url}");
        HttpResponseMessage response = await this.httpClient.PostAsJsonAsync(
            url,
            parameters,
            cancellationToken
        );
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<RenderedPassage>(
                cancellationToken: cancellationToken
            ) ?? new RenderedPassage()
            : new RenderedPassage();
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                this.httpClient.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            this.disposedValue = true;
        }
    }
}
