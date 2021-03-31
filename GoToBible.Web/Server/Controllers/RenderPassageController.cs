// -----------------------------------------------------------------------
// <copyright file="RenderPassageController.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GoToBible.Engine;
    using GoToBible.Model;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The render passage controller.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Route("[controller]")]
    public class RenderPassageController : ControllerBase
    {
        /// <summary>
        /// The renderer.
        /// </summary>
        private readonly Renderer renderer;

        /// <summary>
        /// Initialises a new instance of the <see cref="RenderPassageController" /> class.
        /// </summary>
        /// <param name="providers">The providers.</param>
        public RenderPassageController(IEnumerable<IProvider> providers)
        {
            // TODO: Store statistics for each translations and provider
            this.renderer = new Renderer(providers);
        }

        /// <summary>
        /// POST: <c>/RenderPassage</c>.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="renderCompleteHtmlPage">If set to <c>true</c>, render the complete HTML page.</param>
        /// <returns>
        /// The task.
        /// </returns>
        [HttpPost]
        public async Task<RenderedPassage> Post(RenderingParameters parameters, bool renderCompleteHtmlPage = false) => await this.renderer.RenderAsync(parameters, renderCompleteHtmlPage);
    }
}
