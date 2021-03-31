// -----------------------------------------------------------------------
// <copyright file="PassageReference.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    /// <summary>
    /// A passage reference.
    /// </summary>
    public class PassageReference
    {
        /// <summary>
        /// Gets or sets the passage reference end.
        /// </summary>
        /// <value>
        /// The end of the passage reference.
        /// </value>
        public string End { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// We only require the start reference. This method assumes you used <c>AsPassageReference()</c> to generate this object.
        /// </remarks>
        public bool IsValid => !string.IsNullOrWhiteSpace(this.Start);

        /// <summary>
        /// Gets or sets the original passage reference.
        /// </summary>
        /// <value>
        /// The original passage reference.
        /// </value>
        public string Original { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the passage reference start.
        /// </summary>
        /// <value>
        /// The start of the passage reference.
        /// </value>
        public string Start { get; set; } = string.Empty;
    }
}
