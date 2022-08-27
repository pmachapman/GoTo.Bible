// -----------------------------------------------------------------------
// <copyright file="RenderFormat.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

/// <summary>
/// The format to render in.
/// </summary>
public enum RenderFormat
{
    /// <summary>
    /// Render as text.
    /// </summary>
    Text = 0,

    /// <summary>
    /// Render as HTML.
    /// </summary>
    Html = 1,

    /// <summary>
    /// Render for import into Accordance.
    /// </summary>
    Accordance = 2,

    /// <summary>
    /// Render as an apparatus.
    /// </summary>
    /// <remarks>This should be used with <see cref="ApparatusRenderingParameters"/>.</remarks>
    Apparatus = 3,

    /// <summary>
    /// Renders as an apparatus in CSV based spreadsheet format.
    /// The columns are: Book,Chapter,Verse,Occurrence,Phrase,Variant.
    /// </summary>
    /// <remarks>This should be used with <see cref="SpreadsheetRenderingParameters"/>.</remarks>
    Spreadsheet = 4,
}
