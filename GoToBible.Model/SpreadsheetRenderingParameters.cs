// -----------------------------------------------------------------------
// <copyright file="SpreadsheetRenderingParameters.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

/// <summary>
/// The spreadsheet rendering parameters.
/// </summary>
public record SpreadsheetRenderingParameters : RenderingParameters
{
    /// <summary>
    /// The phrase Omit for the omission marker.
    /// </summary>
    public const string Omit = "Omit";

    /// <summary>
    /// Gets or sets a value indicating whether we should render the neighbouring
    /// word in the apparatus, if a phrase is an addition.
    /// </summary>
    /// <value>
    /// <c>true</c> if we are to render the neighbouring word; otherwise <c>false</c>.
    /// </value>
    public bool RenderNeighbourForAddition { get; set; }

    /// <summary>
    /// Gets or sets the omission marker.
    /// </summary>
    /// <value>The marker for an omission.</value>
    /// <remarks>
    /// This should be plain text.
    /// </remarks>
    public virtual string OmissionMarker { get; set; } = Omit;
}
