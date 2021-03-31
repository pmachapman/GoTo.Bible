// -----------------------------------------------------------------------
// <copyright file="Default.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    using System.Drawing;

    /// <summary>
    /// The renderer defaults.
    /// </summary>
    public static class Default
    {
        /// <summary>
        /// The default to passage to dispaly if none is specified.
        /// </summary>
        public const string Passage = "John 3:16";

        /// <summary>
        /// Gets the default background colour.
        /// </summary>
        /// <value>
        /// The default background colour.
        /// </value>
        public static Color BackgroundColour => Color.White;

        /// <summary>
        /// Gets the default font.
        /// </summary>
        /// <value>
        /// The default font.
        /// </value>
        public static RenderFont Font => new RenderFont
        {
            FamilyName = "Calibri",
            SizeInPoints = 14.25f,
        };

        /// <summary>
        /// Gets the default foreground colour.
        /// </summary>
        /// <value>
        /// The default foreground colour.
        /// </value>
        public static Color ForegroundColour => Color.Black;

        /// <summary>
        /// Gets the default passage reference.
        /// </summary>
        /// <value>
        /// The default passage reference.
        /// </value>
        public static PassageReference PassageReference => new PassageReference();
    }
}
