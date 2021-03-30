// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Extension Methods to the Model.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Renders the CSS.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The CSS code.
        /// </returns>
        public static string RenderCss(this RenderingParameters parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("body{");
            sb.Append($"background-color:{ColorTranslator.ToHtml(parameters.BackgroundColour)};");
            sb.Append($"color:{ColorTranslator.ToHtml(parameters.ForegroundColour)};");
            if (parameters.Font.Bold)
            {
                sb.Append("font-weight:bold;");
            }
            else
            {
                sb.Append("font-weight:normal;");
            }

            if (parameters.Font.Italic)
            {
                sb.Append("font-style:italic;");
            }
            else
            {
                sb.Append("font-style:normal;");
            }

            if (parameters.Font.Strikeout && parameters.Font.Underline)
            {
                sb.Append("text-decoration:underline line-through;");
            }
            else if (parameters.Font.Strikeout)
            {
                sb.Append("text-decoration:line-through;");
            }
            else if (parameters.Font.Underline)
            {
                sb.Append("text-decoration:underline;");
            }
            else
            {
                sb.Append("text-decoration:none;");
            }

            sb.Append($"font-size:{parameters.Font.SizeInPoints}pt;");
            sb.Append($"font-family:{parameters.Font.FamilyName}}}");
            if (parameters.Font.Italic)
            {
                sb.Append("em{font-style:normal}");
            }

            sb.Append($"sup{{font-size:{parameters.Font.SizeInPoints * 0.75f}pt;font-weight:bold}}.sup{{font-weight:bold}}");
            sb.Append($".copyright{{border-top:1px solid {ColorTranslator.ToHtml(parameters.ForegroundColour)};font-size:{Math.Round(parameters.Font.SizeInPoints * 0.75, 2)}pt}}");
            sb.Append(".supsub{display:inline-flex;flex-direction:column;justify-content:space-between;vertical-align:middle;font-size:50%}");
            return sb.ToString();
        }

        /// <summary>
        /// Gets a unique name for the translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="translations">The translations.</param>
        /// <returns>
        /// The unique name.
        /// </returns>
        public static string UniqueName(this Translation translation, IEnumerable<Translation> translations)
        {
            if (translations.Count(t => t.Name == translation.Name) <= 1)
            {
                return translation.Name;
            }
            else if (translations.Count(t => t.Name == translation.Name && t.Year == translation.Year && t.Year > 0) == 1)
            {
                return $"{translation.Name} ({translation.Year})";
            }
            else
            {
                return $"{translation.Name} ({translation.Provider})";
            }
        }
    }
}
