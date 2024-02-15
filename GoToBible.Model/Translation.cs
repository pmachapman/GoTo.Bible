// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

/// <summary>
/// A bible translation.
/// </summary>
public class Translation
{
    /// <summary>
    /// Gets or sets the translation's author.
    /// </summary>
    /// <value>
    /// The translation author.
    /// </value>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance can be exported.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance can be exported; otherwise, <c>false</c>.
    /// </value>
    public bool CanBeExported { get; set; }

    /// <summary>
    /// Gets or sets the translation's unique code.
    /// </summary>
    /// <value>
    /// The unique translation code.
    /// </value>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Translation"/> is a commentary.
    /// </summary>
    /// <value>
    ///   <c>true</c> if a commentary; otherwise, <c>false</c>.
    /// </value>
    public bool Commentary { get; set; }

    /// <summary>
    /// Gets or sets the copyright message.
    /// </summary>
    /// <value>
    /// The copyright message.
    /// </value>
    public string? Copyright { get; set; }

    /// <summary>
    /// Gets or sets the translation's dialect.
    /// </summary>
    /// <value>
    /// The translation dialect.
    /// </value>
    public string? Dialect { get; set; }

    /// <summary>
    /// Gets or sets the translation's language.
    /// </summary>
    /// <value>
    /// The translation language.
    /// </value>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the name of the translation.
    /// </summary>
    /// <value>
    /// The translation name.
    /// </value>
    /// <remarks>
    /// This should be unique for the sake of the UI.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the id of the translation provider.
    /// </summary>
    /// <value>
    /// The translation provider id.
    /// </value>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the year the translation was made.
    /// </summary>
    /// <value>
    /// The translation year.
    /// </value>
    public int Year { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        this.Language is null ? this.Name : $"{this.Language}: {this.Name}";
}
