// -----------------------------------------------------------------------
// <copyright file="EmptyWebBrowser.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows.WebBrowser;

using System;
using System.Threading.Tasks;

/// <summary>
/// An empty web browser implementation.
/// </summary>
/// <seealso cref="GoToBible.Windows.WebBrowser.IWebBrowser" />
public class EmptyWebBrowser : IWebBrowser
{
    /// <inheritdoc/>
    public bool DeveloperMode { get; set; }

    /// <inheritdoc/>
    public void Dispose() => GC.SuppressFinalize(this);

    /// <inheritdoc/>
    public void Initialise(string name, int tabIndex)
    {
    }

    /// <inheritdoc/>
    public void NavigateToString(string htmlContent)
    {
    }

    /// <inheritdoc/>
    public Task SetInnerHtmlAsync(string htmlContent) => Task.CompletedTask;
}
