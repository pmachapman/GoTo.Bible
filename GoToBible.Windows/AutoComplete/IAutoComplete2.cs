// -----------------------------------------------------------------------
// <copyright file="IAutoComplete2.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

// The following ReSharper disable comments are so that
// the class in this file can match the COM type:
//
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace GoToBible.Windows.AutoComplete;

using System.Runtime.InteropServices;

/// <summary>
/// From https://www.pinvoke.net/default.aspx/Interfaces.IAutoComplete2.
/// </summary>
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("EAC04BC0-3791-11D2-BB95-0060977B464C")]
public interface IAutoComplete2
{
    /// <summary>
    /// Initializes the specified HWND edit.
    /// </summary>
    /// <param name="hwndEdit">
    /// Handle to the window for the system edit control that is to
    /// have autocompletion enabled.
    /// </param>
    /// <param name="punkACL">
    /// Pointer to the IUnknown interface of the string list object that
    /// is responsible for generating candidates for the completed
    /// string. The object must expose an IEnumString interface.
    /// </param>
    /// <param name="pwszRegKeyPath">
    /// Pointer to an optional null-terminated Unicode string that gives
    /// the registry path, including the value name, where the format
    /// string is stored as a REG_SZ value. The autocomplete object
    /// first looks for the path under HKEY_CURRENT_USER . If it fails,
    /// it then tries HKEY_LOCAL_MACHINE . For a discussion of the
    /// format string, see the definition of pwszQuickComplete.
    /// </param>
    /// <param name="pwszQuickComplete">
    /// Pointer to an optional string that specifies the format to be
    /// used if the user enters some text and presses CTRL+ENTER. Set
    /// this parameter to NULL to disable quick completion. Otherwise,
    /// the autocomplete object treats pwszQuickComplete as a sprintf
    /// format string, and the text in the edit box as its associated
    /// argument, to produce a new string. For example, set
    /// pwszQuickComplete to "http://www. %s.com/". When a user enters
    /// "MyURL" into the edit box and presses CTRL+ENTER, the text in
    /// the edit box is updated to "http://www.MyURL.com/".</param>
    /// <returns>
    /// If this method succeeds, it returns 0. Otherwise, it returns an HRESULT error code.
    /// </returns>
    [PreserveSig]
    int Init(
        nint hwndEdit,
        [MarshalAs(UnmanagedType.IUnknown)] object punkACL,
        [MarshalAs(UnmanagedType.LPWStr)] string pwszRegKeyPath,
        [MarshalAs(UnmanagedType.LPWStr)] string pwszQuickComplete);

    /// <summary>
    /// Enables or disables autocompletion.
    /// </summary>
    /// <param name="value">If set to <c>true</c>, enable autocompletion.</param>
    /// <returns>
    /// If this method succeeds, it returns 0. Otherwise, it returns an HRESULT error code.
    /// </returns>
    [PreserveSig]
    int Enable(bool value);

    /// <summary>
    /// Sets the current autocomplete options.
    /// </summary>
    /// <param name="dwFlag">
    /// One or more flags from the <see cref="AUTOCOMPLETEOPTIONS" /> enumeration
    /// that specify autocomplete options.
    /// </param>
    /// <returns>
    /// If this method succeeds, it returns 0. Otherwise, it returns an HRESULT error code.
    /// </returns>
    [PreserveSig]
    int SetOptions(AUTOCOMPLETEOPTIONS dwFlag);

    /// <summary>
    /// Retrieves the current autocomplete options.
    /// </summary>
    /// <param name="pdwFlag">
    /// One or more flags from the <see cref="AUTOCOMPLETEOPTIONS" /> enumeration
    /// that indicate the options that are currently set.
    /// </param>
    /// <returns>
    /// If this method succeeds, it returns 0. Otherwise, it returns an HRESULT error code.
    /// </returns>
    [PreserveSig]
    int GetOptions(out AUTOCOMPLETEOPTIONS pdwFlag);
}
