// -----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable InconsistentNaming
namespace GoToBible.Windows;

using System.Runtime.InteropServices;

/// <summary>
/// Native Methods.
/// </summary>
internal static partial class NativeMethods
{
    /// <summary>
    /// A window receives this message when the user chooses a command from the Window menu
    /// (formerly known as the system or control menu) or when the user chooses the
    /// maximize button, minimize button, restore button, or close button.
    /// </summary>
    public const int WM_SYSCOMMAND = 0x112;

    /// <summary>
    /// Specifies that the menu item is a text string.
    /// </summary>
    public const int MF_STRING = 0x0;

    /// <summary>
    /// Draws a horizontal dividing line.
    /// </summary>
    public const int MF_SEPARATOR = 0x800;

    /// <summary>
    /// Appends a new item to the end of the specified menu bar, drop-down menu, submenu, or shortcut menu.
    /// You can use this function to specify the content, appearance, and behavior of the menu item.
    /// </summary>
    /// <param name="hMenu">A handle to the menu bar, drop-down menu, submenu, or shortcut menu to be changed.</param>
    /// <param name="uFlags">Controls the appearance and behavior of the new menu item.</param>
    /// <param name="uIdNewItem">
    /// The identifier of the new menu item or, if the uFlags parameter is set to <c>MF_POPUP</c>,
    /// a handle to the drop-down menu or submenu.
    /// </param>
    /// <param name="lpNewItem">The content of the new menu item. </param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.
    /// To get extended error information, call <c>GetLastError</c>.
    /// </returns>
    [LibraryImport(
        "user32.dll",
        EntryPoint = "AppendMenuW",
        StringMarshalling = StringMarshalling.Utf16,
        SetLastError = true
    )]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AppendMenu(nint hMenu, int uFlags, int uIdNewItem, string lpNewItem);

    /// <summary>
    /// Enables the application to access the window menu (also known as the system menu or the control menu) for copying and modifying.
    /// </summary>
    /// <param name="hWnd">A handle to the window that will own a copy of the window menu.</param>
    /// <param name="bRevert">
    /// The action to be taken. If this parameter is <c>false></c>, GetSystemMenu returns a handle to the copy of the window menu currently in use.
    /// The copy is initially identical to the window menu, but it can be modified. If this parameter is <c>true</c>,
    /// GetSystemMenu resets the window menu back to the default state. The previous window menu, if any, is destroyed.
    /// </param>
    /// <returns>
    /// If the bRevert parameter is <c>false</c>, the return value is a handle to a copy of the window menu.
    /// If the bRevert parameter is <c>true</c>, the return value is 0.
    /// </returns>
    [LibraryImport(
        "user32.dll",
        EntryPoint = "GetSystemMenu",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16
    )]
    public static partial nint GetSystemMenu(
        nint hWnd,
        [MarshalAs(UnmanagedType.Bool)] bool bRevert
    );

    /// <summary>
    /// Displays a ShellAbout dialog box.
    /// </summary>
    /// <param name="hWnd">A window handle to a parent window. This parameter can be 0.</param>
    /// <param name="szApp">
    /// A pointer to a null-terminated string that contains text to be displayed in the title bar of the ShellAbout
    /// dialog box and on the first line of the dialog box after the text "Microsoft".
    /// If the text contains a separator (#) that divides it into two parts, the function displays the first part
    /// in the title bar and the second part on the first line after the text "Microsoft".
    /// </param>
    /// <param name="szOtherStuff">
    /// A pointer to a null-terminated string that contains text to be displayed in the dialog box after the version and copyright information.
    /// </param>
    /// <param name="hIcon">The handle of an icon that the function displays in the dialog box.</param>
    /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
    [LibraryImport(
        "shell32.dll",
        EntryPoint = "ShellAboutW",
        SetLastError = false,
        StringMarshalling = StringMarshalling.Utf16
    )]
    public static partial int ShellAbout(nint hWnd, string szApp, string szOtherStuff, nint hIcon);
}
