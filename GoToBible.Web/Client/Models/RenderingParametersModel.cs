// -----------------------------------------------------------------------
// <copyright file="RenderingParametersModel.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
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
        /// Gets or sets a value indicating whether the interlinear ignores case.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the interlinear ignores case; otherwise, <c>false</c>.
        /// </value>
        public bool InterlinearIgnoresCase { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the interlinear ignores diacritics.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the interlinear ignores diacritics; otherwise, <c>false</c>.
        /// </value>
        public bool InterlinearIgnoresDiacritics { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the interlinear ignores punctuation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the interlinear ignores punctuation; otherwise, <c>false</c>.
        /// </value>
        public bool InterlinearIgnoresPunctuation { get; set; } = false;

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
