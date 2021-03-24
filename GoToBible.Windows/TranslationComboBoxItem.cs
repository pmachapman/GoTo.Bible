// -----------------------------------------------------------------------
// <copyright file="TranslationComboBoxItem.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows
{
    /// <summary>
    /// A translation combo box item.
    /// </summary>
    /// <seealso cref="GoToBible.Windows.ComboBoxItem" />
    public class TranslationComboBoxItem : ComboBoxItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether the translation can be exported.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the translation can be exported; otherwise, <c>false</c>.
        /// </value>
        public bool CanBeExported { get; set; } = false;

        /// <summary>
        /// Gets or sets the translation code.
        /// </summary>
        /// <value>
        /// The translation code.
        /// </value>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the translation provider.
        /// </summary>
        /// <value>
        /// The translation provider.
        /// </value>
        public string Provider { get; set; } = string.Empty;
    }
}
