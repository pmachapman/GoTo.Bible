// -----------------------------------------------------------------------
// <copyright file="PassageReference.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    using System;
    using System.Linq;

    /// <summary>
    /// A passage reference.
    /// </summary>
    public record PassageReference
    {
        /// <summary>
        /// Gets the chapter reference.
        /// </summary>
        /// <value>
        /// The chapter reference.
        /// </value>
        public ChapterReference ChapterReference { get; init; } = new ChapterReference();

        /// <summary>
        /// Gets the passage reference to display.
        /// </summary>
        /// <value>
        /// The passage reference.
        /// </value>
        public string Display { get; init; } = string.Empty;

        /// <summary>
        /// Gets the highlighted verses.
        /// </summary>
        /// <value>
        /// The verses to highlight.
        /// </value>
        /// <remarks>
        /// This may be in the format of individual verse numbers (v1.0 format),
        ///     i.e. <c>1,2,3,4,5</c>
        /// or a starting number, followed by a hypen, then an ending number (v1.2 format),
        ///     i.e. <c>1,-,5</c>
        /// The values may include letters,
        ///     i.e. <c>24g,-,25</c>
        /// but not colons, commas, or any other characters.
        /// </remarks>
        public string[] HighlightedVerses { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// We only require the chapter reference. This method assumes you used <c>AsPassageReference()</c> to generate this object.
        /// </remarks>
        public bool IsValid => this.ChapterReference.IsValid;

        /// <inheritdoc/>
        public virtual bool Equals(PassageReference? other)
         => other is not null
            && this.ChapterReference == other.ChapterReference
            && this.Display == other.Display
            && this.HighlightedVerses.SequenceEqual(other.HighlightedVerses)
            && this.IsValid == other.IsValid;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            HashCode hashCode = default;
            hashCode.Add(this.ChapterReference);
            hashCode.Add(this.Display);
            foreach (string highlightedVerse in this.HighlightedVerses)
            {
                hashCode.Add(highlightedVerse);
            }

            return hashCode.ToHashCode();
        }
    }
}
