// -----------------------------------------------------------------------
// <copyright file="IRenderer.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// The renderer interface.
    /// </summary>
    public interface IRenderer : IDisposable
    {
        /// <summary>
        /// Gets or sets the providers.
        /// </summary>
        /// <value>
        /// The providers.
        /// </value>
        /// <remarks>
        /// These can be all disposed by the renderer.
        /// </remarks>
        IReadOnlyCollection<IProvider> Providers { get; set; }

        /// <summary>
        /// Renders the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="renderCompleteHtmlPage">If set to <c>true</c>, render the complete HTML page.</param>
        /// <returns>
        /// The output of the rendering.
        /// </returns>
        Task<RenderedPassage> RenderAsync(RenderingParameters parameters, bool renderCompleteHtmlPage);
    }
}
