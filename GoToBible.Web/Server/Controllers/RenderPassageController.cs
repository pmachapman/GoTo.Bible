﻿// -----------------------------------------------------------------------
// <copyright file="RenderPassageController.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Controllers;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GoToBible.Engine;
using GoToBible.Model;
using GoToBible.Web.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// The render passage controller.
/// </summary>
/// <seealso cref="ControllerBase" />
[ApiController]
[Route("v1/[controller]")]
[Route("[controller]")]
public class RenderPassageController(
    IEnumerable<IProvider> providers,
    ILoggerFactory loggerFactory,
    StatisticsContext? context = null
) : ControllerBase
{
    /// <summary>
    /// The statistics database context.
    /// </summary>
    private readonly StatisticsContext? context = context;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger = loggerFactory.CreateLogger<RenderPassageController>();

    /// <summary>
    /// The renderer.
    /// </summary>
    private readonly Renderer renderer = new Renderer(providers);

    /// <summary>
    /// POST: <c>/v1/RenderPassage</c>.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="renderCompleteHtmlPage">If set to <c>true</c>, render the complete HTML page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The task containing an action result.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Post(
        RenderingParameters parameters,
        bool renderCompleteHtmlPage = false,
        CancellationToken cancellationToken = default
    )
    {
        // If we can record statistics
        if (this.context is not null)
        {
            try
            {
                StatisticsContext statisticsContext = this.context!;
                Statistics statistics = new Statistics
                {
                    AccessedAt = DateTime.UtcNow,
                    ForwardedFor = this.Request.Headers["HTTP_X_FORWARDED_FOR"].FirstOrDefault(),
                    IpAddress = this.Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Passage = parameters.PassageReference.ChapterReference.ToString(),
                    PrimaryProvider = parameters.PrimaryProvider,
                    PrimaryTranslation = parameters.PrimaryTranslation,
                    SecondaryProvider = parameters.SecondaryProvider,
                    SecondaryTranslation = parameters.SecondaryTranslation,
                };
                await statisticsContext.Statistics.AddAsync(statistics, cancellationToken);
                await statisticsContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is not DbException)
                {
                    throw;
                }
            }
        }

        try
        {
            return this.Ok(
                await this.renderer.RenderAsync(
                    parameters,
                    renderCompleteHtmlPage,
                    cancellationToken
                )
            );
        }
        catch (Exception ex)
        {
            // Log the URL, with details to help us debug
            RequestHeaders header = this.Request.GetTypedHeaders();
            string renderingParameters = JsonSerializer.Serialize(parameters);
            this.logger.LogError(
                ex,
                "URL: {DisplayUrl}\r\nReferer: {Referer}\r\nRenderingParameters: {RenderingParameters}",
                this.Request.GetDisplayUrl(),
                header.Referer,
                renderingParameters
            );
            return this.Problem();
        }
    }
}
