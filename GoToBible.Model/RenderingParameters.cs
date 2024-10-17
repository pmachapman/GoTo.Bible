// -----------------------------------------------------------------------
// <copyright file="RenderingParameters.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

/// <summary>
/// The rendering parameters.
/// </summary>
public record RenderingParameters
{
    /// <summary>
    /// Gets or sets the background colour.
    /// </summary>
    /// <value>
    /// The background colour.
    /// </value>
    public RenderColour BackgroundColour { get; set; } = Default.BackgroundColour;

    /// <summary>
    /// Gets or sets the font.
    /// </summary>
    /// <value>
    /// The font.
    /// </value>
    public RenderFont Font { get; set; } = Default.Font;

    /// <summary>
    /// Gets or sets the foreground colour.
    /// </summary>
    /// <value>
    /// The foreground colour.
    /// </value>
    public RenderColour ForegroundColour { get; set; } = Default.ForegroundColour;

    /// <summary>
    /// Gets or sets the format to render in.
    /// </summary>
    /// <value>
    /// The rendering format.
    /// </value>
    public RenderFormat Format { get; set; } = RenderFormat.Html;

    /// <summary>
    /// Gets or sets the highlight colour.
    /// </summary>
    /// <value>
    /// The highlight colour.
    /// </value>
    public RenderColour HighlightColour { get; set; } = Default.HighlightColour;

    /// <summary>
    /// Gets or sets a value indicating whether the interlinear ignores case.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the interlinear ignores case; otherwise, <c>false</c>.
    /// </value>
    public bool InterlinearIgnoresCase { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the interlinear ignores diacritics.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the interlinear ignores diacritics; otherwise, <c>false</c>.
    /// </value>
    public bool InterlinearIgnoresDiacritics { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the interlinear ignores punctuation.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the interlinear ignores punctuation; otherwise, <c>false</c>.
    /// </value>
    public bool InterlinearIgnoresPunctuation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="RenderingParameters" /> is in debugging mode.
    /// </summary>
    /// <value>
    ///   <c>true</c> if we are debugging; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Debug mode will display extra developer information. It is not suitable for general use.
    /// </remarks>
    public bool IsDebug { get; set; }

    /// <summary>
    /// Gets or sets the passage reference.
    /// </summary>
    /// <value>
    /// The passage reference.
    /// </value>
    public PassageReference PassageReference { get; set; } = Default.PassageReference;

    /// <summary>
    /// Gets or sets the primary chapter.
    /// </summary>
    /// <value>
    /// Optional. The primary chapter.
    /// </value>
    /// <remarks>
    /// When configured this overrides the primary chapter retrieved by the Renderer.
    /// </remarks>
    public Chapter? PrimaryChapter { get; set; }

    /// <summary>
    /// Gets or sets the primary translation provider.
    /// </summary>
    /// <value>
    /// The primary translation provider.
    /// </value>
    public string PrimaryProvider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary translation.
    /// </summary>
    /// <value>
    /// The primary translation.
    /// </value>
    public string PrimaryTranslation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to render supplied words in italics.
    /// </summary>
    /// <value>
    ///   <c>true</c> if we are to render supplied words in italics; otherwise, <c>false</c>.
    /// </value>
    public bool RenderItalics { get; set; } = true;

    /// <summary>
    /// Gets or sets the secondary chapter.
    /// </summary>
    /// <value>
    /// Optional. The secondary chapter.
    /// </value>
    /// <remarks>
    /// When configured this overrides the secondary chapter retrieved by the Renderer.
    /// </remarks>
    public Chapter? SecondaryChapter { get; set; }

    /// <summary>
    /// Gets or sets the secondary translation provider.
    /// </summary>
    /// <value>
    /// The secondary translation provider.
    /// </value>
    public string? SecondaryProvider { get; set; }

    /// <summary>
    /// Gets or sets the secondary translation.
    /// </summary>
    /// <value>
    /// The secondary translation.
    /// </value>
    public string? SecondaryTranslation { get; set; }
}
