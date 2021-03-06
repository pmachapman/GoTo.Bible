// -----------------------------------------------------------------------
// <copyright file="LegacyWebBrowser.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows.WebBrowser
{
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// The Internet Explorer Web Browser.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.WebBrowser" />
    /// <seealso cref="GoToBible.Windows.WebBrowser.IWebBrowser" />
    public class LegacyWebBrowser : WebBrowser, IWebBrowser
    {
        /// <inheritdoc/>
        public bool DeveloperMode
        {
            get => this.IsWebBrowserContextMenuEnabled;
            set => this.IsWebBrowserContextMenuEnabled = value;
        }

        /// <inheritdoc/>
        public Task SetInnerHtmlAsync(string innerHtml)
        {
            if (this.Document != null && this.Document.Body != null)
            {
                this.Document.Body.InnerHtml = innerHtml;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Initialise(string name, int tabIndex)
        {
            this.Dock = DockStyle.Fill;
            this.Location = new System.Drawing.Point(0, 0);
            this.MinimumSize = new System.Drawing.Size(20, 20);
            this.Name = name;
            this.Size = new System.Drawing.Size(100, 100);
            this.TabIndex = tabIndex;
        }

        /// <inheritdoc/>
        public void NavigateToString(string htmlContent)
        {
            if (this.Document != null)
            {
                this.Document.OpenNew(true);
                this.Document.Write(htmlContent);
            }
            else
            {
                this.DocumentText = htmlContent;
            }
        }
    }
}
