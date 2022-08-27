// -----------------------------------------------------------------------
// <copyright file="Error.cshtml.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Pages;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

/// <summary>
/// The error page model.
/// </summary>
/// <seealso cref="PageModel" />
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "This is a Razor page.")]
public class ErrorModel : PageModel
{
    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<ErrorModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorModel"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ErrorModel(ILogger<ErrorModel> logger) => this.logger = logger;

    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    /// <value>
    /// The request identifier.
    /// </value>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether we are to show the request identifier.
    /// </summary>
    /// <value>
    ///   <c>true</c> if we are to show the request identifier; otherwise, <c>false</c>.
    /// </value>
    public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);

    /// <summary>
    /// GET: <c>/Error</c>.
    /// </summary>
    /// ReSharper disable once UnusedMember.Global
    public void OnGet()
    {
        this.RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier;
        this.logger.LogError("Error in Request #{RequestId}", this.RequestId);
    }
}
