// -----------------------------------------------------------------------
// <copyright file="ApparatusRenderingParameters.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    /// <summary>
    /// The apparatus rendering parameters.
    /// </summary>
    public record ApparatusRenderingParameters : RenderingParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether we should render the neighbouring
        /// word in the apparatus, if a phrase is an addition.
        /// </summary>
        /// <value>
        /// <c>true</c> if we are to render the neighbouring word; otherwise <c>false</c>.
        /// </value>
        public bool RenderNeighbourForAddition { get; set; }

        /// <summary>
        /// Gets or sets the occurrence marker.
        /// </summary>
        /// <value>The marker for an occurrence.</value>
        /// <remarks>
        /// This should be HTML code or an HTML entity.
        /// You should substitute the occurrence number with the marker %OCCURRENCE%
        /// Otherwise, the marker will be shown unmodified (not ideal!).
        /// The default value uses the occurrence class to show plain text in superscript.
        /// </remarks>
        public string OccurrenceMarker { get; set; } = "<span class=\"occurrence\">%OCCURRENCE%</span>";

        /// <summary>
        /// Gets or sets the omission marker.
        /// </summary>
        /// <value>The marker for an omission.</value>
        /// <remarks>
        /// This should be HTML code or an HTML entity.
        /// You can substitute the omitted phrase with the marker %OMITTED_PHRASE%
        /// Otherwise, the marker will be shown unmodified.
        /// </remarks>
        public string OmissionMarker { get; set; } = "<em>Omit</em>";
    }
}
