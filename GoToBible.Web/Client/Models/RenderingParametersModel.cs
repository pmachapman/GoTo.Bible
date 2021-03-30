// -----------------------------------------------------------------------
// <copyright file="RenderingParametersModel.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Client.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The model which is used to collect the rendering parameters.
    /// </summary>
    public class RenderingParametersModel
    {
        /// <summary>
        /// Gets or sets the passage reference.
        /// </summary>
        /// <value>
        /// The passage reference.
        /// </value>
        [Required]
        public string Passage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the primary translation.
        /// </summary>
        /// <value>
        /// The primary translation.
        /// </value>
        [Required]
        public string PrimaryTranslation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the secondary translation.
        /// </summary>
        /// <value>
        /// The secondary translation.
        /// </value>
        public string? SecondaryTranslation { get; set; }
    }
}
