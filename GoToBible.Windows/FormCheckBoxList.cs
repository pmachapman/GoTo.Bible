// -----------------------------------------------------------------------
// <copyright file="FormCheckBoxList.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// The checkbox list form.
    /// </summary>
    /// <seealso cref="Form" />
    public partial class FormCheckBoxList : Form
    {
        /// <summary>
        /// The items.
        /// </summary>
        private readonly Dictionary<string, string> items;

        /// <summary>
        /// A value indicating whether the check box list is updating.
        /// </summary>
        private bool checkBoxListIsUpdating = false;

        /// <summary>
        /// A value indicating whether the select all check box is updating.
        /// </summary>
        private bool selectAllCheckBoxIsUpdating = false;

        /// <summary>
        /// Initialises a new instance of the <see cref="FormCheckBoxList" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="uncheckedItems">The unchecked items.</param>
        /// <param name="title">The form title.</param>
        /// <param name="icon">The icon.</param>
        public FormCheckBoxList(Dictionary<string, string> items, List<string> uncheckedItems, string title, Icon icon)
        {
            this.InitializeComponent();
            this.Icon = icon;
            this.items = items;
            this.Text = title;
            this.UncheckedItems = uncheckedItems.AsReadOnly();
        }

        /// <summary>
        /// Gets the unchecked items.
        /// </summary>
        /// <value>
        /// The unchecked items.
        /// </value>
        public ReadOnlyCollection<string> UncheckedItems { get; private set; }

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
            List<string> uncheckedItems = new List<string>();
            foreach (object item in this.CheckedListBoxItems.Items)
            {
                if (!this.CheckedListBoxItems.CheckedItems.Contains(item) && item is KeyValuePair<string, string> kvp)
                {
                    uncheckedItems.Add(kvp.Key);
                }
            }

            this.UncheckedItems = uncheckedItems.AsReadOnly();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the Select All CheckBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckBoxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.selectAllCheckBoxIsUpdating)
            {
                this.CheckedListBoxItems.BeginUpdate();
                this.checkBoxListIsUpdating = true;
                for (int i = 0; i < this.CheckedListBoxItems.Items.Count; i++)
                {
                    this.CheckedListBoxItems.SetItemChecked(i, this.CheckBoxSelectAll.Text == "&Select All");
                }

                this.CheckedListBoxItems.EndUpdate();
                this.checkBoxListIsUpdating = false;
                this.UpdateCheckBoxSelectAll();
            }
        }

        /// <summary>
        /// Handles the ItemCheck event of the Translations CheckedListBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemCheckEventArgs"/> instance containing the event data.</param>
        private void CheckedListBoxTranslations_ItemCheck(object sender, ItemCheckEventArgs e) => this.UpdateCheckBoxSelectAll(e.NewValue == CheckState.Checked);

        /// <summary>
        /// Handles the Load event of the Translations Form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FormTranslations_Load(object sender, EventArgs e)
        {
            this.CheckedListBoxItems.FormattingEnabled = true;
            this.CheckedListBoxItems.Format += (_, e) => e.Value = ((KeyValuePair<string, string>)e.ListItem).Value;
            this.CheckedListBoxItems.BeginUpdate();
            this.checkBoxListIsUpdating = true;
            foreach (KeyValuePair<string, string> item in this.items)
            {
                this.CheckedListBoxItems.Items.Add(item, !this.UncheckedItems.Contains(item.Key));
            }

            this.CheckedListBoxItems.EndUpdate();
            this.checkBoxListIsUpdating = false;
            this.UpdateCheckBoxSelectAll();
        }

        /// <summary>
        /// Updates the Select All CheckBox.
        /// </summary>
        private void UpdateCheckBoxSelectAll(bool? checkOn = null)
        {
            if (!this.checkBoxListIsUpdating)
            {
                this.selectAllCheckBoxIsUpdating = true;
                int checkedCount = this.CheckedListBoxItems.CheckedItems.Count;
                if (checkOn == true)
                {
                    checkedCount++;
                }
                else if (checkOn == false)
                {
                    checkedCount--;
                }

                if (checkedCount == this.CheckedListBoxItems.Items.Count)
                {
                    this.CheckBoxSelectAll.Text = "De&select All";
                    this.CheckBoxSelectAll.CheckState = CheckState.Checked;
                }
                else
                {
                    this.CheckBoxSelectAll.Text = "&Select All";
                    if (checkedCount == 0)
                    {
                        this.CheckBoxSelectAll.CheckState = CheckState.Unchecked;
                    }
                    else
                    {
                        this.CheckBoxSelectAll.CheckState = CheckState.Indeterminate;
                    }
                }

                this.selectAllCheckBoxIsUpdating = false;
            }
        }
    }
}
