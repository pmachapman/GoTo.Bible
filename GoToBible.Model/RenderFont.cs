// -----------------------------------------------------------------------
// <copyright file="RenderFont.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    /// <summary>
    /// The font to render with.
    /// </summary>
    /// <remarks>
    /// This is used instead of <c>System.Drawing.Font</c>, as that class is not supported by Blazor. It only includes font properties used by the <c>GoToBible.Renderer</c>.
    /// </remarks>
    public record RenderFont
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RenderFont"/> is bold.
        /// </summary>
        /// <value>
        ///   <c>true</c> if bold; otherwise, <c>false</c>.
        /// </value>
        public bool Bold { get; set; }

        /// <summary>
        /// Gets or sets the name of the font family.
        /// </summary>
        /// <value>
        /// The name of the font family.
        /// </value>
        public string FamilyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RenderFont"/> is italic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if italic; otherwise, <c>false</c>.
        /// </value>
        public bool Italic { get; set; }

        /// <summary>
        /// Gets or sets the font size in points.
        /// </summary>
        /// <value>
        /// The font size in points.
        /// </value>
        public float SizeInPoints { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RenderFont"/> is strikethrough.
        /// </summary>
        /// <value>
        ///   <c>true</c> if strikethrough; otherwise, <c>false</c>.
        /// </value>
        public bool Strikeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RenderFont"/> is underlined.
        /// </summary>
        /// <value>
        ///   <c>true</c> if underlined; otherwise, <c>false</c>.
        /// </value>
        public bool Underline { get;  set; }
    }
}
