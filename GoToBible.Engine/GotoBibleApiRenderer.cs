// -----------------------------------------------------------------------
// <copyright file="GotoBibleApiRenderer.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using GoToBible.Model;

    /// <summary>
    /// The GoTo.Bible API Renderer.
    /// </summary>
    /// <seealso cref="GoToBible.Model.IRenderer" />
    public class GotoBibleApiRenderer : IRenderer
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
        /// Initialises a new instance of the <see cref="GotoBibleApiRenderer" /> class.
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
        /// Finalises an instance of the <see cref="GotoBibleApiRenderer"/> class.
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
        public async Task<RenderedPassage> RenderAsync(RenderingParameters parameters, bool renderCompleteHtmlPage)
        {
            string url = $"RenderPassage?renderCompleteHtmlPage={renderCompleteHtmlPage}";
            HttpResponseMessage response = await this.httpClient.PostAsJsonAsync(url, parameters);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RenderedPassage>() ?? new RenderedPassage();
            }
            else
            {
                return new RenderedPassage();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
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
}
