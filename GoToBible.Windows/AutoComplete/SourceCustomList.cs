// -----------------------------------------------------------------------
// <copyright file="SourceCustomList.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows.AutoComplete;

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

/// <summary>
/// Implements the https://docs.microsoft.com/en-us/windows/win32/api/objidl/nn-objidl-ienumstring
/// interface for autocomplete.
/// </summary>
public class SourceCustomList : IEnumString
{
    /// <summary>
    /// The current position.
    /// </summary>
    private int currentPosition;

    /// <summary>
    /// Gets the string list.
    /// </summary>
    /// <value>
    /// The string list.
    /// </value>
    public string[] StringList { get; init; } = Array.Empty<string>();

    /// <inheritdoc/>
    public int Next(int celt, string[] rgelt, IntPtr pceltFetched)
    {
        int fetched = 0;
        while (this.currentPosition <= this.StringList.Length - 1 && fetched < celt)
        {
            rgelt[fetched] = this.StringList[this.currentPosition];
            fetched++;
            this.currentPosition++;
        }

        if (pceltFetched != IntPtr.Zero)
        {
            Marshal.WriteInt32(pceltFetched, fetched);
        }

        if (fetched == celt)
        {
            // S_OK
            return 0;
        }
        else
        {
            // S_FALSE
            return 1;
        }
    }

    /// <inheritdoc/>
    public int Skip(int celt)
    {
        this.currentPosition += celt;
        if (this.currentPosition <= this.StringList.Length - 1)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    /// <inheritdoc/>
    public void Reset() => this.currentPosition = 0;

    /// <inheritdoc/>
    public void Clone(out IEnumString ppenum)
    {
        SourceCustomList clone = new SourceCustomList
        {
            currentPosition = this.currentPosition,
            StringList = (string[])this.StringList.Clone(),
        };
        ppenum = clone;
    }
}
