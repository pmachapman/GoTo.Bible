// -----------------------------------------------------------------------
// <copyright file="FormMain.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Windows.Forms;
    using GoToBible.Engine;
    using GoToBible.Model;
    using GoToBible.Providers;
    using GoToBible.Windows.AutoComplete;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Caching.SqlServer;
    using Microsoft.Extensions.Options;
    using Microsoft.Web.WebView2.WinForms;

    /// <summary>
    /// The primary form.
    /// </summary>
    /// <seealso cref="Form" />
    [SupportedOSPlatform("windows")]
    public partial class FormMain : Form
    {
        /// <summary>
        /// The commentaries.
        /// </summary>
        private readonly List<Translation> commentaries = new List<Translation>();

        /// <summary>
        /// Whether or not this is the primary window.
        /// </summary>
        private readonly bool primaryWindow;

        /// <summary>
        /// The providers.
        /// </summary>
        private readonly List<IProvider> providers = new List<IProvider>();

        /// <summary>
        /// The bible data renderer.
        /// </summary>
        private readonly Renderer renderer = new Renderer();

        /// <summary>
        /// The translations.
        /// </summary>
        private readonly List<Translation> translations = new List<Translation>();

        /// <summary>
        /// The cache.
        /// </summary>
        private IDistributedCache cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

        /// <summary>
        /// The rendered passage.
        /// </summary>
        private RenderedPassage renderedPassage = new RenderedPassage();

        /// <summary>
        /// The rendering parameters.
        /// </summary>
        private RenderingParameters parameters = new RenderingParameters();

        /// <summary>
        /// A value indicating whether or not the translations are loaded.
        /// </summary>
        private bool translationsLoaded = false;

        /// <summary>
        /// A value indicating whether or not the web view is initialised.
        /// </summary>
        private bool webViewInitialised = false;

        /// <summary>
        /// Initialises a new instance of the <see cref="FormMain" /> class.
        /// </summary>
        /// <param name="primaryWindow">If set to <c>true</c>, this is the primary window, which means settings will be saved.</param>
        public FormMain(bool primaryWindow)
        {
            // Specify primary window
            this.primaryWindow = primaryWindow;

            // Setup the GUI via the designer file
            this.InitializeComponent();

            // Clean up the toolstrips
            this.ToolStripContainerMain.SuspendLayout();
            this.ToolStripNavigate.Dock = DockStyle.None;
            this.ToolStripNavigate.Location = new Point(0, 0);
            this.ToolStripPassage.Dock = DockStyle.None;
            this.ToolStripPassage.Location = new Point(this.ToolStripNavigate.Left + this.ToolStripNavigate.Width, 0);
            this.ToolStripTranslation.Dock = DockStyle.None;
            this.ToolStripTranslation.Location = new Point(this.ToolStripPassage.Left + this.ToolStripPassage.Width, 0);
            this.ToolStripResource.Dock = DockStyle.None;
            this.ToolStripResource.Location = new Point(this.ToolStripTranslation.Left + this.ToolStripTranslation.Width, 0);
            this.ToolStripSettings.Dock = DockStyle.None;
            this.ToolStripSettings.Location = new Point(this.ToolStripResource.Left + this.ToolStripResource.Width, 0);
            this.ToolStripContainerMain.ResumeLayout();

            // Set initial button states
            this.ToolStripButtonNavigateBack.Enabled = false;
            this.ToolStripButtonNavigateForward.Enabled = false;
            this.ToolStripMenuItemConfigure.Enabled = false;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><c>true</c> if managed resources should be disposed; otherwise, <c>false</c>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
                this.renderer.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Handles the FormClosing event of the Main Form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs" /> instance containing the event data.</param>
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If we are the primary window
            if (this.primaryWindow || Program.Forms.Count == 1)
            {
                // Save the window state
                Properties.Settings.Default.SplitterDistance = this.SplitContainerMain.SplitterDistance;
                Properties.Settings.Default.WindowState = (int)this.WindowState;
                if (this.WindowState == FormWindowState.Normal)
                {
                    Properties.Settings.Default.WindowLocation = this.Location;
                    Properties.Settings.Default.WindowSize = this.Size;
                }
                else
                {
                    Properties.Settings.Default.WindowLocation = this.RestoreBounds.Location;
                    Properties.Settings.Default.WindowSize = this.RestoreBounds.Size;
                }

                Properties.Settings.Default.Save();
            }

            // Remove this form from the program list
            Program.Forms.Remove(this);

            // Disable autocomplete
            AutoSuggest.Disable(this.ToolStripTextBoxPassage.TextBox);

            // If there are no forms left, exit
            if (!Program.Forms.Any())
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Handles the Load event of the Main Form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void FormMain_Load(object sender, EventArgs e)
        {
            // Restore the window state, size, and location if this is the primary
            if (this.primaryWindow)
            {
                this.WindowState = (FormWindowState)Properties.Settings.Default.WindowState;
                this.Location = Properties.Settings.Default.WindowLocation;
                this.Size = Properties.Settings.Default.WindowSize;
            }

            // Set up the splitter
            int splitterDistance = Properties.Settings.Default.SplitterDistance;
            if (splitterDistance == 0)
            {
                splitterDistance = this.Width - (this.Width / 3);
            }

            this.SplitContainerMain.SplitterDistance = splitterDistance;

            // Get the colours and font
            this.ColourDialogMain.Color = ColorTranslator.FromOle(Properties.Settings.Default.BackgroundColour);
            this.ColourDialogMain.CustomColors = Properties.Settings.Default.CustomColours
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => int.TryParse(c, out int i) ? i : 16777215)
                .ToArray();
            this.FontDialogMain.Color = ColorTranslator.FromOle(Properties.Settings.Default.ForegroundColour);
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
            if (converter.ConvertFromString(Properties.Settings.Default.Font) is Font font)
            {
                this.FontDialogMain.Font = font;
            }
            else
            {
                this.FontDialogMain.Font = Default.Font.AsFont();
            }

            // Setup the rendering parameters for CSS
            this.parameters = new RenderingParameters
            {
                BackgroundColour = this.ColourDialogMain.Color,
                Font = this.FontDialogMain.Font.AsRenderFont(),
                ForegroundColour = this.FontDialogMain.Color,
            };

            // Initialise the WebView2
            await this.InitialiseAsync();

            // Get the translation and passage settings
            string primaryTranslation = Properties.Settings.Default.PrimaryTranslation;
            string secondaryTranslation = Properties.Settings.Default.SecondaryTranslation;
            string resource = Properties.Settings.Default.Resource;
            string passage = Properties.Settings.Default.Passage;
            if (string.IsNullOrWhiteSpace(passage))
            {
                passage = Default.Passage;
            }

            // Setup the toolstrip controls
            this.ToolStripComboBoxPrimaryTranslation.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.ToolStripComboBoxPrimaryTranslation.ComboBox.DrawItem += new DrawItemEventHandler(this.ToolStripComboBox_DrawItem);
            this.ToolStripComboBoxPrimaryTranslation.ComboBox.DropDownClosed += new EventHandler(this.ToolStripComboBox_DropDownClosed);
            this.ToolStripComboBoxSecondaryTranslation.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.ToolStripComboBoxSecondaryTranslation.ComboBox.DrawItem += new DrawItemEventHandler(this.ToolStripComboBox_DrawItem);
            this.ToolStripComboBoxSecondaryTranslation.ComboBox.DropDownClosed += new EventHandler(this.ToolStripComboBox_DropDownClosed);
            this.ToolStripComboBoxResource.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.ToolStripComboBoxResource.ComboBox.DrawItem += new DrawItemEventHandler(this.ToolStripComboBox_DrawItem);
            this.ToolStripComboBoxResource.ComboBox.DropDownClosed += new EventHandler(this.ToolStripComboBox_DropDownClosed);
            this.ToolStripMenuItemIgnoreCase.Checked = Properties.Settings.Default.InterlinearIgnoresCase;
            this.ToolStripMenuItemIgnoreDiacritics.Checked = Properties.Settings.Default.InterlinearIgnoresDiacritics;
            this.ToolStripMenuItemIgnorePunctuation.Checked = Properties.Settings.Default.InterlinearIgnoresPunctuation;
            this.ToolStripMenuItemShowItalics.Checked = Properties.Settings.Default.RenderItalics;
            this.ToolStripMenuItemDebugMode.Checked = Properties.Settings.Default.IsDebug;

            // Setup the cache
            this.LoadSqlCache(Properties.Settings.Default.SqlConnectionString);

            // Set up the translation combo boxes
            await this.LoadTranslationComboBoxes(this.LoadProviders(), primaryTranslation, secondaryTranslation, resource);

            // Set up the autocomplete
            await this.SetupAutoCompleteAsync();

            // Start on the default passage, if it is blank
            if (string.IsNullOrWhiteSpace(this.ToolStripTextBoxPassage.Text))
            {
                this.ToolStripTextBoxPassage.Text = passage;
                await this.ShowPassage(true, true);
            }
        }

        /// <summary>
        /// Handles the Shown event of the Main Form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void FormMain_Shown(object sender, EventArgs e)
        {
            if (this.webViewInitialised && !string.IsNullOrWhiteSpace(this.renderedPassage.Content))
            {
                await this.WebViewMain.ExecuteScriptAsync($"document.body.innerHTML=\"{HttpUtility.JavaScriptStringEncode(Html.LoadingCodeBody)}\";");
                await this.WebViewResource.ExecuteScriptAsync($"document.body.innerHTML=\"{HttpUtility.JavaScriptStringEncode(Html.LoadingCodeBody)}\";");
            }
        }

        /// <summary>
        /// Initialises the WebView2 asynchronously.
        /// </summary>
        private async Task InitialiseAsync()
        {
            await this.WebViewMain.EnsureCoreWebView2Async();
            this.WebViewMain.CoreWebView2.Settings.AreDevToolsEnabled = Properties.Settings.Default.IsDebug;
            this.SetupWebPage(this.WebViewMain);
            await this.WebViewResource.EnsureCoreWebView2Async();
            this.WebViewResource.CoreWebView2.Settings.AreDevToolsEnabled = Properties.Settings.Default.IsDebug;
            this.SetupWebPage(this.WebViewResource);

            this.webViewInitialised = true;
        }

        /// <summary>
        /// Loads the providers.
        /// </summary>
        /// <returns>
        /// The providers that were loaded.
        /// </returns>
        private IList<IProvider> LoadProviders()
        {
            // Clear the providers
            this.providers.Clear();

            // Load the API.Bible Provider
            string bibleApiKey = Properties.Settings.Default.BibleApiKey;
            if (!string.IsNullOrWhiteSpace(bibleApiKey))
            {
                this.providers.Add(new BibleApi(Options.Create(new BibleApiOptions { ApiKey = bibleApiKey }), this.cache));
            }

            // Load the Biblia Provider
            string bibliaApiKey = Properties.Settings.Default.BibliaApiKey;
            if (!string.IsNullOrWhiteSpace(bibliaApiKey))
            {
                this.providers.Add(new BibliaApi(Options.Create(new BibliaApiOptions { ApiKey = bibliaApiKey }), this.cache));
            }

            // Load the Digital Bible Platform Provider
            string digitalBiblePlatformApiKey = Properties.Settings.Default.DigitalBiblePlatformApiKey;
            if (!string.IsNullOrWhiteSpace(digitalBiblePlatformApiKey))
            {
                this.providers.Add(new DigitalBiblePlatformApi(Options.Create(new DigitalBiblePlatformApiOptions { ApiKey = digitalBiblePlatformApiKey }), this.cache));
            }

            // Load the ESV Provider
            string esvApiKey = Properties.Settings.Default.EsvApiKey;
            if (!string.IsNullOrWhiteSpace(esvApiKey))
            {
                this.providers.Add(new EsvBible(Options.Create(new EsvBibleOptions { ApiKey = esvApiKey }), this.cache));
            }

            // Load the Laodiceans Provider
            this.providers.Add(new Laodiceans());

            // Load the NET Provider
            this.providers.Add(new NetBible(this.cache));

            // Load the NLT Provider
            string nltApiKey = Properties.Settings.Default.NltApiKey;
            if (!string.IsNullOrWhiteSpace(nltApiKey))
            {
                this.providers.Add(new NltBible(Options.Create(new NltBibleOptions { ApiKey = nltApiKey }), this.cache));
            }

            // Load the SBL Greek New Testament Provider
            this.providers.Add(new SblGnt());

            // Get the list of blocked providers
            List<string> blockedProviders = Properties.Settings.Default.BlockedProviders?.Cast<string>()?.ToList() ?? new List<string>();

            // Create a list of enabled providers
            List<IProvider> enabledProviders = new List<IProvider>();

            // Set the providers for the renderer
            this.renderer.Providers.Clear();
            foreach (IProvider provider in this.providers)
            {
                if (!blockedProviders.Contains(provider.Id))
                {
                    this.renderer.Providers.Add(provider);
                    enabledProviders.Add(provider);
                }
            }

            // We return (and use this) to stop collection modified crashes
            return enabledProviders;
        }

        /// <summary>
        /// Loads the SQL cache.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        private void LoadSqlCache(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                this.cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            }
            else
            {
                try
                {
                    this.cache = new SqlServerCache(Options.Create(new SqlServerCacheOptions
                    {
                        ConnectionString = connectionString,
                        SchemaName = "dbo",
                        TableName = "GoToBible",
                    }));
                }
                catch (Exception)
                {
                    this.cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
                }
            }
        }

        /// <summary>
        /// Loads the translation combo boxes.
        /// </summary>
        /// <param name="providers">The providers.</param>
        /// <param name="primaryTranslation">The primary translation.</param>
        /// <param name="secondaryTranslation">The secondary translation.</param>
        /// <param name="resource">The open resource.</param>
        private async Task LoadTranslationComboBoxes(IList<IProvider> providers, string primaryTranslation, string secondaryTranslation, string resource)
        {
            // If we do not have a primary translation, secondary translation, or resource, see if we can get them
            if (string.IsNullOrWhiteSpace(primaryTranslation)
                && this.ToolStripComboBoxPrimaryTranslation.SelectedItem is TranslationComboBoxItem primaryComboBoxItem)
            {
                primaryTranslation = primaryComboBoxItem.Code;
            }

            if (string.IsNullOrWhiteSpace(secondaryTranslation)
                && this.ToolStripComboBoxSecondaryTranslation.SelectedItem is TranslationComboBoxItem secondaryComboBoxItem)
            {
                secondaryTranslation = secondaryComboBoxItem.Code;
            }

            if (string.IsNullOrWhiteSpace(resource)
                && this.ToolStripComboBoxResource.SelectedItem is TranslationComboBoxItem resourceComboBoxItem)
            {
                resource = resourceComboBoxItem.Code;
            }

            // Being updating the combo boxes
            this.ToolStripComboBoxPrimaryTranslation.BeginUpdate();
            this.ToolStripComboBoxSecondaryTranslation.BeginUpdate();
            this.ToolStripComboBoxResource.BeginUpdate();

            // Clear the combo boxes
            this.ToolStripComboBoxPrimaryTranslation.Items.Clear();
            this.ToolStripComboBoxSecondaryTranslation.Items.Clear();
            this.ToolStripComboBoxResource.Items.Clear();

            // Add the No Translation item
            this.ToolStripComboBoxSecondaryTranslation.Items.Add(new ComboBoxItem
            {
                Text = Properties.Resources.NoTranslation,
            });
            this.ToolStripComboBoxResource.Items.Add(new ComboBoxItem
            {
                Text = Properties.Resources.NoTranslation,
            });

            // Get the full list of translations
            List<string> blockedTranslations = Properties.Settings.Default.BlockedTranslations?.Cast<string>()?.ToList() ?? new List<string>();
            List<string> blockedCommentaries = Properties.Settings.Default.BlockedCommentaries?.Cast<string>()?.ToList() ?? new List<string>();
            List<Translation> commentaries = new List<Translation>();
            List<Translation> translations = new List<Translation>();
            foreach (IProvider provider in providers)
            {
                await foreach (Translation translation in provider.GetTranslationsAsync())
                {
                    if (translation.Commentary)
                    {
                        commentaries.Add(translation);
                    }
                    else
                    {
                        translations.Add(translation);
                    }
                }
            }

            // Store the translations for use by other methods
            this.translations.Clear();
            this.translations.AddRange(translations.OrderBy(t => t.Language, new LanguageComparer()).ThenBy(t => t.Name));
            this.commentaries.Clear();
            this.commentaries.AddRange(commentaries.OrderBy(t => t.Name));
            this.ToolStripMenuItemConfigure.Enabled = true;

            // Load the commentaries and translations from the providers
            int primaryTranslationSelectedIndex = 0;
            int secondaryTranslationSelectedIndex = 0;
            int resourceSelectedIndex = 0;
            int resourceIndex = 0;
            int translationIndex = 0;
            string lastLanguage = string.Empty;

            // Add the commentaries to resources
            foreach (Translation commentary in this.commentaries)
            {
                // If this translation is not blocked
                if (!blockedCommentaries.Contains(commentary.UniqueKey()))
                {
                    // Add the commentaries heading if required
                    if (resourceIndex == 0)
                    {
                        ComboBoxItem commentariesComboBoxItem = new ComboBoxItem
                        {
                            Bold = true,
                            Selectable = false,
                            Text = "Commentaries",
                        };
                        this.ToolStripComboBoxResource.Items.Add(commentariesComboBoxItem);
                        resourceIndex++;
                    }

                    // Set up the combo box item
                    TranslationComboBoxItem comboBoxItem = new TranslationComboBoxItem
                    {
                        Bold = false,
                        Selectable = true,
                        Text = commentary.UniqueName(this.translations),
                        CanBeExported = commentary.CanBeExported,
                        Code = commentary.Code,
                        Language = commentary.Language,
                        Provider = commentary.Provider,
                    };
                    this.ToolStripComboBoxResource.Items.Add(comboBoxItem);
                    if (commentary.Code == resource)
                    {
                        resourceSelectedIndex = resourceIndex + 1;
                    }

                    resourceIndex++;
                }
            }

            foreach (Translation translation in this.translations)
            {
                // If this translation is not blocked
                if (!blockedTranslations.Contains(translation.UniqueKey()))
                {
                    // Add a heading, if required
                    if (translation.Language != lastLanguage)
                    {
                        ComboBoxItem headingComboBoxItem = new ComboBoxItem
                        {
                            Bold = true,
                            Selectable = false,
                            Text = translation.Language ?? Properties.Resources.UnknownLanguage,
                        };
                        this.ToolStripComboBoxPrimaryTranslation.Items.Add(headingComboBoxItem);
                        this.ToolStripComboBoxSecondaryTranslation.Items.Add(headingComboBoxItem);
                        this.ToolStripComboBoxResource.Items.Add(headingComboBoxItem);
                        lastLanguage = translation.Language ?? Properties.Resources.UnknownLanguage;
                        resourceIndex++;
                        translationIndex++;
                    }

                    // Set up the combo box item
                    TranslationComboBoxItem comboBoxItem = new TranslationComboBoxItem
                    {
                        Bold = false,
                        Selectable = true,
                        Text = translation.UniqueName(this.translations),
                        CanBeExported = translation.CanBeExported,
                        Code = translation.Code,
                        Language = translation.Language,
                        Provider = translation.Provider,
                    };
                    this.ToolStripComboBoxPrimaryTranslation.Items.Add(comboBoxItem);
                    if (translation.Code == primaryTranslation)
                    {
                        primaryTranslationSelectedIndex = translationIndex;
                    }

                    this.ToolStripComboBoxSecondaryTranslation.Items.Add(comboBoxItem);
                    if (translation.Code == secondaryTranslation)
                    {
                        secondaryTranslationSelectedIndex = translationIndex + 1;
                    }

                    this.ToolStripComboBoxResource.Items.Add(comboBoxItem);
                    if (translation.Code == resource)
                    {
                        resourceSelectedIndex = resourceIndex + 1;
                    }

                    resourceIndex++;
                    translationIndex++;
                }
            }

            // Completed updating the combo boxes
            this.ToolStripComboBoxPrimaryTranslation.EndUpdate();
            this.ToolStripComboBoxSecondaryTranslation.EndUpdate();
            this.ToolStripComboBoxResource.EndUpdate();

            // Add none to the primary, if it is empty
            if (this.ToolStripComboBoxPrimaryTranslation.Items.Count == 0)
            {
                this.ToolStripComboBoxPrimaryTranslation.Items.Add(new ComboBoxItem
                {
                    Text = Properties.Resources.NoTranslation,
                });
            }

            // Select the first items in each translation drop down
            this.ToolStripComboBoxPrimaryTranslation.SelectedIndex = primaryTranslationSelectedIndex;
            this.ToolStripComboBoxSecondaryTranslation.SelectedIndex = secondaryTranslationSelectedIndex;
            this.ToolStripComboBoxResource.SelectedIndex = resourceSelectedIndex;

            // The translations are loaded
            this.translationsLoaded = true;
        }

        /// <summary>
        /// Sets up the web page.
        /// </summary>
        /// <param name="webView">The web view.</param>
        private void SetupWebPage(WebView2 webView)
        {
            try
            {
                string body;
                if (this.translationsLoaded && !this.translations.Any())
                {
                    body = string.Empty;
                }
                else
                {
                    body = Html.LoadingCodeBody;
                }

                webView.NavigateToString($"<!DOCTYPE html>\n<html><head><style type=\"text/css\">{this.parameters.RenderCss()}{Html.LoadingCodeCss}</style></head><body>{body}</body></html>");
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        /// <summary>
        /// Handles the Click event of the Export ToolStripButton.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripButtonExport_Click(object sender, EventArgs e)
        {
            this.SaveFileDialogMain.FileName = this.parameters.PassageReference.Start.Split(':').First() + ".txt";
            if (this.SaveFileDialogMain.ShowDialog() == DialogResult.OK)
            {
                // Render for export to Accordance
                RenderedPassage renderedPassage = await this.renderer.RenderAsync(this.parameters with { Format = RenderFormat.Accordance }, false);
                await File.WriteAllTextAsync(this.SaveFileDialogMain.FileName, renderedPassage.Content);
            }
        }

        /// <summary>
        /// Handles the Click event of the Navigate Back ToolStripButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripButtonNavigateBack_Click(object sender, EventArgs e)
        {
            if (this.renderedPassage.PreviousPassage.IsValid)
            {
                this.ToolStripTextBoxPassage.Text = this.renderedPassage.PreviousPassage.Start;
                await this.ShowPassage(true, true);
            }
        }

        /// <summary>
        /// Handles the Click event of the Navigate Forward ToolStripButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripButtonNavigateForward_Click(object sender, EventArgs e)
        {
            if (this.renderedPassage.NextPassage.IsValid)
            {
                this.ToolStripTextBoxPassage.Text = this.renderedPassage.NextPassage.Start;
                await this.ShowPassage(true, true);
            }
        }

        /// <summary>
        /// Handles the Click event of the Passage "Go" ToolStripButton.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void ToolStripButtonPassageGo_Click(object sender, EventArgs e) => await this.ShowPassage(true, true);

        /// <summary>
        /// Sets up the automatic complete asynchronously.
        /// </summary>
        private async Task SetupAutoCompleteAsync()
        {
            // Build the list of suggestions
            List<string> suggestions = new List<string>();

            // If we have a translation in the primary combo box
            if (this.ToolStripComboBoxPrimaryTranslation.SelectedItem is TranslationComboBoxItem primaryComboBoxItem)
            {
                // Add the book names to the suggestions list
                await foreach (Book book in this.providers.First(p => p.Id == primaryComboBoxItem.Provider).GetBooksAsync(primaryComboBoxItem.Code, false))
                {
                    if (!suggestions.Contains(book.Name))
                    {
                        suggestions.Add(book.Name);
                    }
                }
            }

            // If we have a translation in the secondary combo box
            if (this.ToolStripComboBoxSecondaryTranslation.SelectedItem is TranslationComboBoxItem secondaryComboBoxItem)
            {
                // Add the book names to the suggestions list
                await foreach (Book book in this.providers.First(p => p.Id == secondaryComboBoxItem.Provider).GetBooksAsync(secondaryComboBoxItem.Code, false))
                {
                    if (!suggestions.Contains(book.Name))
                    {
                        suggestions.Add(book.Name);
                    }
                }
            }

            // Enable autocomplete
            AutoSuggest.Disable(this.ToolStripTextBoxPassage.TextBox);
            AutoSuggest.Enable(this.ToolStripTextBoxPassage.TextBox, suggestions.ToArray());
        }

        /// <summary>
        /// Shows the passage.
        /// </summary>
        /// <param name="updateMain">if set to <c>true</c> update the main web view.</param>
        /// <param name="updateResource">if set to <c>true</c> update resource web view.</param>
        /// <returns>The task.</returns>
        private async Task ShowPassage(bool updateMain, bool updateResource)
        {
            if (!updateMain && !updateResource)
            {
                return;
            }

            Stopwatch watch = Stopwatch.StartNew();

            // Show the loading animation
            try
            {
                if (updateMain)
                {
                    await this.WebViewMain.ExecuteScriptAsync($"document.body.innerHTML=\"{HttpUtility.JavaScriptStringEncode(Html.LoadingCodeBody)}\";");
                }

                if (updateResource)
                {
                    await this.WebViewResource.ExecuteScriptAsync($"document.body.innerHTML=\"{HttpUtility.JavaScriptStringEncode(Html.LoadingCodeBody)}\";");
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }

            string primaryTranslation = string.Empty;
            string primaryProvider = string.Empty;
            if (this.ToolStripComboBoxPrimaryTranslation.SelectedItem is TranslationComboBoxItem primaryComboBoxItem)
            {
                primaryProvider = primaryComboBoxItem.Provider;
                primaryTranslation = primaryComboBoxItem.Code;
            }

            string secondaryTranslation = string.Empty;
            string secondaryProvider = string.Empty;
            if (this.ToolStripComboBoxSecondaryTranslation.SelectedItem is TranslationComboBoxItem secondaryComboBoxItem)
            {
                secondaryProvider = secondaryComboBoxItem.Provider;
                secondaryTranslation = secondaryComboBoxItem.Code;
            }

            string resource = string.Empty;
            string resourceProvider = string.Empty;
            if (this.ToolStripComboBoxResource.SelectedItem is TranslationComboBoxItem resourceComboBoxItem)
            {
                resourceProvider = resourceComboBoxItem.Provider;
                resource = resourceComboBoxItem.Code;
            }

            this.ToolStripButtonSwap.Enabled = !string.IsNullOrWhiteSpace(secondaryTranslation);
            this.parameters = new RenderingParameters
            {
                BackgroundColour = this.ColourDialogMain.Color,
                Font = this.FontDialogMain.Font.AsRenderFont(),
                ForegroundColour = this.FontDialogMain.Color,
                Format = RenderFormat.Html,
                InterlinearIgnoresCase = this.ToolStripMenuItemIgnoreCase.Checked,
                InterlinearIgnoresDiacritics = this.ToolStripMenuItemIgnoreDiacritics.Checked,
                InterlinearIgnoresPunctuation = this.ToolStripMenuItemIgnorePunctuation.Checked,
                IsDebug = this.ToolStripMenuItemDebugMode.Checked,
                PassageReference = this.ToolStripTextBoxPassage.Text.AsPassageReference(),
                PrimaryProvider = primaryProvider,
                PrimaryTranslation = primaryTranslation,
                RenderItalics = this.ToolStripMenuItemShowItalics.Checked,
                SecondaryProvider = secondaryProvider,
                SecondaryTranslation = secondaryTranslation,
            };
            if (updateMain)
            {
                this.renderedPassage = await this.renderer.RenderAsync(this.parameters, false);
            }

            RenderedPassage renderedResource = new RenderedPassage();
            if (!string.IsNullOrWhiteSpace(resource) && updateResource)
            {
                renderedResource = await this.renderer.RenderAsync(this.parameters with { PrimaryProvider = resourceProvider, PrimaryTranslation = resource, SecondaryTranslation = null }, false);
            }

            if (!string.IsNullOrWhiteSpace(this.renderedPassage.Content))
            {
                // Save settings
                if (this.primaryWindow || Program.Forms.Count == 1)
                {
                    Properties.Settings.Default.InterlinearIgnoresCase = this.parameters.InterlinearIgnoresCase;
                    Properties.Settings.Default.InterlinearIgnoresDiacritics = this.parameters.InterlinearIgnoresDiacritics;
                    Properties.Settings.Default.InterlinearIgnoresPunctuation = this.parameters.InterlinearIgnoresPunctuation;
                    Properties.Settings.Default.IsDebug = this.parameters.IsDebug;
                    Properties.Settings.Default.RenderItalics = this.parameters.RenderItalics;
                    Properties.Settings.Default.Passage = this.parameters.PassageReference.Start;
                    Properties.Settings.Default.PrimaryTranslation = this.parameters.PrimaryTranslation;
                    Properties.Settings.Default.SecondaryTranslation = this.parameters.SecondaryTranslation;
                    Properties.Settings.Default.Resource = resource;
                    Properties.Settings.Default.CustomColours = string.Join(',', this.ColourDialogMain.CustomColors.Select(c => c.ToString()));
                    Properties.Settings.Default.BackgroundColour = ColorTranslator.ToOle(this.ColourDialogMain.Color);
                    Properties.Settings.Default.ForegroundColour = ColorTranslator.ToOle(this.FontDialogMain.Color);
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
                    Properties.Settings.Default.Font = converter.ConvertToString(this.FontDialogMain.Font);
                    Properties.Settings.Default.Save();
                }
            }

            // Show the content
            try
            {
                if (updateMain)
                {
                    await this.WebViewMain.ExecuteScriptAsync($"document.body.innerHTML=\"{HttpUtility.JavaScriptStringEncode(this.renderedPassage.Content)}\";");
                }

                if (updateResource)
                {
                    await this.WebViewResource.ExecuteScriptAsync($"document.body.innerHTML=\"{HttpUtility.JavaScriptStringEncode(renderedResource.Content)}\";");
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
            finally
            {
                watch.Stop();
                Debug.WriteLine("ShowPassage execution time: " + watch.ElapsedMilliseconds);
            }

            // Enable or disable the next/previous buttons
            this.ToolStripButtonNavigateForward.Enabled = this.renderedPassage.NextPassage.IsValid;
            this.ToolStripButtonNavigateBack.Enabled = this.renderedPassage.PreviousPassage.IsValid;

            // Build the list of suggestions
            List<string> suggestions = new List<string>();

            // If we have a translation in the primary combo box
            if (this.ToolStripComboBoxPrimaryTranslation.SelectedItem is TranslationComboBoxItem translationItem)
            {
                // Show/hide the export button
                this.ToolStripButtonExport.Enabled = translationItem.CanBeExported;
            }
        }

        /// <summary>
        /// Handles the Click event of the ToolStripButtonNewWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolStripButtonNewWindow_Click(object sender, EventArgs e)
        {
            // Create the form
            FormMain formMain = new FormMain(false)
            {
                StartPosition = FormStartPosition.CenterParent,
            };
            Program.Forms.Add(formMain);
            formMain.Show();

            // See if we should show on the second monitor
            if (this.WindowState == FormWindowState.Maximized)
            {
                Screen[] screens = Screen.AllScreens;
                if (screens.Length > 1)
                {
                    Screen thisScreen = Screen.FromControl(this);
                    foreach (Screen screen in screens)
                    {
                        if (screen.Bounds != thisScreen.Bounds)
                        {
                            formMain.Location = screen.WorkingArea.Location;
                            formMain.Size = Properties.Settings.Default.WindowSize;
                            formMain.WindowState = FormWindowState.Maximized;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Swap ToolStripButton.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripButtonSwap_Click(object sender, EventArgs e)
        {
            if (this.ToolStripComboBoxSecondaryTranslation.Text != Properties.Resources.NoTranslation)
            {
                int secondaryIndex = this.ToolStripComboBoxSecondaryTranslation.SelectedIndex;
                this.ToolStripComboBoxSecondaryTranslation.SelectedIndex = this.ToolStripComboBoxPrimaryTranslation.SelectedIndex + 1;
                this.ToolStripComboBoxPrimaryTranslation.SelectedIndex = secondaryIndex - 1;
                await this.ShowPassage(true, false);
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the Resource ToolStripComboBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripComboBoxResource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ToolStripComboBoxResource.SelectedItem is ComboBoxItem item && !item.Selectable)
            {
                // Handle the non-selectable items
                if (this.ToolStripComboBoxResource.Items.Count - 1 > this.ToolStripComboBoxResource.SelectedIndex)
                {
                    this.ToolStripComboBoxResource.SelectedIndex++;
                }
                else if (this.ToolStripComboBoxResource.SelectedIndex > 0)
                {
                    this.ToolStripComboBoxResource.SelectedIndex--;
                }
                else
                {
                    this.ToolStripComboBoxResource.SelectedIndex = -1;
                }
            }
            else
            {
                await this.SetupAutoCompleteAsync();
                this.SplitContainerMain.Panel2Collapsed = this.ToolStripComboBoxResource.SelectedIndex < 1;
                if (!string.IsNullOrWhiteSpace(this.ToolStripTextBoxPassage.Text))
                {
                    await this.ShowPassage(false, true);
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the Primary Translation ToolStripComboBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripComboBoxPrimaryTranslation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ToolStripComboBoxPrimaryTranslation.SelectedItem is ComboBoxItem item && !item.Selectable)
            {
                // Handle the non-selectable items
                if (this.ToolStripComboBoxPrimaryTranslation.Items.Count - 1 > this.ToolStripComboBoxPrimaryTranslation.SelectedIndex)
                {
                    this.ToolStripComboBoxPrimaryTranslation.SelectedIndex++;
                }
                else if (this.ToolStripComboBoxPrimaryTranslation.SelectedIndex > 0)
                {
                    this.ToolStripComboBoxPrimaryTranslation.SelectedIndex--;
                }
                else
                {
                    this.ToolStripComboBoxPrimaryTranslation.SelectedIndex = -1;
                }
            }
            else
            {
                // Make sure we are not showing an interlinear with the original language
                if (this.ToolStripComboBoxPrimaryTranslation.SelectedItem is TranslationComboBoxItem primaryItem
                    && this.ToolStripComboBoxSecondaryTranslation.SelectedItem is TranslationComboBoxItem secondaryItem)
                {
                    if ((primaryItem.Language == "Greek" && secondaryItem.Language != "Greek")
                        || (primaryItem.Language == "Hebrew" && secondaryItem.Language != "Hebrew")
                        || (secondaryItem.Language == "Greek" && primaryItem.Language != "Greek")
                        || (secondaryItem.Language == "Hebrew" && primaryItem.Language != "Hebrew"))
                    {
                        MessageBox.Show("Sorry, you cannot show this translation interlinear with an original language", "GoTo.Bible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.ToolStripComboBoxSecondaryTranslation.SelectedIndex = 0;
                    }
                }

                // Show the passage
                await this.SetupAutoCompleteAsync();
                if (!string.IsNullOrWhiteSpace(this.ToolStripTextBoxPassage.Text))
                {
                    await this.ShowPassage(true, false);
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the Secondary Translation ToolStripComboBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripComboBoxSecondaryTranslation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ToolStripComboBoxSecondaryTranslation.SelectedItem is ComboBoxItem item && !item.Selectable)
            {
                // Handle the non-selectable items
                if (this.ToolStripComboBoxSecondaryTranslation.Items.Count - 1 > this.ToolStripComboBoxSecondaryTranslation.SelectedIndex)
                {
                    this.ToolStripComboBoxSecondaryTranslation.SelectedIndex++;
                }
                else if (this.ToolStripComboBoxSecondaryTranslation.SelectedIndex > 0)
                {
                    this.ToolStripComboBoxSecondaryTranslation.SelectedIndex--;
                }
                else
                {
                    this.ToolStripComboBoxSecondaryTranslation.SelectedIndex = -1;
                }
            }
            else
            {
                // Make sure we are not showing an interlinear with the original language
                if (this.ToolStripComboBoxPrimaryTranslation.SelectedItem is TranslationComboBoxItem primaryItem
                    && this.ToolStripComboBoxSecondaryTranslation.SelectedItem is TranslationComboBoxItem secondaryItem)
                {
                    if ((primaryItem.Language == "Greek" && secondaryItem.Language != "Greek")
                        || (primaryItem.Language == "Hebrew" && secondaryItem.Language != "Hebrew")
                        || (secondaryItem.Language == "Greek" && primaryItem.Language != "Greek")
                        || (secondaryItem.Language == "Hebrew" && primaryItem.Language != "Hebrew"))
                    {
                        MessageBox.Show("Sorry, you cannot show this translation interlinear with an original language", "GoTo.Bible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.ToolStripComboBoxSecondaryTranslation.SelectedIndex = 0;
                    }
                }

                // Show the passage
                await this.SetupAutoCompleteAsync();
                if (!string.IsNullOrWhiteSpace(this.ToolStripTextBoxPassage.Text))
                {
                    await this.ShowPassage(true, false);
                }
            }
        }

        /// <summary>
        /// Handles the DrawItem event of the ToolStripComboBox controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DrawItemEventArgs"/> instance containing the event data.</param>
        private void ToolStripComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (sender is ComboBox comboBox && e.Index > -1 && comboBox.Items[e.Index] is ComboBoxItem item)
            {
                if (item.Selectable)
                {
                    e.DrawBackground();
                }
                else
                {
                    using Brush backBrush = new SolidBrush(Color.White);
                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                }

                SizeF size;
                if (item.Bold)
                {
                    using Font font = new Font(comboBox.Font, FontStyle.Bold);
                    e.Graphics.DrawString(item.Text, font, SystemBrushes.ControlText, e.Bounds);
                    size = e.Graphics.MeasureString(item.Text, font);
                }
                else
                {
                    using Font font = new Font(comboBox.Font, FontStyle.Regular);
                    e.Graphics.DrawString(item.Text, font, SystemBrushes.ControlText, e.Bounds);
                    size = e.Graphics.MeasureString(item.Text, font);
                }

                // See if we need to show the tooltip or hide it
                if (size.Width > e.Bounds.Width && comboBox.DroppedDown)
                {
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                        this.ToolTipMain.Show(item.Text, comboBox, e.Bounds.Right, e.Bounds.Bottom);
                    }
                }
                else
                {
                    this.ToolStripComboBox_DropDownClosed(sender, e);
                }

                if (item.Selectable && comboBox.DroppedDown)
                {
                    e.DrawFocusRectangle();
                }
            }
        }

        /// <summary>
        /// Handles the DropDownClosed event of the ToolStripComboBox controls.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolStripComboBox_DropDownClosed(object? sender, EventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                this.ToolTipMain.Hide(comboBox);
            }
        }

        /// <summary>
        /// Handles the Click event of the Background Colour ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemBackgroundColour_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = this.ColourDialogMain.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                // Update the rendering parameters
                this.parameters.BackgroundColour = this.ColourDialogMain.Color;

                // Update the CSS
                this.SetupWebPage(this.WebViewMain);
                this.SetupWebPage(this.WebViewResource);

                // Render the passage
                await this.ShowPassage(true, true);
            }
        }

        /// <summary>
        /// Handles the Click event of the BibleApi ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemBibleApi_Click(object sender, EventArgs e)
        {
            // Show the API.Bible Key Dialog
            string key = Properties.Settings.Default.BibleApiKey;
            using FormApiKey formApiKey = new FormApiKey(key, "API.Bible", new Uri("https://scripture.api.bible/", UriKind.Absolute), Properties.Resources.BibleApiIcon);
            formApiKey.ShowDialog(this);

            // Only load the provider if the key has changed
            if (key != formApiKey.Key)
            {
                // Save the key
                Properties.Settings.Default.BibleApiKey = formApiKey.Key;
                Properties.Settings.Default.Save();

                // Reload the providers and translations
                await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Handles the Click event of the Biblia ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemBiblia_Click(object sender, EventArgs e)
        {
            // Show the Biblia API Key Dialog
            string key = Properties.Settings.Default.BibliaApiKey;
            using FormApiKey formApiKey = new FormApiKey(key, "Biblia API", new Uri("https://api.biblia.com/v1/Users/SignIn", UriKind.Absolute), Properties.Resources.BibliaIcon);
            formApiKey.ShowDialog(this);

            // Only load the provider if the key has changed
            if (key != formApiKey.Key)
            {
                // Save the key
                Properties.Settings.Default.BibliaApiKey = formApiKey.Key;
                Properties.Settings.Default.Save();

                // Reload the providers and translations
                await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Handles the Click event of the Commentaries ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemCommentaries_Click(object sender, EventArgs e)
        {
            // Show the show/hide commentaries dialog
            List<string> blockedCommentaries = Properties.Settings.Default.BlockedCommentaries?.Cast<string>()?.ToList() ?? new List<string>();
            Dictionary<string, string> items = this.commentaries.ToDictionary(k => k.UniqueKey(), v => v.UniqueName(this.commentaries));
            using FormCheckBoxList formCheckBoxList = new FormCheckBoxList(items, blockedCommentaries, "Configure Commentaries", Properties.Resources.CommentariesIcon);
            DialogResult dialogResult = formCheckBoxList.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                // Save the blocked commentaries
                Properties.Settings.Default.BlockedCommentaries?.Clear();
                if (Properties.Settings.Default.BlockedCommentaries == null)
                {
                    Properties.Settings.Default.BlockedCommentaries = new StringCollection();
                }

                Properties.Settings.Default.BlockedCommentaries.AddRange(formCheckBoxList.UncheckedItems.ToArray());
                Properties.Settings.Default.Save();

                // Reload the commentaries
                await this.LoadTranslationComboBoxes(this.renderer.Providers.ToList(), string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Handles the Click event of the Debug Mode ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemDebugMode_Click(object sender, EventArgs e)
        {
            // Enable debug tools
            this.WebViewMain.CoreWebView2.Settings.AreDevToolsEnabled = this.ToolStripMenuItemDebugMode.Checked;
            this.SetupWebPage(this.WebViewMain);
            this.WebViewResource.CoreWebView2.Settings.AreDevToolsEnabled = this.ToolStripMenuItemDebugMode.Checked;
            this.SetupWebPage(this.WebViewResource);

            // Debug the passage renderer
            await this.ShowPassage(true, true);
        }

        /// <summary>
        /// Handles the Click event of the Digital Bible Platform ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemDigitalBiblePlatform_Click(object sender, EventArgs e)
        {
            // Show the Digital Bible Platform API Key Dialog
            string key = Properties.Settings.Default.DigitalBiblePlatformApiKey;
            using FormApiKey formApiKey = new FormApiKey(key, "Digital Bible Platform API", new Uri("https://www.digitalbibleplatform.com/", UriKind.Absolute), Properties.Resources.DigitalBiblePlatformIcon);
            formApiKey.ShowDialog(this);

            // Only load the provider if the key has changed
            if (key != formApiKey.Key)
            {
                // Save the key
                Properties.Settings.Default.DigitalBiblePlatformApiKey = formApiKey.Key;
                Properties.Settings.Default.Save();

                // Reload the providers and translations
                await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Handles the Click event of the ESV ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemEsv_Click(object sender, EventArgs e)
        {
            // Show the ESV API Key Dialog
            string key = Properties.Settings.Default.EsvApiKey;
            using FormApiKey formApiKey = new FormApiKey(key, "ESV API", new Uri("https://api.esv.org/", UriKind.Absolute), Properties.Resources.EsvIcon);
            formApiKey.ShowDialog(this);

            // Only load the provider if the key has changed
            if (key != formApiKey.Key)
            {
                // Save the key
                Properties.Settings.Default.EsvApiKey = formApiKey.Key;
                Properties.Settings.Default.Save();

                // Reload the providers and translations
                await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Handles the Click event of the Font Settings ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemFontSettings_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = this.FontDialogMain.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                // Update the rendering parameters
                this.parameters.Font = this.FontDialogMain.Font.AsRenderFont();
                this.parameters.ForegroundColour = this.FontDialogMain.Color;

                // Update the CSS
                this.SetupWebPage(this.WebViewMain);
                this.SetupWebPage(this.WebViewResource);

                // Render the passage
                await this.ShowPassage(true, true);
            }
        }

        /// <summary>
        /// Handles the Click event of the Ignore Case ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemIgnoreCase_Click(object sender, EventArgs e) => await this.ShowPassage(true, false);

        /// <summary>
        /// Handles the Click event of the Ignore Diacritics ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemIgnoreDiacritics_Click(object sender, EventArgs e) => await this.ShowPassage(true, false);

        /// <summary>
        /// Handles the Click event of the Ignore Punctuation ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemIgnorePunctuation_Click(object sender, EventArgs e) => await this.ShowPassage(true, false);

        /// <summary>
        /// Handles the Click event of the NLT ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemNlt_Click(object sender, EventArgs e)
        {
            // Show the NLT API Key Dialog
            string key = Properties.Settings.Default.NltApiKey;
            using FormApiKey formApiKey = new FormApiKey(key, "NLT API", new Uri("https://api.nlt.to/", UriKind.Absolute), Properties.Resources.NltIcon);
            formApiKey.ShowDialog(this);

            // Only load the provider if the key has changed
            if (key != formApiKey.Key)
            {
                // Save the key
                Properties.Settings.Default.NltApiKey = formApiKey.Key;
                Properties.Settings.Default.Save();

                // Reload the providers and translations
                await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Handles the Click event of the Providers ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemProviders_Click(object sender, EventArgs e)
        {
            // Show the show/hide providers dialog
            List<string> blockedProviders = Properties.Settings.Default.BlockedProviders?.Cast<string>()?.ToList() ?? new List<string>();
            Dictionary<string, string> items = this.providers.ToDictionary(k => k.Id, v => v.Name);
            using FormCheckBoxList formCheckBoxList = new FormCheckBoxList(items, blockedProviders, "Configure Providers", Properties.Resources.ProviderIcon);
            DialogResult dialogResult = formCheckBoxList.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                // Save the blocked providers
                Properties.Settings.Default.BlockedProviders?.Clear();
                if (Properties.Settings.Default.BlockedProviders == null)
                {
                    Properties.Settings.Default.BlockedProviders = new StringCollection();
                }

                Properties.Settings.Default.BlockedProviders.AddRange(formCheckBoxList.UncheckedItems.ToArray());
                Properties.Settings.Default.Save();

                // Reload the providers and translations
                await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Handles the Click event of the ShowItalics ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemShowItalics_Click(object sender, EventArgs e) => await this.ShowPassage(true, true);

        /// <summary>
        /// Handles the Click event of the SQL ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolStripMenuItemSql_Click(object sender, EventArgs e)
        {
            // Show the SQL Server Connection String Dialog
            string key = Properties.Settings.Default.SqlConnectionString;
            using FormApiKey formApiKey = new FormApiKey(key, "SQL Server", new Uri("https://www.microsoft.com/en-us/sql-server/sql-server-downloads/", UriKind.Absolute), Properties.Resources.SqlIcon);
            formApiKey.ShowDialog(this);

            // Only load the provider if the key has changed
            if (key != formApiKey.Key)
            {
                // Save the key
                Properties.Settings.Default.SqlConnectionString = formApiKey.Key;
                Properties.Settings.Default.Save();

                // Update the cache
                if (string.IsNullOrWhiteSpace(formApiKey.Key))
                {
                    this.cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
                }
                else
                {
                    this.LoadSqlCache(formApiKey.Key);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Translations ToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ToolStripMenuItemTranslations_Click(object sender, EventArgs e)
        {
            // Show the show/hide translations dialog
            List<string> blockedTranslations = Properties.Settings.Default.BlockedTranslations?.Cast<string>()?.ToList() ?? new List<string>();
            Dictionary<string, string> items = this.translations.ToDictionary(k => k.UniqueKey(), v => v.UniqueName(this.translations));
            using FormCheckBoxList formCheckBoxList = new FormCheckBoxList(items, blockedTranslations, "Configure Translations", Properties.Resources.TranslationsIcon);
            DialogResult dialogResult = formCheckBoxList.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                // Save the blocked translations
                Properties.Settings.Default.BlockedTranslations?.Clear();
                if (Properties.Settings.Default.BlockedTranslations == null)
                {
                    Properties.Settings.Default.BlockedTranslations = new StringCollection();
                }

                Properties.Settings.Default.BlockedTranslations.AddRange(formCheckBoxList.UncheckedItems.ToArray());
                Properties.Settings.Default.Save();

                // Reload the providers and translations
                await this.LoadTranslationComboBoxes(this.renderer.Providers.ToList(), string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Handles the ButtonClick event of the Settings ToolStripSplitButton.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolStripSplitButtonSettings_ButtonClick(object sender, EventArgs e) => this.ToolStripSplitButtonSettings.ShowDropDown();

        /// <summary>
        /// Handles the KeyDown event of the Passage ToolStripTextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private async void ToolStripTextBoxPassage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                await this.ShowPassage(true, true);
                e.Handled = e.SuppressKeyPress = true;
            }
        }
    }
}
