// -----------------------------------------------------------------------
// <copyright file="RenderFormat.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    /// <summary>
    /// The format to render in.
    /// </summary>
    public enum RenderFormat
    {
        /// <summary>
        /// Render as text.
        /// </summary>
        Text = 0,

        /// <summary>
        /// Render as HTML.
        /// </summary>
        Html = 1,

        /// <summary>
        /// Render for import into Accordance.
        /// </summary>
        Accordance = 2,
    }
}
