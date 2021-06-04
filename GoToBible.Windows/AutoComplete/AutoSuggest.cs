// -----------------------------------------------------------------------
// <copyright file="AutoSuggest.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows.AutoComplete
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Windows.Forms;

    /// <summary>
    /// AutoSuggest Implementation (see https://stackoverflow.com/questions/3185587/how-to-make-autocomplete-on-a-textbox-show-suggestions-when-empty).
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class AutoSuggest
    {
        /// <summary>
        /// Gets the automatic complete class ID.
        /// </summary>
        private static Guid AutoCompleteClsId => new Guid("{00BB2763-6A77-11D0-A535-00C04FD7D062}");

        /// <summary>
        /// Disables auto suggest for the specified text box.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        public static void Disable(TextBox textBox)
        {
            textBox.AutoCompleteCustomSource = null;
            Disable(textBox.Handle);
        }

        /// <summary>
        /// Enables auto suggest for the specified text box.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="suggestions">The suggestions.</param>
        public static void Enable(TextBox textBox, string[] suggestions)
        {
            // Try to enable a more advanced settings for AutoComplete via the WinShell interface
            try
            {
                SourceCustomList source = new SourceCustomList { StringList = suggestions.ToArray() };

                // For options descriptions see:
                // https://docs.microsoft.com/en-us/windows/win32/api/shldisp/ne-shldisp-autocompleteoptions
                AUTOCOMPLETEOPTIONS options = AUTOCOMPLETEOPTIONS.ACO_UPDOWNKEYDROPSLIST | AUTOCOMPLETEOPTIONS.ACO_USETAB |
                    AUTOCOMPLETEOPTIONS.ACO_AUTOAPPEND | AUTOCOMPLETEOPTIONS.ACO_AUTOSUGGEST;
                Enable(textBox.Handle, source, options);
            }
            catch (Exception)
            {
                // In case of an error, let's fall back to the default
                AutoCompleteStringCollection source = new AutoCompleteStringCollection();
                source.AddRange(suggestions);
                textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                textBox.AutoCompleteCustomSource = source;
            }
        }

        /// <summary>
        /// Gets the automatic complete object.
        /// </summary>
        /// <returns>
        /// The autocomplete object.
        /// </returns>
        private static object? GetAutoComplete()
        {
            Type? autocompleteType = Type.GetTypeFromCLSID(AutoCompleteClsId);
            if (autocompleteType != null)
            {
                return Activator.CreateInstance(autocompleteType);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Enables the specified control handle.
        /// </summary>
        /// <param name="controlHandle">The control handle.</param>
        /// <param name="items">The items.</param>
        /// <param name="options">The options.</param>
        private static void Enable(IntPtr controlHandle, SourceCustomList items, AUTOCOMPLETEOPTIONS options)
        {
            if (controlHandle == IntPtr.Zero)
            {
                return;
            }

            IAutoComplete2? iac = null;
            try
            {
                iac = GetAutoComplete() as IAutoComplete2;
                iac?.Init(controlHandle, items, string.Empty, string.Empty);
                iac?.SetOptions(options);
                iac?.Enable(true);
            }
            finally
            {
                if (iac != null)
                {
                    Marshal.ReleaseComObject(iac);
                }
            }
        }

        /// <summary>
        /// Disables the specified control handle.
        /// </summary>
        /// <param name="controlHandle">The control handle.</param>
        private static void Disable(IntPtr controlHandle)
        {
            if (controlHandle == IntPtr.Zero)
            {
                return;
            }

            IAutoComplete2? iac = null;
            try
            {
                iac = GetAutoComplete() as IAutoComplete2;
                iac?.Enable(false);
            }
            finally
            {
                if (iac != null)
                {
                    Marshal.ReleaseComObject(iac);
                }
            }
        }
    }
}
