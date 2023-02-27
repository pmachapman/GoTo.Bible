// -----------------------------------------------------------------------
// <copyright file="FormMain.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoToBible.Engine;
using GoToBible.Model;
using GoToBible.Providers;
using GoToBible.Windows.AutoComplete;
using GoToBible.Windows.Properties;
using GoToBible.Windows.WebBrowser;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Options;
using Microsoft.Web.WebView2.Core;
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
    /// The system menu.
    /// </summary>
    private readonly SystemMenu systemMenu;

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
    /// The bible data renderer.
    /// </summary>
    private IRenderer renderer = new Renderer();

    /// <summary>
    /// The rendering parameters.
    /// </summary>
    private RenderingParameters parameters = new RenderingParameters();

    /// <summary>
    /// A value indicating whether or not the translations are loaded.
    /// </summary>
    private bool translationsLoaded;

    /// <summary>
    /// A value indicating whether or not the web view is initialised.
    /// </summary>
    private bool webViewInitialised;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormMain" /> class.
    /// </summary>
    /// <param name="primaryWindow">If set to <c>true</c>, this is the primary window, which means settings will be saved.</param>
    public FormMain(bool primaryWindow)
    {
        // Specify primary window
        this.primaryWindow = primaryWindow;

        // Setup the system menu
        this.systemMenu = new SystemMenu(this);
        this.systemMenu.AddMenuItem("&About GoTo.Bible…", this.SystemMenuAbout_Click, true);

        // Setup the GUI via the designer file
        this.InitializeComponent();

        // Clean up the tool strips
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
        this.ToolStripExtras.Dock = DockStyle.None;
        this.ToolStripExtras.Location = new Point(this.ToolStripSettings.Left + this.ToolStripSettings.Width, 0);
        this.ToolStripContainerMain.ResumeLayout();

        // Set initial button states
        this.ToolStripButtonNavigateBack.Enabled = false;
        this.ToolStripButtonNavigateForward.Enabled = false;
        this.ToolStripMenuItemConfigure.Enabled = false;
        this.ToolStripButtonApparatusGenerator.Enabled = false;
    }

    /// <summary>
    /// Gets the path to the settings directory.
    /// </summary>
    /// <value>
    /// The settings directory path.
    /// </value>
    private static string SettingsDirectory
    {
        get
        {
            try
            {
                Configuration userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                return Path.GetDirectoryName(userConfig.FilePath) ?? Path.GetTempPath();
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException
                    or ConfigurationErrorsException
                    or ConfigurationException
                    or PathTooLongException
                    or SecurityException)
                {
                    return Path.GetTempPath();
                }
                else
                {
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is compiled in debug mode.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance compiled debug mode; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This value has no effect on the rendering parameter <see cref="RenderingParameters.IsDebug"/>.
    /// </remarks>
    private static bool IsDebug =>
#if DEBUG
        true;
#else
            false;
#endif

    /// <summary>
    /// Gets or sets a value indicating whether this instance is in developer mode.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is in developer mode; otherwise, <c>false</c>.
    /// </value>
    private bool IsDeveloper
    {
        get => this.ToolStripMenuItemDeveloperMode.Checked;
        set => this.ToolStripMenuItemDeveloperMode.Checked = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is in legacy browser mode.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is in legacy browser mode; otherwise, <c>false</c>.
    /// </value>
    private bool IsLegacyBrowser
    {
        get => this.ToolStripMenuItemLegacyBrowser.Checked;
        set => this.ToolStripMenuItemLegacyBrowser.Checked = value;
    }

    /// <summary>
    /// Gets or sets the main web view.
    /// </summary>
    /// <value>
    /// The main web view.
    /// </value>
    private IWebBrowser WebViewMain { get; set; } = new EmptyWebBrowser();

    /// <summary>
    /// Gets or sets the resource web view.
    /// </summary>
    /// <value>
    /// The resource web view.
    /// </value>
    private IWebBrowser WebViewResource { get; set; } = new EmptyWebBrowser();

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing"><c>true</c> if managed resources should be disposed; otherwise, <c>false</c>.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && this.components is not null)
        {
            this.components.Dispose();
            this.renderer.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Processes Windows messages.
    /// </summary>
    /// <param name="msg">The Windows <see cref="Message"/> to process.</param>
    protected override void WndProc(ref Message msg)
    {
        base.WndProc(ref msg);
        this.systemMenu.HandleMessage(ref msg);
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
            Settings.Default.SplitterDistance = this.SplitContainerMain.SplitterDistance;
            Settings.Default.WindowState = (int)this.WindowState;
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowLocation = this.Location;
                Settings.Default.WindowSize = this.Size;
            }
            else
            {
                Settings.Default.WindowLocation = this.RestoreBounds.Location;
                Settings.Default.WindowSize = this.RestoreBounds.Size;
            }

            Settings.Default.Save();
        }

        // Remove this form from the program list
        Program.Forms.Remove(this);

        // Disable autocomplete
        if (this.ToolStripTextBoxPassage.TextBox is not null)
        {
            AutoSuggest.Disable(this.ToolStripTextBoxPassage.TextBox);
        }

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
            this.WindowState = (FormWindowState)Settings.Default.WindowState;
            this.Location = Settings.Default.WindowLocation;
            this.Size = Settings.Default.WindowSize;
        }

        // Set up the splitter
        int splitterDistance = Settings.Default.SplitterDistance;
        if (splitterDistance == 0)
        {
            splitterDistance = this.Width - (this.Width / 3);
        }

        this.SplitContainerMain.SplitterDistance = splitterDistance;

        // Get the colours and font
        this.ColourDialogBackground.Color = ColorTranslator.FromOle(Settings.Default.BackgroundColour);
        this.ColourDialogBackground.CustomColors = Settings.Default.BackgroundCustomColours
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => int.TryParse(c, out int i) ? i : 16777215)
            .ToArray();
        this.ColourDialogHighlight.Color = ColorTranslator.FromOle(Settings.Default.HighlightColour);
        this.ColourDialogHighlight.CustomColors = Settings.Default.HighlightCustomColours
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => int.TryParse(c, out int i) ? i : 16777215)
            .ToArray();
        this.FontDialogMain.Color = ColorTranslator.FromOle(Settings.Default.ForegroundColour);
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
        this.FontDialogMain.Font = converter.ConvertFromString(Settings.Default.Font) as Font ?? Default.Font.AsFont();

        // Setup the rendering parameters for CSS
        this.parameters = new RenderingParameters
        {
            BackgroundColour = this.ColourDialogBackground.Color.AsRenderColour(),
            Font = this.FontDialogMain.Font.AsRenderFont(),
            ForegroundColour = this.FontDialogMain.Color.AsRenderColour(),
            HighlightColour = this.ColourDialogHighlight.Color.AsRenderColour(),
        };

        // Get the translation and passage settings
        string primaryTranslation = Settings.Default.PrimaryTranslation;
        string secondaryTranslation = Settings.Default.SecondaryTranslation;
        string resource = Settings.Default.Resource;
        string passage = Settings.Default.Passage;
        if (string.IsNullOrWhiteSpace(passage))
        {
            passage = Default.Passage;
        }

        // Setup the tool strip controls
        if (this.ToolStripComboBoxPrimaryTranslation.ComboBox is not null)
        {
            this.ToolStripComboBoxPrimaryTranslation.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.ToolStripComboBoxPrimaryTranslation.ComboBox.DrawItem += this.ToolStripComboBox_DrawItem;
            this.ToolStripComboBoxPrimaryTranslation.ComboBox.DropDownClosed += this.ToolStripComboBox_DropDownClosed;
        }

        if (this.ToolStripComboBoxSecondaryTranslation.ComboBox is not null)
        {
            this.ToolStripComboBoxSecondaryTranslation.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.ToolStripComboBoxSecondaryTranslation.ComboBox.DrawItem += this.ToolStripComboBox_DrawItem;
            this.ToolStripComboBoxSecondaryTranslation.ComboBox.DropDownClosed += this.ToolStripComboBox_DropDownClosed;
        }

        if (this.ToolStripComboBoxResource.ComboBox is not null)
        {
            this.ToolStripComboBoxResource.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.ToolStripComboBoxResource.ComboBox.DrawItem += this.ToolStripComboBox_DrawItem;
            this.ToolStripComboBoxResource.ComboBox.DropDownClosed += this.ToolStripComboBox_DropDownClosed;
        }

        this.ToolStripMenuItemIgnoreCase.Checked = Settings.Default.InterlinearIgnoresCase;
        this.ToolStripMenuItemIgnoreDiacritics.Checked = Settings.Default.InterlinearIgnoresDiacritics;
        this.ToolStripMenuItemIgnorePunctuation.Checked = Settings.Default.InterlinearIgnoresPunctuation;
        this.ToolStripMenuItemShowItalics.Checked = Settings.Default.RenderItalics;
        this.ToolStripMenuItemDebugMode.Checked = Settings.Default.IsDebug;
        this.ToolStripMenuItemLegacyBrowser.Checked = Settings.Default.IsLegacyBrowser;
        this.IsDeveloper = Settings.Default.IsDeveloper;
        this.ToolStripMenuItemDeveloperMode.Visible = IsDebug || this.IsDeveloper;
        this.ToolStripSeparatorDebugMode.Visible = IsDebug || this.IsDeveloper;
        if (!this.IsDeveloper)
        {
            // As the default is false, this would not have been triggered if not in developer mode
            await this.UpdateDeveloperMode();
        }

        // Initialise the web browser
        await this.InitialiseWebBrowser(false);

        // Setup the cache
        this.LoadSqlCache(Settings.Default.SqlConnectionString);

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
            await this.WebViewMain.SetInnerHtmlAsync(Html.LoadingCodeBody);
            await this.WebViewResource.SetInnerHtmlAsync(Html.LoadingCodeBody);
        }
    }

    /// <summary>
    /// Initializes the WebView2 asynchronously.
    /// </summary>
    private async Task InitialiseAsync()
    {
        // Edge specific configuration
        if (this.WebViewMain is WebView2 webViewMain && this.WebViewResource is WebView2 webViewResource)
        {
            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(SettingsDirectory, "WebView2"));
            await webViewMain.EnsureCoreWebView2Async(environment);
            await webViewResource.EnsureCoreWebView2Async(environment);
        }

        // General configuration
        this.WebViewMain.DeveloperMode = Settings.Default.IsDebug;
        this.SetupWebPage(this.WebViewMain);
        this.WebViewResource.DeveloperMode = Settings.Default.IsDebug;
        this.SetupWebPage(this.WebViewResource);

        this.webViewInitialised = true;
    }

    /// <summary>
    /// Initializes the Web Browser.
    /// </summary>
    /// <param name="removeExisting">if set to <c>true</c>, remove the existing web browser control.</param>
    private async Task InitialiseWebBrowser(bool removeExisting)
    {
        // Suspend the layouts
        this.SplitContainerMain.Panel1.SuspendLayout();
        this.SplitContainerMain.Panel2.SuspendLayout();

        // Remove the existing controls
        if (removeExisting)
        {
            this.SplitContainerMain.Panel1.Controls.Clear();
            this.SplitContainerMain.Panel2.Controls.Clear();
            this.WebViewMain.Dispose();
            this.WebViewResource.Dispose();
        }

        // Get the correct web browser controls
        IWebBrowser webViewMain;
        IWebBrowser webViewResource;
        if (this.IsLegacyBrowser)
        {
            webViewMain = new LegacyWebBrowser();
            webViewResource = new LegacyWebBrowser();
        }
        else
        {
            webViewMain = new EdgeWebBrowser();
            webViewResource = new EdgeWebBrowser();
        }

        // Setup the main web browser
        webViewMain.Initialise("WebViewMain", 1);
        if (webViewMain is Control webViewMainControl)
        {
            this.SplitContainerMain.Panel1.Controls.Add(webViewMainControl);
        }

        this.WebViewMain = webViewMain;

        // Setup the resource web browser
        webViewResource.Initialise("WebViewResource", 2);
        if (webViewResource is Control webViewResourceControl)
        {
            this.SplitContainerMain.Panel2.Controls.Add(webViewResourceControl);
        }

        this.WebViewResource = webViewResource;

        // Resume the layouts
        this.SplitContainerMain.Panel1.ResumeLayout();
        this.SplitContainerMain.Panel2.ResumeLayout();

        // Initialise the WebView2
        if (this.IsLegacyBrowser)
        {
            this.SetupWebPage(this.WebViewMain);
            this.SetupWebPage(this.WebViewResource);
        }
        else
        {
            try
            {
                await this.InitialiseAsync();
            }
            catch (Exception)
            {
                if (MessageBox.Show(string.Format(Resources.WebViewNotFound), $@"Cannot Start {Program.Title}", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    // Download WebView2
                    Process.Start(new ProcessStartInfo("https://go.microsoft.com/fwlink/p/?LinkId=2124703")
                    {
                        UseShellExecute = true,
                        Verb = "open",
                    });
                    Application.Exit();
                }
                else
                {
                    // Run in Legacy Mode
                    this.IsLegacyBrowser = true;
                    await this.InitialiseWebBrowser(removeExisting);
                    return;
                }
            }
        }

        // Render the passage, if we are changing web browsers
        if (removeExisting)
        {
            await this.ShowPassage(true, true);
        }
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

        // Create a list of blocked and enabled providers
        List<string> blockedProviders = new List<string>();

        if (this.IsDeveloper)
        {
            // Load the API.Bible Provider
            string bibleApiKey = Settings.Default.BibleApiKey;
            if (!string.IsNullOrWhiteSpace(bibleApiKey))
            {
                this.providers.Add(new BibleApi(Options.Create(new BibleApiOptions { ApiKey = bibleApiKey }), this.cache));
            }

            // Load the Biblia Provider
            string bibliaApiKey = Settings.Default.BibliaApiKey;
            if (!string.IsNullOrWhiteSpace(bibliaApiKey))
            {
                this.providers.Add(new BibliaApi(Options.Create(new BibliaApiOptions { ApiKey = bibliaApiKey }), this.cache));
            }

            // Load the Digital Bible Platform Provider
            string digitalBiblePlatformApiKey = Settings.Default.DigitalBiblePlatformApiKey;
            if (!string.IsNullOrWhiteSpace(digitalBiblePlatformApiKey))
            {
                this.providers.Add(new DigitalBiblePlatformApi(Options.Create(new DigitalBiblePlatformApiOptions { ApiKey = digitalBiblePlatformApiKey }), this.cache));
            }

            // Load the ESV Provider
            string esvApiKey = Settings.Default.EsvApiKey;
            if (!string.IsNullOrWhiteSpace(esvApiKey))
            {
                this.providers.Add(new EsvBible(Options.Create(new EsvBibleOptions { ApiKey = esvApiKey }), this.cache));
            }

            // Load the Logos Provider
            this.providers.Add(new LogosProvider());

            // Load the NET Provider
            this.providers.Add(new NetBible(this.cache));

            // Load the NLT Provider
            string nltApiKey = Settings.Default.NltApiKey;
            if (!string.IsNullOrWhiteSpace(nltApiKey))
            {
                this.providers.Add(new NltBible(Options.Create(new NltBibleOptions { ApiKey = nltApiKey }), this.cache));
            }

            // Get the list of blocked providers
            blockedProviders = Settings.Default.BlockedProviders?.Cast<string>().ToList() ?? new List<string>();

            // Set the renderer
            if (this.renderer is not Renderer)
            {
                this.renderer.Dispose();
                this.renderer = new Renderer();
            }

            // Load the Bundled Translations Provider
            this.providers.Add(new BundledTranslations(this.renderer));
        }
        else
        {
            // Only one provider is allowed if not developer
            this.providers.Add(new GoToBibleApi(this.cache));

            // Set the renderer
            if (this.renderer is not GotoBibleApiRenderer)
            {
                this.renderer.Dispose();
                this.renderer = new GotoBibleApiRenderer();
            }
        }

        // Set the providers for the renderer
        List<IProvider> enabledProviders = this.providers.Where(p => !blockedProviders.Contains(p.Id)).ToList();

        // Set the renderer providers
        this.renderer.Providers = enabledProviders.AsReadOnly();

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
    /// <param name="translationProviders">The translation providers.</param>
    /// <param name="primaryTranslation">The primary translation.</param>
    /// <param name="secondaryTranslation">The secondary translation.</param>
    /// <param name="resource">The open resource.</param>
    private async Task LoadTranslationComboBoxes(IList<IProvider> translationProviders, string primaryTranslation, string secondaryTranslation, string resource)
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
            Text = Resources.NoTranslation,
        });
        this.ToolStripComboBoxResource.Items.Add(new ComboBoxItem
        {
            Text = Resources.NoTranslation,
        });

        // Get the full list of translations
        List<string> blockedLanguages = Settings.Default.BlockedLanguages?.Cast<string>().ToList() ?? new List<string>();
        List<string> blockedTranslations = Settings.Default.BlockedTranslations?.Cast<string>().ToList() ?? new List<string>();
        List<string> blockedCommentaries = Settings.Default.BlockedCommentaries?.Cast<string>().ToList() ?? new List<string>();
        List<Translation> unsortedCommentaries = new List<Translation>();
        List<Translation> unsortedTranslations = new List<Translation>();
        foreach (IProvider provider in translationProviders)
        {
            await foreach (Translation translation in provider.GetTranslationsAsync())
            {
                if (translation.Commentary)
                {
                    unsortedCommentaries.Add(translation);
                }
                else
                {
                    // Clean up any names we are displaying
                    if (ApiProvider.NameSubstitutions.TryGetValue(translation.Name, out string? translationName))
                    {
                        translation.Name = translationName;
                    }

                    unsortedTranslations.Add(translation);
                }
            }
        }

        // Store the translations for use by other methods
        this.translations.Clear();
        this.translations.AddRange(unsortedTranslations.OrderBy(t => t.Language, new LanguageComparer()).ThenBy(t => t.Name));
        this.commentaries.Clear();
        this.commentaries.AddRange(unsortedCommentaries.OrderBy(t => t.Name));
        this.ToolStripMenuItemConfigure.Enabled = true;
        this.ToolStripButtonApparatusGenerator.Enabled = true;

        // Load the commentaries and translations from the providers
        int primaryTranslationSelectedIndex = 0;
        int secondaryTranslationSelectedIndex = 0;
        int resourceSelectedIndex = 0;
        int resourceIndex = 0;
        int translationIndex = 0;
        string lastLanguage = string.Empty;

        // Add the commentaries that are not blocked to resources
        foreach (Translation commentary in this.commentaries.Where(c => !blockedCommentaries.Contains(c.UniqueKey())))
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
                Text = commentary.UniqueName(this.commentaries),
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

        List<Translation> unblockedTranslations = this.translations.Where(t => !blockedTranslations.Contains(t.UniqueKey())).ToList();
        foreach (Translation translation in this.translations)
        {
            // If this translation is not blocked
            string translationLanguage = translation.Language ?? Resources.UnknownLanguage;
            if (!blockedTranslations.Contains(translation.UniqueKey()) && !blockedLanguages.Contains(translationLanguage))
            {
                // Add a heading, if required
                if (translationLanguage != lastLanguage)
                {
                    ComboBoxItem headingComboBoxItem = new ComboBoxItem
                    {
                        Bold = true,
                        Selectable = false,
                        Text = translationLanguage,
                    };
                    this.ToolStripComboBoxPrimaryTranslation.Items.Add(headingComboBoxItem);
                    this.ToolStripComboBoxSecondaryTranslation.Items.Add(headingComboBoxItem);
                    this.ToolStripComboBoxResource.Items.Add(headingComboBoxItem);
                    lastLanguage = translationLanguage;
                    resourceIndex++;
                    translationIndex++;
                }

                // Set up the combo box item
                TranslationComboBoxItem comboBoxItem = new TranslationComboBoxItem
                {
                    Bold = false,
                    Selectable = true,
                    Text = translation.UniqueName(unblockedTranslations),
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
                Text = Resources.NoTranslation,
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
    /// Processes the suggestions from the renderer.
    /// </summary>
    private async Task ProcessSuggestions()
    {
        if (this.renderedPassage.Suggestions.IgnoreCaseDiacriticsAndPunctuation
            && MessageBox.Show(
                Resources.IgnoreCaseDiacriticsAndPunctuation,
                Program.Title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information) == DialogResult.Yes)
        {
            this.ToolStripMenuItemIgnoreCase.Checked = true;
            this.ToolStripMenuItemIgnoreDiacritics.Checked = true;
            this.ToolStripMenuItemIgnorePunctuation.Checked = true;
            await this.ShowPassage(true, false);
        }
        else if (this.renderedPassage.Suggestions.NavigateToChapter is not null)
        {
            // If this book is not present, go to the suggested book
            this.ToolStripTextBoxPassage.Text = this.renderedPassage.Suggestions.NavigateToChapter.ToString();
            await this.ShowPassage(true, true);
        }
    }

    /// <summary>
    /// Sets up the web page.
    /// </summary>
    /// <param name="webView">The web view.</param>
    private void SetupWebPage(IWebBrowser webView)
    {
        try
        {
            string body = this.translationsLoaded && !this.translations.Any() ? string.Empty : Html.LoadingCodeBody;
            webView.NavigateToString($"<!DOCTYPE html>\n<html><head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" /><style type=\"text/css\">{this.parameters.RenderCss()}{Html.LoadingCodeCss}</style></head><body>{body}</body></html>");
        }
        catch (InvalidOperationException)
        {
        }
    }

    /// <summary>
    /// Handles the Click event of the Apparatus Generator ToolStripButton.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ToolStripButtonApparatusGenerator_Click(object sender, EventArgs e)
    {
        FormApparatusGenerator formApparatusGenerator = new FormApparatusGenerator(this.parameters.PrimaryTranslation, this.renderer, this.translations, this.providers);
        formApparatusGenerator.ShowDialog();
    }

    /// <summary>
    /// Handles the Click event of the Export ToolStripButton.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ToolStripButtonExport_Click(object sender, EventArgs e)
    {
        this.SaveFileDialogMain.FileName = this.parameters.PassageReference.ChapterReference + ".txt";
        if (this.SaveFileDialogMain.ShowDialog() == DialogResult.OK)
        {
            // Render for export to Accordance
            RenderedPassage renderedPassageForExport = await this.renderer.RenderAsync(this.parameters with { Format = RenderFormat.Accordance }, false);
            await File.WriteAllTextAsync(this.SaveFileDialogMain.FileName, renderedPassageForExport.Content);
        }
    }

    /// <summary>
    /// Handles the Click event of the GitHub ToolStripButton.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ToolStripButtonGitHub_Click(object sender, EventArgs e)
        => Process.Start(new ProcessStartInfo("https://github.com/pmachapman/GoTo.Bible") { UseShellExecute = true, Verb = "open" });

    /// <summary>
    /// Handles the Click event of the Navigate Back ToolStripButton control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ToolStripButtonNavigateBack_Click(object sender, EventArgs e)
    {
        if (this.renderedPassage.PreviousPassage.IsValid)
        {
            this.ToolStripTextBoxPassage.Text = this.renderedPassage.PreviousPassage.ChapterReference.ToString();
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
            this.ToolStripTextBoxPassage.Text = this.renderedPassage.NextPassage.ChapterReference.ToString();
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
            IProvider? provider = this.providers.FirstOrDefault(p => p.Id == primaryComboBoxItem.Provider) ?? this.providers.FirstOrDefault();
            if (provider is not null)
            {
                await foreach (Book book in provider.GetBooksAsync(primaryComboBoxItem.Code, false))
                {
                    if (!suggestions.Contains(book.Name))
                    {
                        suggestions.Add(book.Name);
                    }
                }
            }
        }

        // If we have a translation in the secondary combo box
        if (this.ToolStripComboBoxSecondaryTranslation.SelectedItem is TranslationComboBoxItem secondaryComboBoxItem)
        {
            // Add the book names to the suggestions list
            IProvider? provider = this.providers.FirstOrDefault(p => p.Id == secondaryComboBoxItem.Provider) ?? this.providers.FirstOrDefault();
            if (provider is not null)
            {
                await foreach (Book book in provider.GetBooksAsync(secondaryComboBoxItem.Code, false))
                {
                    if (!suggestions.Contains(book.Name))
                    {
                        suggestions.Add(book.Name);
                    }
                }
            }
        }

        // Enable autocomplete
        if (this.ToolStripTextBoxPassage.TextBox is not null)
        {
            AutoSuggest.Disable(this.ToolStripTextBoxPassage.TextBox);
            AutoSuggest.Enable(this.ToolStripTextBoxPassage.TextBox, suggestions.ToArray());
        }
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
                await this.WebViewMain.SetInnerHtmlAsync(Html.LoadingCodeBody);
            }

            if (updateResource)
            {
                await this.WebViewResource.SetInnerHtmlAsync(Html.LoadingCodeBody);
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
            BackgroundColour = this.ColourDialogBackground.Color.AsRenderColour(),
            Font = this.FontDialogMain.Font.AsRenderFont(),
            ForegroundColour = this.FontDialogMain.Color.AsRenderColour(),
            Format = RenderFormat.Html,
            HighlightColour = this.ColourDialogHighlight.Color.AsRenderColour(),
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

        // Save settings
        if (!string.IsNullOrWhiteSpace(this.renderedPassage.Content) && (this.primaryWindow || Program.Forms.Count == 1))
        {
            Settings.Default.InterlinearIgnoresCase = this.parameters.InterlinearIgnoresCase;
            Settings.Default.InterlinearIgnoresDiacritics = this.parameters.InterlinearIgnoresDiacritics;
            Settings.Default.InterlinearIgnoresPunctuation = this.parameters.InterlinearIgnoresPunctuation;
            Settings.Default.IsDebug = this.parameters.IsDebug;
            Settings.Default.IsDeveloper = this.IsDeveloper;
            Settings.Default.IsLegacyBrowser = this.IsLegacyBrowser;
            Settings.Default.RenderItalics = this.parameters.RenderItalics;
            Settings.Default.Passage = this.parameters.PassageReference.Display;
            Settings.Default.PrimaryTranslation = this.parameters.PrimaryTranslation;
            Settings.Default.SecondaryTranslation = this.parameters.SecondaryTranslation;
            Settings.Default.Resource = resource;
            Settings.Default.BackgroundCustomColours = string.Join(',', this.ColourDialogBackground.CustomColors.Select(c => c.ToString()));
            Settings.Default.BackgroundColour = ColorTranslator.ToOle(this.ColourDialogBackground.Color);
            Settings.Default.HighlightCustomColours = string.Join(',', this.ColourDialogHighlight.CustomColors.Select(c => c.ToString()));
            Settings.Default.HighlightColour = ColorTranslator.ToOle(this.ColourDialogHighlight.Color);
            Settings.Default.ForegroundColour = ColorTranslator.ToOle(this.FontDialogMain.Color);
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
            Settings.Default.Font = converter.ConvertToString(this.FontDialogMain.Font);
            Settings.Default.Save();
        }

        // Show the content
        try
        {
            if (updateMain)
            {
                await this.WebViewMain.SetInnerHtmlAsync(this.renderedPassage.Content);
            }

            if (updateResource)
            {
                await this.WebViewResource.SetInnerHtmlAsync(renderedResource.Content);
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
    private void SystemMenuAbout_Click()
    {
        Assembly assembly = typeof(FormMain).Assembly;
        Version version = assembly.GetName().Version ?? new Version(1, 0);
        object[] attribs = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
        string copyright = (attribs.FirstOrDefault() as AssemblyCopyrightAttribute)?.Copyright ?? string.Empty;
        NativeMethods.ShellAbout(
            this.Handle,
            "Windows",
            $"{this.Text} for Windows {version.Major}.{version.Minor}.{version.Build}{Environment.NewLine}{copyright}",
            this.Icon?.Handle ?? 0
        );
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
                Screen? screen = screens.FirstOrDefault(s => s.Bounds != thisScreen.Bounds);
                if (screen is not null)
                {
                    formMain.Location = screen.WorkingArea.Location;
                    formMain.Size = Settings.Default.WindowSize;
                    formMain.WindowState = FormWindowState.Maximized;

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
        if (this.ToolStripComboBoxSecondaryTranslation.Text != Resources.NoTranslation)
        {
            int secondaryIndex = this.ToolStripComboBoxSecondaryTranslation.SelectedIndex;
            this.ToolStripComboBoxSecondaryTranslation.SelectedIndex = this.ToolStripComboBoxPrimaryTranslation.SelectedIndex + 1;
            this.ToolStripComboBoxPrimaryTranslation.SelectedIndex = secondaryIndex - 1;
            await this.ShowPassage(true, false);
        }
    }

    /// <summary>
    /// Handles the Click event of the Web Browser ToolStripButton.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void ToolStripButtonWebBrowser_Click(object sender, EventArgs e)
        => Process.Start(new ProcessStartInfo(this.parameters.AsUrl().ToString()) { UseShellExecute = true, Verb = "open" });

    /// <summary>
    /// Handles the SelectedIndexChanged event of the Resource ToolStripComboBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ToolStripComboBoxResource_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (this.ToolStripComboBoxResource.SelectedItem is ComboBoxItem { Selectable: false })
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
        if (this.ToolStripComboBoxPrimaryTranslation.SelectedItem is ComboBoxItem { Selectable: false })
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
                && this.ToolStripComboBoxSecondaryTranslation.SelectedItem is TranslationComboBoxItem secondaryItem
                && ((primaryItem.Language == "Greek" && secondaryItem.Language is not ("Greek" or null))
                    || (primaryItem.Language == "Hebrew" && secondaryItem.Language is not ("Hebrew" or null))
                    || (secondaryItem.Language == "Greek" && primaryItem.Language is not ("Greek" or null))
                    || (secondaryItem.Language == "Hebrew" && primaryItem.Language is not ("Hebrew" or null))))
            {
                MessageBox.Show(Resources.CannotShowInterlinear, Program.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.ToolStripComboBoxSecondaryTranslation.SelectedIndex = 0;
            }

            // Show the passage
            await this.SetupAutoCompleteAsync();
            if (!string.IsNullOrWhiteSpace(this.ToolStripTextBoxPassage.Text))
            {
                await this.ShowPassage(true, false);
                await this.ProcessSuggestions();
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
        if (this.ToolStripComboBoxSecondaryTranslation.SelectedItem is ComboBoxItem { Selectable: false })
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
                && this.ToolStripComboBoxSecondaryTranslation.SelectedItem is TranslationComboBoxItem secondaryItem
                && ((primaryItem.Language == "Greek" && secondaryItem.Language is not ("Greek" or null))
                    || (primaryItem.Language == "Hebrew" && secondaryItem.Language is not ("Hebrew" or null))
                    || (secondaryItem.Language == "Greek" && primaryItem.Language is not ("Greek" or null))
                    || (secondaryItem.Language == "Hebrew" && primaryItem.Language is not ("Hebrew" or null))))
            {
                MessageBox.Show(Resources.CannotShowInterlinear, Program.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.ToolStripComboBoxSecondaryTranslation.SelectedIndex = 0;
            }

            // Show the passage
            await this.SetupAutoCompleteAsync();
            if (!string.IsNullOrWhiteSpace(this.ToolStripTextBoxPassage.Text))
            {
                await this.ShowPassage(true, false);
                await this.ProcessSuggestions();
            }
        }
    }

    /// <summary>
    /// Handles the DrawItem event of the ToolStripComboBox controls.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DrawItemEventArgs"/> instance containing the event data.</param>
    private void ToolStripComboBox_DrawItem(object? sender, DrawItemEventArgs e)
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
                e.Graphics.DrawString(item.Text, font, SystemBrushes.ControlText, e.Bounds, StringFormat.GenericTypographic);
                size = e.Graphics.MeasureString(item.Text, font);
            }
            else
            {
                using Font font = new Font(comboBox.Font, FontStyle.Regular);
                e.Graphics.DrawString(item.Text, font, SystemBrushes.ControlText, e.Bounds, StringFormat.GenericTypographic);
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
        DialogResult dialogResult = this.ColourDialogBackground.ShowDialog();
        if (dialogResult == DialogResult.OK)
        {
            // Update the rendering parameters
            this.parameters.BackgroundColour = this.ColourDialogBackground.Color.AsRenderColour();

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
        string key = Settings.Default.BibleApiKey;
        using FormApiKey formApiKey = new FormApiKey(key, "API.Bible", new Uri("https://scripture.api.bible/", UriKind.Absolute), Resources.BibleApiIcon);
        formApiKey.ShowDialog(this);

        // Only load the provider if the key has changed
        if (key != formApiKey.Key)
        {
            // Save the key
            Settings.Default.BibleApiKey = formApiKey.Key;
            Settings.Default.Save();

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
        string key = Settings.Default.BibliaApiKey;
        using FormApiKey formApiKey = new FormApiKey(key, "Biblia API", new Uri("https://api.biblia.com/v1/Users/SignIn", UriKind.Absolute), Resources.BibliaIcon);
        formApiKey.ShowDialog(this);

        // Only load the provider if the key has changed
        if (key != formApiKey.Key)
        {
            // Save the key
            Settings.Default.BibliaApiKey = formApiKey.Key;
            Settings.Default.Save();

            // Reload the providers and translations
            await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
        }
    }

    /// <summary>
    /// Handles the Click event of the Load Blocked Translations List ToolStripMenuItem.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ToolStripMenuItemBlockedTranslations_Click(object sender, EventArgs e)
    {
        // Generate the new list of blocked translations
        string[] blockedTranslations =
            (Settings.Default.BlockedTranslations?.Cast<string>() ?? new List<string>())
            .Concat(ApiProvider.BlockedTranslations).Distinct().ToArray();

        // Save the blocked translations
        Settings.Default.BlockedTranslations?.Clear();
        Settings.Default.BlockedTranslations ??= new StringCollection();

        Settings.Default.BlockedTranslations.AddRange(blockedTranslations);
        Settings.Default.Save();

        // Reload the providers and translations
        await this.LoadTranslationComboBoxes(this.renderer.Providers.ToList(), string.Empty, string.Empty, string.Empty);
    }

    /// <summary>
    /// Handles the Click event of the Commentaries ToolStripMenuItem.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ToolStripMenuItemCommentaries_Click(object sender, EventArgs e)
    {
        // Show the show/hide commentaries dialog
        List<string> blockedCommentaries = Settings.Default.BlockedCommentaries?.Cast<string>().ToList() ?? new List<string>();
        Dictionary<string, string> items = this.commentaries.ToDictionary(k => k.UniqueKey(), v => v.UniqueName(this.commentaries));
        using FormCheckBoxList formCheckBoxList = new FormCheckBoxList(items, blockedCommentaries, "Configure Commentaries", Resources.CommentariesIcon);
        DialogResult dialogResult = formCheckBoxList.ShowDialog(this);
        if (dialogResult == DialogResult.OK)
        {
            // Save the blocked commentaries
            Settings.Default.BlockedCommentaries?.Clear();
            Settings.Default.BlockedCommentaries ??= new StringCollection();

            Settings.Default.BlockedCommentaries.AddRange(formCheckBoxList.UncheckedItems.ToArray());
            Settings.Default.Save();

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
        this.WebViewMain.DeveloperMode = this.ToolStripMenuItemDebugMode.Checked;
        this.SetupWebPage(this.WebViewMain);
        this.WebViewResource.DeveloperMode = this.ToolStripMenuItemDebugMode.Checked;
        this.SetupWebPage(this.WebViewResource);

        // Debug the passage renderer
        await this.ShowPassage(true, true);
    }

    /// <summary>
    /// Handles the Click event of the ToolStripMenuItemDeveloperMode control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ToolStripMenuItemDeveloperMode_Click(object sender, EventArgs e) => await this.UpdateDeveloperMode();

    /// <summary>
    /// Handles the Click event of the Digital Bible Platform ToolStripMenuItem.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ToolStripMenuItemDigitalBiblePlatform_Click(object sender, EventArgs e)
    {
        // Show the Digital Bible Platform API Key Dialog
        string key = Settings.Default.DigitalBiblePlatformApiKey;
        using FormApiKey formApiKey = new FormApiKey(key, "Digital Bible Platform API", new Uri("https://www.digitalbibleplatform.com/", UriKind.Absolute), Resources.DigitalBiblePlatformIcon);
        formApiKey.ShowDialog(this);

        // Only load the provider if the key has changed
        if (key != formApiKey.Key)
        {
            // Save the key
            Settings.Default.DigitalBiblePlatformApiKey = formApiKey.Key;
            Settings.Default.Save();

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
        string key = Settings.Default.EsvApiKey;
        using FormApiKey formApiKey = new FormApiKey(key, "ESV API", new Uri("https://api.esv.org/", UriKind.Absolute), Resources.EsvIcon);
        formApiKey.ShowDialog(this);

        // Only load the provider if the key has changed
        if (key != formApiKey.Key)
        {
            // Save the key
            Settings.Default.EsvApiKey = formApiKey.Key;
            Settings.Default.Save();

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
            this.parameters.ForegroundColour = this.FontDialogMain.Color.AsRenderColour();

            // Update the CSS
            this.SetupWebPage(this.WebViewMain);
            this.SetupWebPage(this.WebViewResource);

            // Render the passage
            await this.ShowPassage(true, true);
        }
    }

    /// <summary>
    /// Handles the Click event of the Highlight Colour ToolStripMenuItem.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private async void ToolStripMenuItemHighlightColour_Click(object sender, EventArgs e)
    {
        DialogResult dialogResult = this.ColourDialogHighlight.ShowDialog();
        if (dialogResult == DialogResult.OK)
        {
            // Update the rendering parameters
            this.parameters.HighlightColour = this.ColourDialogHighlight.Color.AsRenderColour();

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
    /// Handles the Click event of the Languages ToolStripMenuItem.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private async void ToolStripMenuItemLanguages_Click(object sender, EventArgs e)
    {
        // Show the show/hide languages dialog
        List<string> blockedLanguages = Settings.Default.BlockedLanguages?.Cast<string>().ToList() ?? new List<string>();
        Dictionary<string, string> items = this.translations
            .Select(t => t.Language ?? Resources.UnknownLanguage)
            .Distinct()
            .ToDictionary(k => k, v => v);
        using FormCheckBoxList formCheckBoxList = new FormCheckBoxList(items, blockedLanguages, "Configure Languages", Resources.LanguagesIcon);
        DialogResult dialogResult = formCheckBoxList.ShowDialog(this);
        if (dialogResult == DialogResult.OK)
        {
            // Save the blocked providers
            Settings.Default.BlockedLanguages?.Clear();
            Settings.Default.BlockedLanguages ??= new StringCollection();

            Settings.Default.BlockedLanguages.AddRange(formCheckBoxList.UncheckedItems.ToArray());
            Settings.Default.Save();

            // Reload the providers and translations
            await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
        }
    }

    /// <summary>
    /// Handles the Click event of the Legacy Browser ToolStripMenuItem.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private async void ToolStripMenuItemLegacyBrowser_Click(object sender, EventArgs e) => await this.InitialiseWebBrowser(true);

    /// <summary>
    /// Handles the Click event of the NLT ToolStripMenuItem.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ToolStripMenuItemNlt_Click(object sender, EventArgs e)
    {
        // Show the NLT API Key Dialog
        string key = Settings.Default.NltApiKey;
        using FormApiKey formApiKey = new FormApiKey(key, "NLT API", new Uri("https://api.nlt.to/", UriKind.Absolute), Resources.NltIcon);
        formApiKey.ShowDialog(this);

        // Only load the provider if the key has changed
        if (key != formApiKey.Key)
        {
            // Save the key
            Settings.Default.NltApiKey = formApiKey.Key;
            Settings.Default.Save();

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
        List<string> blockedProviders = Settings.Default.BlockedProviders?.Cast<string>().ToList() ?? new List<string>();
        Dictionary<string, string> items = this.providers.ToDictionary(k => k.Id, v => v.Name);
        using FormCheckBoxList formCheckBoxList = new FormCheckBoxList(items, blockedProviders, "Configure Providers", Resources.ProviderIcon);
        DialogResult dialogResult = formCheckBoxList.ShowDialog(this);
        if (dialogResult == DialogResult.OK)
        {
            // Save the blocked providers
            Settings.Default.BlockedProviders?.Clear();
            Settings.Default.BlockedProviders ??= new StringCollection();

            Settings.Default.BlockedProviders.AddRange(formCheckBoxList.UncheckedItems.ToArray());
            Settings.Default.Save();

            // Reload the providers and translations
            await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
        }
    }

    /// <summary>
    /// Handles the Click event of the SettingsDirectory ToolStripMenuItem.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void ToolStripMenuItemSettingsDirectory_Click(object sender, EventArgs e)
        => Process.Start(new ProcessStartInfo(SettingsDirectory) { UseShellExecute = true, Verb = "open" });

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
        string key = Settings.Default.SqlConnectionString;
        using FormApiKey formApiKey = new FormApiKey(key, "SQL Server", new Uri("https://www.microsoft.com/en-us/sql-server/sql-server-downloads/", UriKind.Absolute), Resources.SqlIcon);
        formApiKey.ShowDialog(this);

        // Only load the provider if the key has changed
        if (key != formApiKey.Key)
        {
            // Save the key
            Settings.Default.SqlConnectionString = formApiKey.Key;
            Settings.Default.Save();

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
        List<string> blockedTranslations = Settings.Default.BlockedTranslations?.Cast<string>().ToList() ?? new List<string>();
        Dictionary<string, string> items = this.translations.ToDictionary(k => k.UniqueKey(), v => v.UniqueName(this.translations));
        using FormCheckBoxList formCheckBoxList = new FormCheckBoxList(items, blockedTranslations, "Configure Translations", Resources.TranslationsIcon);
        DialogResult dialogResult = formCheckBoxList.ShowDialog(this);
        if (dialogResult == DialogResult.OK)
        {
            // Save the blocked translations
            Settings.Default.BlockedTranslations?.Clear();
            Settings.Default.BlockedTranslations ??= new StringCollection();

            Settings.Default.BlockedTranslations.AddRange(formCheckBoxList.UncheckedItems.ToArray());
            Settings.Default.Save();

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

    /// <summary>
    /// Updates the app based on the <see cref="IsDeveloper" /> value.
    /// </summary>
    /// <returns>The task.</returns>
    private async Task UpdateDeveloperMode()
    {
        // Update the menus
        this.ToolStripMenuItemEnterApiKeys.Visible = this.IsDeveloper;
        this.ToolStripMenuItemProviders.Visible = this.IsDeveloper;
        this.ToolStripMenuItemDebugMode.Visible = this.IsDeveloper;
        this.ToolStripMenuItemLegacyBrowser.Visible = this.IsDeveloper;
        this.ToolStripMenuItemBlockedTranslations.Visible = this.IsDeveloper;
        this.ToolStripMenuItemSettingsDirectory.Visible = this.IsDeveloper;

        // Reload the providers and translations
        await this.LoadTranslationComboBoxes(this.LoadProviders(), string.Empty, string.Empty, string.Empty);
    }
}
