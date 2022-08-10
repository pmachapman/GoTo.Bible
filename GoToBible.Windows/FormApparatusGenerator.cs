// -----------------------------------------------------------------------
// <copyright file="FormApparatusGenerator.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Text;
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
        /// The books in order.
        /// </summary>
        private readonly List<string> books = new List<string>();

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
        /// Initializes a new instance of the <see cref="FormApparatusGenerator" /> class.
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
        /// Gets or sets a value indicating whether or not this instance is generating.
        /// </summary>
        /// <value><c>true</c> if the instance is generating; otherwise, <c>false</c>.</value>
        private bool IsGenerating
        {
            get => this.ProgressBarMain.Visible;
            set
            {
                this.ProgressBarMain.Visible = value;
                this.ComboBoxBaseText.Enabled = !value;
                this.CheckedListBoxComparisonTexts.Enabled = !value;
                this.CheckBoxSelectAllComparisonTexts.Enabled = !value;
                this.CheckedListBoxBooks.Enabled = !value;
                this.CheckBoxSelectAllBooks.Enabled = !value;
                this.ButtonBrowse.Enabled = !value;
                this.RadioButtonCsv.Enabled = !value;
                this.RadioButtonHtml.Enabled = !value;
                this.ButtonOk.Enabled = !value;
            }
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
            this.IsGenerating = false;
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
            if (!this.selectedFileNames.Any())
            {
                if (this.CheckedListBoxComparisonTexts.CheckedItems.Count == 0)
                {
                    MessageBox.Show(
                        @"You must select at least one comparison translation, or additional apparatus data file.",
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (this.CheckedListBoxBooks.CheckedItems.Count == 0)
                {
                    MessageBox.Show(
                        @"You must select at least one book.",
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            // Get the primary translation
            if (this.ComboBoxBaseText.SelectedItem is not TranslationComboBoxItem primaryTranslation)
            {
                MessageBox.Show(@"You must select a base translation.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Show the progress bar
            this.IsGenerating = true;

            // Build the data table for the final output
            DataTable dataTable = new DataTable();

            // Book,Chapter,Verse,Occurrence,Phrase,Variant
            Type[] columnTypes = { typeof(string), typeof(int), typeof(string), typeof(int), typeof(string), typeof(string) };
            dataTable.Columns.Add("Book", columnTypes[0]);
            dataTable.Columns.Add("Chapter", columnTypes[1]);
            dataTable.Columns.Add("Verse", columnTypes[2]);
            dataTable.Columns.Add("Occurrence", columnTypes[3]);
            dataTable.Columns.Add("Phrase", columnTypes[4]);

            // Use variant if we are only comparing with one text
            if (this.CheckedListBoxComparisonTexts.CheckedItems.Count == 1
                && !this.selectedFileNames.Any())
            {
                dataTable.Columns.Add("Variant", columnTypes[5]);
            }

            // Set up the apparatus rendering parameters, as we will use this with the spreadsheet generation
            ApparatusRenderingParameters apparatusParameters = new ApparatusRenderingParameters
            {
                Format = RenderFormat.Spreadsheet,
                InterlinearIgnoresCase = true,
                InterlinearIgnoresDiacritics = true,
                InterlinearIgnoresPunctuation = true,
                PrimaryProvider = primaryTranslation.Provider,
                PrimaryTranslation = primaryTranslation.Code,
                RenderNeighbourForAddition = true,
            };

            // For every comparison translation
            foreach (TranslationComboBoxItem comboBoxItem in this.CheckedListBoxComparisonTexts.CheckedItems)
            {
                // For every selected book
                foreach (Book book in this.CheckedListBoxBooks.CheckedItems)
                {
                    // For every chapter in the book
                    foreach (ChapterReference chapter in book.Chapters)
                    {
                        // Setup the parameters for a spreadsheet
                        SpreadsheetRenderingParameters parameters = apparatusParameters with
                        {
                            PassageReference = chapter.AsPassageReference(),
                            SecondaryProvider = comboBoxItem.Provider,
                            SecondaryTranslation = comboBoxItem.Code,
                        };

                        // Render the output
                        RenderedPassage passage = await this.renderer.RenderAsync(parameters, true);
                        if (!string.IsNullOrWhiteSpace(passage.Content) && this.IsGenerating)
                        {
                            DataTable table = passage.Content.AsDataTable(columnTypes);
                            if (dataTable.Columns[^1].ColumnName == "Variant")
                            {
                                // We are only dealing with one text - just merge
                                dataTable.Merge(table);
                            }
                            else
                            {
                                // Set up the columns then merge
                                table.Columns[^1].ColumnName = comboBoxItem.Code;
                                dataTable.Columns.Add(comboBoxItem.Code, typeof(string));
                                dataTable.Merge(table);
                            }
                        }
                        else
                        {
                            this.IsGenerating = false;
                            if (string.IsNullOrWhiteSpace(passage.Content))
                            {
                                MessageBox.Show(
                                    $@"There was an unknown error rendering the apparatus for {parameters}.",
                                    this.Text,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }

                            return;
                        }
                    }
                }
            }

            // Add the multiple apparatus files
            foreach (string path in this.selectedFileNames)
            {
                // Get the CSV file contents
                string content;
                try
                {
                    content = await File.ReadAllTextAsync(path);
                }
                catch (IOException)
                {
                    this.IsGenerating = false;
                    MessageBox.Show(
                        $@"There was an error loading: {Path.GetFileName(path)}.",
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                DataTable table = content.AsDataTable(columnTypes);

                // If we have a variant column, rename it to the file name
                if (table.Columns.Count == 6 && table.Columns[5].ColumnName == "Variant")
                {
                    string columnName = Path.GetFileNameWithoutExtension(path);
                    table.Columns[5].ColumnName = columnName;
                }

                // Add the additional columns
                for (int i = 5; i < table.Columns.Count; i++)
                {
                    string columnName = table.Columns[i].ColumnName;
                    if (dataTable.Columns.Contains(columnName))
                    {
                        columnName += $"_{dataTable.Columns.Count}";
                    }

                    dataTable.Columns.Add(columnName, typeof(string));
                }

                dataTable.Merge(table);
            }

            // If we have more than one variant
            DataTable finalDataTable;
            if (dataTable.Columns.Count > 6)
            {
                // Add the row number for sorting purposes
                dataTable.Columns.Add("RowNumber", typeof(int));
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    dataTable.Rows[i].BeginEdit();
                    dataTable.Rows[i]["RowNumber"] = i + 1;
                    dataTable.Rows[i].EndEdit();
                }

                // Set up the final data table which will be de-duplicated
                DataTable unsortedDataTable = new DataTable();
                foreach (DataColumn column in dataTable.Columns)
                {
                    unsortedDataTable.Columns.Add(column.ColumnName, column.DataType);
                }

                // De-duplicate the rows by filling in empty columns
                DataRow? lastRow = null;
                foreach (DataRow row in dataTable.AsEnumerable()
                             .OrderBy(r => (string)r["Book"], new PositionComparer<string>(this.books))
                             .ThenBy(r => (int)r["Chapter"])
                             .ThenBy(r => (string)r["Verse"], new VerseComparer())
                             .ThenBy(r => (int)r["Occurrence"])
                             .ThenBy(r => (string)r["Phrase"], new WordComparer(apparatusParameters))
                             .ThenBy(r => (int)r["RowNumber"]))
                {
                    // If the last row is the same as this row
                    if (lastRow != null
                        && (string)row["Book"] == (string)lastRow["Book"]
                        && (int)row["Chapter"] == (int)lastRow["Chapter"]
                        && (string)row["Verse"] == (string)lastRow["Verse"]
                        && (int)row["Occurrence"] == (int)lastRow["Occurrence"]
                        && string.Compare((string)row["Phrase"], (string)lastRow["Phrase"], CultureInfo.InvariantCulture, apparatusParameters.AsCompareOptions()) == 0)
                    {
                        // Fill in the empty fields
                        for (int i = 5; i < row.ItemArray.Length - 1; i++)
                        {
                            if (string.IsNullOrEmpty(lastRow[i].ToString())
                                && !string.IsNullOrEmpty(row[i].ToString()))
                            {
                                lastRow.BeginEdit();
                                lastRow[i] = row[i];
                                lastRow.EndEdit();
                            }
                        }
                    }
                    else
                    {
                        // Add the data row to the new table
                        DataRow newRow = unsortedDataTable.NewRow();
                        newRow.ItemArray = row.ItemArray;
                        unsortedDataTable.Rows.Add(newRow);
                        lastRow = newRow;
                    }
                }

                // Set up the final data table which will be sorted
                finalDataTable = new DataTable();
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.ColumnName != "RowNumber")
                    {
                        finalDataTable.Columns.Add(column.ColumnName, column.DataType);
                    }
                }

                // Sort the output
                foreach (DataRow row in unsortedDataTable.AsEnumerable()
                             .OrderBy(r => (string)r["Book"], new PositionComparer<string>(this.books))
                             .ThenBy(r => (int)r["Chapter"])
                             .ThenBy(r => (string)r["Verse"], new VerseComparer())
                             .ThenBy(r => (int)r["RowNumber"])
                             .ThenBy(r => (int)r["Occurrence"])
                             .ThenBy(r => (string)r["Phrase"], new WordComparer(apparatusParameters)))
                {
                    // Add the data row to the new table
                    DataRow newRow = finalDataTable.NewRow();
                    newRow.ItemArray = row.ItemArray[..^1];
                    finalDataTable.Rows.Add(newRow);
                }
            }
            else
            {
                finalDataTable = dataTable;
            }

            // Save the output
            if (this.RadioButtonCsv.Checked)
            {
                // Save the spreadsheet
                this.SaveFileDialogMain.DefaultExt = "*.csv";
                this.SaveFileDialogMain.Filter = @"CSV Spreadsheet (*.csv)|*.csv|All Files (*.*)|*.*";
                if (this.SaveFileDialogMain.ShowDialog() == DialogResult.OK)
                {
                    // Save the file with the BOM
                    await File.WriteAllTextAsync(
                        this.SaveFileDialogMain.FileName,
                        finalDataTable.AsCsvData(),
                        Encoding.UTF8);
                }
                else
                {
                    this.IsGenerating = false;
                    return;
                }
            }
            else
            {
                // Save the HTML file
                this.SaveFileDialogMain.DefaultExt = "*.html";
                this.SaveFileDialogMain.Filter = @"HTML File (*.html)|*.html;*.htm|All Files (*.*)|*.*";
                if (this.SaveFileDialogMain.ShowDialog() == DialogResult.OK)
                {
                    // Save the file with the BOM
                    await File.WriteAllTextAsync(
                        this.SaveFileDialogMain.FileName,
                        finalDataTable.AsHtmlApparatus(apparatusParameters),
                        Encoding.UTF8);
                }
                else
                {
                    this.IsGenerating = false;
                    return;
                }
            }

            this.IsGenerating = false;
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
        /// Handles the FormClosing event of the Form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FormApparatusGenerator_FormClosing(object sender, FormClosingEventArgs e) => this.IsGenerating = false;

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
            this.books.Clear();
            this.checkedListBoxBooksIsUpdating = true;
            this.CheckedListBoxBooks.BeginUpdate();
            this.CheckedListBoxBooks.Items.Clear();

            // If we have a translation in the primary combo box
            if (this.ComboBoxBaseText.SelectedItem is TranslationComboBoxItem comboBoxItem)
            {
                // Add the book names to the suggestions list
                IProvider? provider = this.providers.FirstOrDefault(p => p.Id == comboBoxItem.Provider) ??
                                      this.providers.FirstOrDefault();
                if (provider is not null)
                {
                    await foreach (Book book in provider.GetBooksAsync(comboBoxItem.Code, true))
                    {
                        this.books.Add(book.Name);
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
