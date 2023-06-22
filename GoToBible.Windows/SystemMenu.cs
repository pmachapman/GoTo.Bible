// -----------------------------------------------------------------------
// <copyright file="SystemMenu.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Windows.Forms;

/// <summary>
/// Allows extension of the System Menu with additional menu items.
/// </summary>
[SupportedOSPlatform("windows")]
public class SystemMenu
{
    /// <summary>
    /// The form.
    /// </summary>
    private readonly Form form;

    /// <summary>
    /// The actions.
    /// </summary>
    private readonly IList<Action> actions = new List<Action>();

    /// <summary>
    /// The pending menu items.
    /// </summary>
    private readonly List<MenuItem> pendingMenuItems = new List<MenuItem>();

    /// <summary>
    /// The handle on the system menu.
    /// </summary>
    private nint hSysMenu;

    /// <summary>
    /// The last menu item ID.
    /// </summary>
    private int lastId;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemMenu"/> class..
    /// </summary>
    /// <param name="form">The form to modify the system menu of.</param>
    public SystemMenu(Form form)
    {
        this.form = form;
        if (!form.IsHandleCreated)
        {
            form.HandleCreated += this.OnHandleCreated;
        }
        else
        {
            this.OnHandleCreated(null, null);
        }
    }

    /// <summary>
    /// Adds a menu item to the system menu.
    /// </summary>
    /// <param name="text">The displayed command text.</param>
    /// <param name="action">The action that is executed when the user clicks on the command.</param>
    /// <param name="separatorBeforeCommand">Indicates whether a separator is inserted before the command.</param>
    public void AddMenuItem(string text, Action action, bool separatorBeforeCommand)
    {
        int id = ++this.lastId;
        if (!this.form.IsHandleCreated)
        {
            // The form is not yet created, queue the command for later addition
            this.pendingMenuItems.Add(new MenuItem
            {
                Id = id,
                Text = text,
                Separator = separatorBeforeCommand,
            });
        }
        else
        {
            // The form is created, add the command now
            if (separatorBeforeCommand)
            {
                NativeMethods.AppendMenu(this.hSysMenu, NativeMethods.MF_SEPARATOR, 0, string.Empty);
            }

            NativeMethods.AppendMenu(this.hSysMenu, NativeMethods.MF_STRING, id, text);
        }

        this.actions.Add(action);
    }

    /// <summary>
    /// Checks a window message for system menu commands and executes the associated action. This
    /// method must be called from within the Form's overridden WndProc method because it is not
    /// publicly accessible.
    /// </summary>
    /// <param name="msg">The window message to test.</param>
    public void HandleMessage(ref Message msg)
    {
        // This method is kept short and simple to allow inlining.
        // It will be called for every single message that is sent to the window.
        if (msg.Msg == NativeMethods.WM_SYSCOMMAND)
        {
            this.OnSysCommandMessage(ref msg);
        }
    }

    /// <summary>
    /// Handles the OnHandleCreated event of the form.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void OnHandleCreated(object? sender, EventArgs? e)
    {
        this.form.HandleCreated -= this.OnHandleCreated;
        this.hSysMenu = NativeMethods.GetSystemMenu(this.form.Handle, false);

        // Add all queued menu items
        foreach (MenuItem menuItem in this.pendingMenuItems)
        {
            if (menuItem.Separator)
            {
                NativeMethods.AppendMenu(this.hSysMenu, NativeMethods.MF_SEPARATOR, 0, string.Empty);
            }

            NativeMethods.AppendMenu(this.hSysMenu, NativeMethods.MF_STRING, menuItem.Id, menuItem.Text);
        }

        this.pendingMenuItems.Clear();
    }

    /// <summary>
    /// Handles the system command message.
    /// </summary>
    /// <param name="msg">The message.</param>
    private void OnSysCommandMessage(ref Message msg)
    {
        if ((long)msg.WParam > 0 && (long)msg.WParam <= this.lastId)
        {
            this.actions[(int)msg.WParam - 1]();
        }
    }

    /// <summary>
    /// A menu item.
    /// </summary>
    private class MenuItem
    {
        /// <summary>
        /// Gets the menu item identifier.
        /// </summary>
        /// <value>The menu item identifier.</value>
        public int Id { get; init; }

        /// <summary>
        /// Gets the menu item text.
        /// </summary>
        /// <value>The menu item text.</value>
        public string Text { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether gets or sets the whether this is a menu item separator.
        /// </summary>
        /// <value><c>true</c> if this is a menu item separator; otherwise, <c>false</c>.</value>
        public bool Separator { get; init; }
    }
}
