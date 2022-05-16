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
        /// Gets or sets the omission marker.
        /// </summary>
        /// <value>The marker for an omission.</value>
        /// <remarks>
        /// This should be HTML code or an HTML entity.
        /// You can substitute the omitted phrase with the marker %OMITTED_PHRASE%
        /// Otherwise, the marker will be shown unmodified.</remarks>
        public string OmissionMarker { get; set; } = "<em>Omit</em>";
    }
}
