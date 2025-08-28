// -----------------------------------------------------------------------
// <copyright file="FormCheckBoxList.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

/// <summary>
/// The checkbox list form.
/// </summary>
/// <seealso cref="Form" />
[SupportedOSPlatform("windows")]
public partial class FormCheckBoxList : Form
{
    /// <summary>
    /// The items.
    /// </summary>
    private readonly Dictionary<string, string> items;

    /// <summary>
    /// A value indicating whether the check box list is updating.
    /// </summary>
    private bool checkBoxListIsUpdating;

    /// <summary>
    /// A value indicating whether the select all check box is updating.
    /// </summary>
    private bool selectAllCheckBoxIsUpdating;

    /// <summary>
    /// The tool tip index.
    /// </summary>
    private int toolTipIndex = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormCheckBoxList" /> class.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <param name="uncheckedItems">The unchecked items.</param>
    /// <param name="title">The form title.</param>
    /// <param name="icon">The icon.</param>
    public FormCheckBoxList(
        Dictionary<string, string> items,
        List<string> uncheckedItems,
        string title,
        Icon icon
    )
    {
        this.InitializeComponent();
        this.Icon = icon;
        this.items = items;
        this.Text = title;
        this.UncheckedItems = uncheckedItems.AsReadOnly();
    }

    /// <inheritdoc />
    [AllowNull]
    public sealed override string Text
    {
        get => base.Text;
        set => base.Text = value;
    }

    /// <summary>
    /// Gets the unchecked items.
    /// </summary>
    /// <value>
    /// The unchecked items.
    /// </value>
    public ReadOnlyCollection<string> UncheckedItems { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is compiled in debug mode.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance compiled debug mode; otherwise, <c>false</c>.
    /// </value>
    private static bool IsDebug =>
#if DEBUG
        true;
#else
        false;
#endif

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
        List<string> uncheckedItems = [];
        foreach (object item in this.CheckedListBoxItems.Items)
        {
            if (
                !this.CheckedListBoxItems.CheckedItems.Contains(item)
                && item is KeyValuePair<string, string> kvp
            )
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
                this.CheckedListBoxItems.SetItemChecked(
                    i,
                    this.CheckBoxSelectAll.Text == @"&Select All"
                );
            }

            this.CheckedListBoxItems.EndUpdate();
            this.checkBoxListIsUpdating = false;
            this.UpdateCheckBoxSelectAll();
        }
    }

    /// <summary>
    /// Handles the ItemCheck event of the Items CheckedListBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ItemCheckEventArgs"/> instance containing the event data.</param>
    private void CheckedListBoxItems_ItemCheck(object sender, ItemCheckEventArgs e) =>
        this.UpdateCheckBoxSelectAll(e.NewValue == CheckState.Checked);

    /// <summary>
    /// Handles the MouseMove event of the Items CheckedListBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    private void CheckedListBoxItems_MouseMove(object sender, MouseEventArgs e)
    {
        // We only want the tooltip in debug mode, as it is very specific to the development environment
        if (IsDebug && this.toolTipIndex != this.CheckedListBoxItems.IndexFromPoint(e.Location))
        {
            this.toolTipIndex = this.CheckedListBoxItems.IndexFromPoint(
                this.CheckedListBoxItems.PointToClient(MousePosition)
            );
            if (
                this.toolTipIndex > -1
                && this.toolTipIndex < this.CheckedListBoxItems.Items.Count
                && this.CheckedListBoxItems.Items[this.toolTipIndex]
                    is KeyValuePair<string, string> item
            )
            {
                this.ToolTipItem.SetToolTip(this.CheckedListBoxItems, item.Key);
            }
        }
    }

    /// <summary>
    /// Handles the MouseUp event of the Items CheckedListBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    private void CheckedListBoxItems_MouseUp(object sender, MouseEventArgs e)
    {
        // We only want to copy the ID in debug mode, as this feature is very specific to the development environment
        if (IsDebug && e.Button == MouseButtons.Right)
        {
            int itemIndex = this.CheckedListBoxItems.IndexFromPoint(
                this.CheckedListBoxItems.PointToClient(MousePosition)
            );
            if (
                itemIndex > -1
                && itemIndex < this.CheckedListBoxItems.Items.Count
                && this.CheckedListBoxItems.Items[itemIndex] is KeyValuePair<string, string> item
            )
            {
                Clipboard.SetText(item.Key);
            }
        }
    }

    /// <summary>
    /// Handles the Load event of the CheckBoxList Form.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void FormCheckBoxList_Load(object sender, EventArgs e)
    {
        this.CheckedListBoxItems.FormattingEnabled = true;
        this.CheckedListBoxItems.Format += (_, eventArgs) =>
            eventArgs.Value = (eventArgs.ListItem as KeyValuePair<string, string>?)?.Value;
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
    /// <param name="checkOn">Whether or not to check the check box.</param>
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
                this.CheckBoxSelectAll.Text = @"De&select All";
                this.ToolTipItem.SetToolTip(
                    this.CheckBoxSelectAll,
                    $"{checkedCount} items selected"
                );
                this.CheckBoxSelectAll.CheckState = CheckState.Checked;
            }
            else
            {
                this.CheckBoxSelectAll.Text = @"&Select All";
                this.ToolTipItem.SetToolTip(
                    this.CheckBoxSelectAll,
                    $"{this.CheckedListBoxItems.Items.Count} items"
                );
                this.CheckBoxSelectAll.CheckState =
                    checkedCount == 0 ? CheckState.Unchecked : CheckState.Indeterminate;
            }

            this.selectAllCheckBoxIsUpdating = false;
        }
    }
}
