// -----------------------------------------------------------------------
// <copyright file="FormApparatusGenerator.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using GoToBible.Engine;
    using GoToBible.Model;
    using GoToBible.Windows.Properties;

    /// <summary>
    /// The Apparatus Generator Form.
    /// </summary>
    /// <seealso cref="Form" />
    [SupportedOSPlatform("windows")]
    public partial class FormApparatusGenerator : Form
    {
        /// <summary>
        /// The available providers.
        /// </summary>
        private readonly IEnumerable<IProvider> providers;

        /// <summary>
        /// The bible data renderer.
        /// </summary>
        private readonly IRenderer renderer;

        /// <summary>
        /// The selected base translation.
        /// </summary>
        private readonly string selectedBaseTranslation;

        /// <summary>
        /// The available translations.
        /// </summary>
        private readonly IEnumerable<Translation> translations;

        /// <summary>
        /// A value indicating whether the books checked box list is updating.
        /// </summary>
        private bool checkedListBoxBooksIsUpdating;

        /// <summary>
        /// A value indicating whether the comparison texts checked box list is updating.
        /// </summary>
        private bool checkedListBoxComparisonTextsIsUpdating;

        /// <summary>
        /// A value indicating whether the select all books check box is updating.
        /// </summary>
        private bool checkBoxSelectAllBooksIsUpdating;

        /// <summary>
        /// A value indicating whether the select all comparison texts check box is updating.
        /// </summary>
        private bool checkBoxSelectAllComparisonTextsIsUpdating;

        /// <summary>
        /// The selected file names.
        /// </summary>
        private string[] selectedFileNames = Array.Empty<string>();

        /// <summary>
        /// Initialises a new instance of the <see cref="FormApparatusGenerator" /> class.
        /// </summary>
        /// <param name="selectedBaseTranslation">The selected base translation.</param>
        /// <param name="renderer">The renderer to use.</param>
        /// <param name="translations">A list of the available translations.</param>
        /// <param name="providers">A list of the available providers.</param>
        public FormApparatusGenerator(
            string selectedBaseTranslation,
            IRenderer renderer,
            IEnumerable<Translation> translations,
            IEnumerable<IProvider> providers)
        {
            this.InitializeComponent();
            this.renderer = renderer;
            this.selectedBaseTranslation = selectedBaseTranslation;
            this.translations = translations;
            this.providers = providers;
        }

        /// <summary>
        /// Handles the Click event of the Browse Button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            if (this.OpenFileDialogMain.ShowDialog() == DialogResult.OK)
            {
                this.selectedFileNames = this.OpenFileDialogMain.FileNames;
                this.TextBoxIncludeApparatusData.Text = string.Join(";", this.OpenFileDialogMain.FileNames);
            }
        }

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
        private async void ButtonOk_Click(object sender, EventArgs e)
        {
            // Check for valid inputs
            if (!this.selectedFileNames.Any() && this.CheckedListBoxComparisonTexts.SelectedItems.Count == 0)
            {
                MessageBox.Show(@"You must select at least one comparison translation, or additional apparatus data file.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (this.CheckedListBoxBooks.SelectedItems.Count == 0)
            {
                MessageBox.Show(@"You must select at least one book.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // TODO: Show and start the progress bar

            // If we have only one comparison translation, and no addition apparatus, just generate it
            if (this.CheckedListBoxComparisonTexts.SelectedItems.Count == 1 && !this.selectedFileNames.Any())
            {
                // TODO: Flesh out the parameters
                RenderingParameters parameters;
                if (this.RadioButtonCsv.Checked)
                {
                    parameters = new SpreadsheetRenderingParameters
                    {
                        Format = RenderFormat.Spreadsheet,
                    };
                }
                else
                {
                    parameters = new ApparatusRenderingParameters
                    {
                        Format = RenderFormat.Apparatus,
                    };
                }

                // Render the output
                RenderedPassage passage = await this.renderer.RenderAsync(parameters, true);
                if (string.IsNullOrWhiteSpace(passage.Content))
                {
                    // TODO: Hide the progress bar
                    MessageBox.Show(@"There was an unknown error rendering the apparatus.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    // TODO: Save the file
                    // TODO: Add save dialog to form
                }
            }
            else
            {
                // TODO: Generate the spreadsheet that will be the source for our output

                // Create the output
                if (this.RadioButtonCsv.Checked)
                {
                    // TODO: Save the spreadsheet
                }
                else
                {
                    // TODO: Generate the HTML file
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the Select All Books CheckBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckBoxSelectAllBooks_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.checkBoxSelectAllBooksIsUpdating)
            {
                this.CheckedListBoxBooks.BeginUpdate();
                this.checkedListBoxBooksIsUpdating = true;
                for (int i = 0; i < this.CheckedListBoxBooks.Items.Count; i++)
                {
                    this.CheckedListBoxBooks.SetItemChecked(i, this.CheckBoxSelectAllBooks.Text == @"Select A&ll");
                }

                this.CheckedListBoxBooks.EndUpdate();
                this.checkedListBoxBooksIsUpdating = false;
                this.UpdateCheckBoxSelectAllBooks();
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the Comparison Texts CheckBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckBoxSelectAllComparisonTexts_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.checkBoxSelectAllComparisonTextsIsUpdating)
            {
                this.CheckedListBoxComparisonTexts.BeginUpdate();
                this.checkedListBoxComparisonTextsIsUpdating = true;
                for (int i = 0; i < this.CheckedListBoxComparisonTexts.Items.Count; i++)
                {
                    this.CheckedListBoxComparisonTexts.SetItemChecked(i, this.CheckBoxSelectAllComparisonTexts.Text == @"Select &All");
                }

                this.CheckedListBoxComparisonTexts.EndUpdate();
                this.checkedListBoxComparisonTextsIsUpdating = false;
                this.UpdateCheckBoxSelectAllComparisonTexts();
            }
        }

        /// <summary>
        /// Handles the ItemCheck event of the Books CheckedListBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemCheckEventArgs"/> instance containing the event data.</param>
        private void CheckedListBoxBooks_ItemCheck(object sender, ItemCheckEventArgs e) => this.UpdateCheckBoxSelectAllBooks(e.NewValue == CheckState.Checked);

        /// <summary>
        /// Handles the ItemCheck event of the Comparison Texts CheckedListBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemCheckEventArgs"/> instance containing the event data.</param>
        private void CheckedListBoxComparisonTexts_ItemCheck(object sender, ItemCheckEventArgs e) => this.UpdateCheckBoxSelectAllComparisonTexts(e.NewValue == CheckState.Checked);

        /// <summary>
        /// Handles the DrawItem event of the Base Text ComboBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DrawItemEventArgs"/> instance containing the event data.</param>
        private void ComboBoxBaseText_DrawItem(object sender, DrawItemEventArgs e)
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

                if (item.Bold)
                {
                    using Font font = new Font(comboBox.Font, FontStyle.Bold);
                    e.Graphics.DrawString(
                        item.Text,
                        font,
                        SystemBrushes.ControlText,
                        e.Bounds,
                        StringFormat.GenericTypographic);
                }
                else
                {
                    using Font font = new Font(comboBox.Font, FontStyle.Regular);
                    e.Graphics.DrawString(
                        item.Text,
                        font,
                        SystemBrushes.ControlText,
                        e.Bounds,
                        StringFormat.GenericTypographic);
                }

                if (item.Selectable && comboBox.DroppedDown)
                {
                    e.DrawFocusRectangle();
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the Base Text ComboBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ComboBoxBaseText_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the comparison texts to match the base translation's language
            this.UpdateComparisonTexts();

            // Update the books to match the base translation's canon
            await this.UpdateBooksAsync();
        }

        /// <summary>
        /// Handles the Load event of the Form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FormApparatusGenerator_Load(object sender, EventArgs e)
        {
            // Fill in the base translation
            this.ComboBoxBaseText.BeginUpdate();

            // Get the blocked languages and translations so we do not show them
            List<string> blockedLanguages =
                Settings.Default.BlockedLanguages?.Cast<string>().ToList() ?? new List<string>();
            List<string> blockedTranslations =
                Settings.Default.BlockedTranslations?.Cast<string>().ToList() ?? new List<string>();

            // Add the translations
            string lastLanguage = string.Empty;
            int selectedIndex = 0;
            int translationIndex = 0;
            foreach (Translation translation in this.translations)
            {
                // If this translation is not blocked
                string translationLanguage = translation.Language ?? Resources.UnknownLanguage;
                if (!blockedTranslations.Contains(translation.UniqueKey()) &&
                    !blockedLanguages.Contains(translationLanguage))
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
                        this.ComboBoxBaseText.Items.Add(headingComboBoxItem);
                        lastLanguage = translationLanguage;
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
                    this.ComboBoxBaseText.Items.Add(comboBoxItem);
                    if (translation.Code == this.selectedBaseTranslation)
                    {
                        selectedIndex = translationIndex;
                    }

                    translationIndex++;
                }
            }

            // Completed updating the combo boxes
            this.ComboBoxBaseText.EndUpdate();

            // Select the correct items in each translation drop down
            this.ComboBoxBaseText.SelectedIndex = selectedIndex;
        }

        /// <summary>
        /// Handles the Click event of the Include Apparatus Data TextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextBoxIncludeApparatusData_Click(object sender, EventArgs e) => this.ButtonBrowse_Click(sender, e);

        /// <summary>
        /// Updates the books to match the base translation's canon asynchronously.
        /// </summary>
        /// <returns>The task.</returns>
        private async Task UpdateBooksAsync()
        {
            // Update the books to match the base translation's canon
            this.checkedListBoxBooksIsUpdating = true;
            this.CheckedListBoxBooks.BeginUpdate();
            this.CheckedListBoxBooks.Items.Clear();

            // If we have a translation in the primary combo box
            if (this.ComboBoxBaseText.SelectedItem is TranslationComboBoxItem comboBoxItem)
            {
                // Add the book names to the suggestions list
                IProvider? provider = this.providers.FirstOrDefault(p => p.Id == comboBoxItem.Provider) ??
                                      this.providers.FirstOrDefault();
                if (provider != null)
                {
                    await foreach (Book book in provider.GetBooksAsync(comboBoxItem.Code, false))
                    {
                        this.CheckedListBoxBooks.Items.Add(book, this.CheckBoxSelectAllBooks.CheckState == CheckState.Checked);
                    }
                }
            }

            this.CheckedListBoxBooks.EndUpdate();
            this.checkedListBoxBooksIsUpdating = false;
            this.UpdateCheckBoxSelectAllBooks();
        }

        /// <summary>
        /// Updates the comparison texts to match the base translation's language.
        /// </summary>
        private void UpdateComparisonTexts()
        {
            // Update the comparison texts to match the base translation's language
            this.checkBoxSelectAllComparisonTextsIsUpdating = true;
            this.CheckedListBoxComparisonTexts.BeginUpdate();
            this.CheckedListBoxComparisonTexts.Items.Clear();

            // If we have a translation in the primary combo box
            if (this.ComboBoxBaseText.SelectedItem is TranslationComboBoxItem comboBoxItem)
            {
                // Add the other texts with the same language, or from Logos
                foreach (Translation translation in this.translations.Where(t => t.Code != comboBoxItem.Code && (t.Language == comboBoxItem.Language || t.Provider == nameof(LogosProvider))))
                {
                    // Use the translation combo box item for display
                    TranslationComboBoxItem translationItem = new TranslationComboBoxItem
                    {
                        Bold = false,
                        Selectable = true,
                        Text = translation.UniqueName(this.translations),
                        CanBeExported = translation.CanBeExported,
                        Code = translation.Code,
                        Language = translation.Language,
                        Provider = translation.Provider,
                    };

                    // Add the item
                    this.CheckedListBoxComparisonTexts.Items.Add(translationItem, this.CheckBoxSelectAllComparisonTexts.CheckState == CheckState.Checked);
                }
            }

            this.CheckedListBoxComparisonTexts.EndUpdate();
            this.checkedListBoxComparisonTextsIsUpdating = false;
            this.UpdateCheckBoxSelectAllComparisonTexts();
        }

        /// <summary>
        /// Updates the Select All Books CheckBox.
        /// </summary>
        /// <param name="checkOn">Whether or not to check the check box.</param>
        private void UpdateCheckBoxSelectAllBooks(bool? checkOn = null)
        {
            if (!this.checkedListBoxBooksIsUpdating)
            {
                this.checkBoxSelectAllBooksIsUpdating = true;
                int checkedCount = this.CheckedListBoxBooks.CheckedItems.Count;
                if (checkOn == true)
                {
                    checkedCount++;
                }
                else if (checkOn == false)
                {
                    checkedCount--;
                }

                if (checkedCount == this.CheckedListBoxBooks.Items.Count)
                {
                    this.CheckBoxSelectAllBooks.Text = @"Deselect A&ll";
                    this.ToolTipItem.SetToolTip(this.CheckBoxSelectAllBooks, $"{checkedCount} items selected");
                    this.CheckBoxSelectAllBooks.CheckState = CheckState.Checked;
                }
                else
                {
                    this.CheckBoxSelectAllBooks.Text = @"Select A&ll";
                    this.ToolTipItem.SetToolTip(
                        this.CheckBoxSelectAllBooks,
                        $"{this.CheckedListBoxBooks.Items.Count} items");
                    this.CheckBoxSelectAllBooks.CheckState =
                        checkedCount == 0 ? CheckState.Unchecked : CheckState.Indeterminate;
                }

                this.checkBoxSelectAllBooksIsUpdating = false;
            }
        }

        /// <summary>
        /// Updates the Select All Comparison Texts CheckBox.
        /// </summary>
        /// <param name="checkOn">Whether or not to check the check box.</param>
        private void UpdateCheckBoxSelectAllComparisonTexts(bool? checkOn = null)
        {
            if (!this.checkedListBoxComparisonTextsIsUpdating)
            {
                this.checkBoxSelectAllComparisonTextsIsUpdating = true;
                int checkedCount = this.CheckedListBoxComparisonTexts.CheckedItems.Count;
                if (checkOn == true)
                {
                    checkedCount++;
                }
                else if (checkOn == false)
                {
                    checkedCount--;
                }

                if (checkedCount == this.CheckedListBoxComparisonTexts.Items.Count)
                {
                    this.CheckBoxSelectAllComparisonTexts.Text = @"Deselect &All";
                    this.ToolTipItem.SetToolTip(this.CheckBoxSelectAllComparisonTexts, $"{checkedCount} items selected");
                    this.CheckBoxSelectAllComparisonTexts.CheckState = CheckState.Checked;
                }
                else
                {
                    this.CheckBoxSelectAllComparisonTexts.Text = @"Select &All";
                    this.ToolTipItem.SetToolTip(this.CheckBoxSelectAllComparisonTexts, $"{this.CheckedListBoxComparisonTexts.Items.Count} items");
                    this.CheckBoxSelectAllComparisonTexts.CheckState = checkedCount == 0 ? CheckState.Unchecked : CheckState.Indeterminate;
                }

                this.checkBoxSelectAllComparisonTextsIsUpdating = false;
            }
        }
    }
}
