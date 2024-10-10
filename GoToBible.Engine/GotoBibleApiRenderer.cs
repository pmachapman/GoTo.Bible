// -----------------------------------------------------------------------
// <copyright file="GotoBibleApiRenderer.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Model;

/// <summary>
/// The GoTo.Bible API Renderer.
/// </summary>
/// <seealso cref="GoToBible.Engine.Renderer" />
/// <seealso cref="GoToBible.Model.IRenderer" />
public sealed class GotoBibleApiRenderer : Renderer
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
    public GotoBibleApiRenderer() => this.httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://goto.bible/", UriKind.Absolute),
    };

    /// <summary>
    /// Finalizes an instance of the <see cref="GotoBibleApiRenderer"/> class.
    /// </summary>
    /// <remarks>Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method</remarks>
    ~GotoBibleApiRenderer() => this.Dispose(false);

    /// <inheritdoc/>
    public override async Task<RenderedPassage> RenderAsync(
        RenderingParameters parameters,
        bool renderCompleteHtmlPage,
        CancellationToken cancellationToken = default
    )
    {
        // If one of the translations requires local rendering, they both must be rendered locally
        if (this.Providers.Any(p => (p.Id == parameters.PrimaryProvider || p.Id == parameters.SecondaryProvider) && p.LocalOnly))
        {
            return await base.RenderAsync(parameters, renderCompleteHtmlPage, cancellationToken);
        }

        string url = $"RenderPassage?renderCompleteHtmlPage={renderCompleteHtmlPage}";
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
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
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
