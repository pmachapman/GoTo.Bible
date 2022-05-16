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
        /// <remarks>This should be HTML code or an HTML entity.</remarks>
        public string OmissionMarker = "<em>omit</em>";
    }
}
