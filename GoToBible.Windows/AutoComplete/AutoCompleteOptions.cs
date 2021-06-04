// -----------------------------------------------------------------------
// <copyright file="AutoCompleteOptions.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

// The following ReSharper disable comments are so that
// the enum in this file can match the COM type:
//
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace GoToBible.Windows.AutoComplete
{
    using System;

    /// <summary>
    ///   Specifies values used by IAutoComplete2::GetOptions and
    ///   "IAutoComplete2.SetOptions" for options surrounding autocomplete.
    /// </summary>
    /// <remarks>
    ///   [AUTOCOMPLETEOPTIONS Enumerated Type ()]
    ///   http://msdn.microsoft.com/en-us/library/bb762479.aspx.
    /// </remarks>
    [Flags]
    public enum AUTOCOMPLETEOPTIONS
    {
        /// <summary>Do not autocomplete.</summary>
        ACO_NONE = 0x0000,

        /// <summary>Enable the autosuggest drop-down list.</summary>
        ACO_AUTOSUGGEST = 0x0001,

        /// <summary>Enable autoappend.</summary>
        ACO_AUTOAPPEND = 0x0002,

        /// <summary>Add a search item to the list of
        /// completed strings. When the user selects
        /// this item, it launches a search engine.</summary>
        ACO_SEARCH = 0x0004,

        /// <summary>Do not match common prefixes, such as
        /// "www." or "http://".</summary>
        ACO_FILTERPREFIXES = 0x0008,

        /// <summary>Use the TAB key to select an
        /// item from the drop-down list.</summary>
        ACO_USETAB = 0x0010,

        /// <summary>Use the UP ARROW and DOWN ARROW keys to
        /// display the autosuggest drop-down list.</summary>
        ACO_UPDOWNKEYDROPSLIST = 0x0020,

        /// <summary>Normal windows display text left-to-right
        /// (LTR). Windows can be mirrored to display languages
        /// such as Hebrew or Arabic that read right-to-left (RTL).
        /// Typically, control text is displayed in the same
        /// direction as the text in its parent window. If
        /// ACO_RTLREADING is set, the text reads in the opposite
        /// direction from the text in the parent window.</summary>
        ACO_RTLREADING = 0x0040,

        /// <summary>[Windows Vista and later]. If set, the
        /// autocompleted suggestion is treated as a phrase
        /// for search purposes. The suggestion, Microsoft
        /// Office, would be treated as "Microsoft Office"
        /// (where both Microsoft AND Office must appear in
        /// the search results).</summary>
        ACO_WORD_FILTER = 0x0080,

        /// <summary>[Windows Vista and later]. Disable prefix
        /// filtering when displaying the autosuggest dropdown.
        /// Always display all suggestions.</summary>
        ACO_NOPREFIXFILTERING = 0x0100,
    }
}
