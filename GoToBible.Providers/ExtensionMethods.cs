// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Normalises the line endings.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="lineEnding">The line ending to normalise to.</param>
        /// <returns>
        /// The value with normalised line endings.
        /// </returns>
        public static string NormaliseLineEndings(this string value, string lineEnding = "\n")
            => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal).Replace("\n", lineEnding, StringComparison.Ordinal);
    }
}
