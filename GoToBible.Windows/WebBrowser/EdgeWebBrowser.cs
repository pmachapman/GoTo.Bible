// -----------------------------------------------------------------------
// <copyright file="EdgeWebBrowser.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows.WebBrowser
{
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Web.WebView2.WinForms;

    /// <summary>
    /// The Edge Web Browser Control.
    /// </summary>
    /// <seealso cref="Microsoft.Web.WebView2.WinForms.WebView2" />
    /// <seealso cref="GoToBible.Windows.WebBrowser.IWebBrowser" />
    public class EdgeWebBrowser : WebView2, IWebBrowser
    {
        /// <inheritdoc />
        public bool DeveloperMode
        {
            get => this.CoreWebView2.Settings.AreDevToolsEnabled;
            set => this.CoreWebView2.Settings.AreDevToolsEnabled = value;
        }

        /// <inheritdoc />
        public void Initialise(string name, int tabIndex)
        {
            this.CreationProperties = null;
            this.DefaultBackgroundColor = System.Drawing.Color.White;
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = name;
            this.Size = new System.Drawing.Size(100, 100);
            this.TabIndex = tabIndex;
            this.ZoomFactor = 1D;
        }

        /// <inheritdoc />
        public async Task SetInnerHtmlAsync(string htmlContent)
            => await this.ExecuteScriptAsync($"document.body.innerHTML=\"{HttpUtility.JavaScriptStringEncode(htmlContent)}\";");
    }
}
