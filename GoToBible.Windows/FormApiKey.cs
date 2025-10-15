// -----------------------------------------------------------------------
// <copyright file="FormApiKey.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

/// <summary>
/// The Enter API Key Form.
/// </summary>
/// <seealso cref="Form" />
[SupportedOSPlatform("windows")]
public partial class FormApiKey : Form
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormApiKey" /> class.
    /// </summary>
    /// <param name="key">The existing key.</param>
    /// <param name="provider">The provider.</param>
    /// <param name="signUpUrl">The sign up URL.</param>
    /// <param name="icon">The form icon.</param>
    public FormApiKey(string key, string provider, Uri signUpUrl, Icon icon)
    {
        this.InitializeComponent();
        this.Icon = icon;
        this.Key = key;
        this.Provider = provider;
        this.SignUpUrl = signUpUrl;
    }

    /// <summary>
    /// Gets the API key.
    /// </summary>
    /// <value>
    /// The API key.
    /// </value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Key { get; private set; }

    /// <summary>
    /// Gets the API provider name.
    /// </summary>
    /// <value>
    /// The API provider name.
    /// </value>
    public string Provider { get; }

    /// <summary>
    /// Gets the API key sign up URL.
    /// </summary>
    /// <value>
    /// The API key sign up URL.
    /// </value>
    public Uri SignUpUrl { get; }

    /// <summary>
    /// Handles the Click event of the Cancel Button.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    /// <summary>
    /// Handles the Click event of the Ok Button.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonOk_Click(object sender, EventArgs e)
    {
        this.Key = this.TextBoxKey.Text;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    /// <summary>
    /// Handles the Load event of the BibleApi Form.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void FormBibleApi_Load(object sender, EventArgs e)
    {
        if (this.Provider.Contains("SQL", StringComparison.InvariantCulture))
        {
            this.Text = $@"Enter {this.Provider} Connection String";
            this.LabelEnterKey.Text = $@"Enter your {this.Provider} connection string below.";
            this.LinkLabelSignup.Text = $@"&Download {this.Provider}";
        }
        else
        {
            this.Text = $@"Enter {this.Provider} Key";
            this.LabelEnterKey.Text = this.Provider.Length switch
            {
                > 15 => $@"Enter your {this.Provider} key below to use your resources.",
                > 10
                    => $@"Enter your {this.Provider} key below to use your resources with GoToBible.",
                _
                    => $@"Enter your {this.Provider} key below to use your {this.Provider} resources with GoToBible.",
            };

            this.LinkLabelSignup.Text = @"&Sign up for an API key";
        }

        this.TextBoxKey.Text = this.Key;
    }

    /// <summary>
    /// Handles the LinkClicked event of the LinkLabelSignup control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
    private void LinkLabelSignup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) =>
        Process.Start(
            new ProcessStartInfo(this.SignUpUrl.ToString())
            {
                UseShellExecute = true,
                Verb = "open",
            }
        );
}
