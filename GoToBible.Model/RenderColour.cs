// -----------------------------------------------------------------------
// <copyright file="RenderColour.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

/// <summary>
/// The colour to render with.
/// </summary>
/// <remarks>
/// This is used instead of <c>System.Drawing.Color</c>, as that class is not supported on other platforms.
/// The order of the properties is important for serialisation.
/// </remarks>
public record RenderColour
{
    /// <summary>
    /// Gets or sets the red value.
    /// </summary>
    /// <value>
    /// The red value.
    /// </value>
    public byte R { get; set; }

    /// <summary>
    /// Gets or sets the green value.
    /// </summary>
    /// <value>
    /// The green value.
    /// </value>
    public byte G { get; set; }

    /// <summary>
    /// Gets or sets the blue value.
    /// </summary>
    /// <value>
    /// The blue value.
    /// </value>
    public byte B { get; set; }
}
