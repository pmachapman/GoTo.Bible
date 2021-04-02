// -----------------------------------------------------------------------
// <copyright file="RenderPassageController.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;
    using GoToBible.Engine;
    using GoToBible.Model;
    using GoToBible.Web.Server.Models;
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
        /// The statistics database context.
        /// </summary>
        private readonly StatisticsContext? context;

        /// <summary>
        /// The renderer.
        /// </summary>
        private readonly Renderer renderer;

        /// <summary>
        /// Initialises a new instance of the <see cref="RenderPassageController" /> class.
        /// </summary>
        /// <param name="providers">The providers.</param>
        /// <param name="context">The statistics database context.</param>
        public RenderPassageController(IEnumerable<IProvider> providers, StatisticsContext? context = null)
        {
            this.context = context;
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
        public async Task<RenderedPassage> Post(RenderingParameters parameters, bool renderCompleteHtmlPage = false)
        {
            // If we can record statistics
            if (this.context != null)
            {
                try
                {
                    StatisticsContext context = this.context!;
                    Statistics statistics = new Statistics
                    {
                        AccessedAt = DateTime.UtcNow,
                        ForwardedFor = this.Request.Headers["HTTP_X_FORWARDED_FOR"].FirstOrDefault(),
                        IpAddress = this.Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Passage = parameters.PassageReference.Start,
                        PrimaryProvider = parameters.PrimaryProvider,
                        PrimaryTranslation = parameters.PrimaryTranslation,
                        SecondaryProvider = parameters.SecondaryProvider,
                        SecondaryTranslation = parameters.SecondaryTranslation,
                    };
                    await context.Statistics.AddAsync(statistics);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (ex is not DbException)
                    {
                        throw;
                    }
                }
            }

            return await this.renderer.RenderAsync(parameters, renderCompleteHtmlPage);
        }
    }
}
