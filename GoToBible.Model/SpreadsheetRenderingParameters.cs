// -----------------------------------------------------------------------
// <copyright file="SpreadsheetRenderingParameters.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
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
        /// Gets or sets the omission marker.
        /// </summary>
        /// <value>The marker for an omission.</value>
        /// <remarks>
        /// This should be plain text.
        /// </remarks>
        public virtual string OmissionMarker { get; set; } = Omit;
    }
}
