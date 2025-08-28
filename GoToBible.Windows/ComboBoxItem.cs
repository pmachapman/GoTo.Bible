// -----------------------------------------------------------------------
// <copyright file="ComboBoxItem.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

/// <summary>
/// A combo box item.
/// </summary>
public class ComboBoxItem
{
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ComboBoxItem"/> is bold.
    /// </summary>
    /// <value>
    ///   <c>true</c> if bold; otherwise, <c>false</c>.
    /// </value>
    public bool Bold { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ComboBoxItem"/> is selectable.
    /// </summary>
    /// <value>
    ///   <c>true</c> if selectable; otherwise, <c>false</c>.
    /// </value>
    public bool Selectable { get; set; } = true;

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <value>
    /// The text.
    /// </value>
    public string Text { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override string ToString() => this.Text;
}
