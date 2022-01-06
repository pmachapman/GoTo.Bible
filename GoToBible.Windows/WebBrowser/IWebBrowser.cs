// -----------------------------------------------------------------------
// <copyright file="IWebBrowser.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows.WebBrowser
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// The web browser interface.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IWebBrowser : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether we are in developer mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if in developer mode; otherwise, <c>false</c>.
        /// </value>
        bool DeveloperMode { get; set; }

        /// <summary>
        /// Sets the HTML document's inner HTML asynchronously.
        /// </summary>
        /// <param name="htmlContent">The HTML content.</param>
        /// <returns>The task.</returns>
        Task SetInnerHtmlAsync(string htmlContent);

        /// <summary>
        /// Initialises the controle.
        /// </summary>
        /// <param name="name">The control name.</param>
        /// <param name="tabIndex">The control tab index.</param>
        void Initialise(string name, int tabIndex);

        /// <summary>
        /// Navigates to specific string.
        /// </summary>
        /// <param name="htmlContent">The HTML content.</param>
        void NavigateToString(string htmlContent);
    }
}
