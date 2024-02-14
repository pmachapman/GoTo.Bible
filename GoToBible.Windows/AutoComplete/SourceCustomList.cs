// -----------------------------------------------------------------------
// <copyright file="SourceCustomList.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
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
    public string[] StringList { get; init; } = [];

    /// <inheritdoc/>
    public int Next(int celt, string[] rgelt, nint pceltFetched)
    {
        int fetched = 0;
        while (this.currentPosition <= this.StringList.Length - 1 && fetched < celt)
        {
            rgelt[fetched] = this.StringList[this.currentPosition];
            fetched++;
            this.currentPosition++;
        }

        if (pceltFetched != nint.Zero)
        {
            Marshal.WriteInt32(pceltFetched, fetched);
        }

        // S_OK = 0, S_FALSE = 1
        return fetched == celt ? 0 : 1;
    }

    /// <inheritdoc/>
    public int Skip(int celt)
    {
        this.currentPosition += celt;
        return this.currentPosition <= this.StringList.Length - 1 ? 0 : 1;
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
