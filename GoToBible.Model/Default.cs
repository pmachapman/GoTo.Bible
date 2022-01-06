// -----------------------------------------------------------------------
// <copyright file="Default.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    /// <summary>
    /// The renderer defaults.
    /// </summary>
    public static class Default
    {
        /// <summary>
        /// The default to passage to display if none is specified.
        /// </summary>
        public const string Passage = "John 3:16";

        /// <summary>
        /// Gets the default background colour.
        /// </summary>
        /// <value>
        /// The default background colour.
        /// </value>
        /// <remarks>
        /// This is white by default.
        /// </remarks>
        public static RenderColour BackgroundColour => new RenderColour { R = 255, G = 255, B = 255 };

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
        /// <remarks>
        /// This is black by default.
        /// </remarks>
        public static RenderColour ForegroundColour => new RenderColour { R = 0, G = 0, B = 0 };

        /// <summary>
        /// Gets the default highlight colour.
        /// </summary>
        /// <value>
        /// The default highlight colour.
        /// </value>
        /// <remarks>
        /// This is yellow by default.
        /// </remarks>
        public static RenderColour HighlightColour => new RenderColour { R = 255, G = 255, B = 0 };

        /// <summary>
        /// Gets the default passage reference.
        /// </summary>
        /// <value>
        /// The default passage reference.
        /// </value>
        public static PassageReference PassageReference => new PassageReference();
    }
}
