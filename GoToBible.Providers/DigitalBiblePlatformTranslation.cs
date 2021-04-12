// -----------------------------------------------------------------------
// <copyright file="DigitalBiblePlatformTranslation.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GoToBible.Model;

    /// <summary>
    /// A translation from the Digital Bible Platform.
    /// </summary>
    /// <seealso cref="GoToBible.Model.Translation" />
    public class DigitalBiblePlatformTranslation : Translation
    {
        /// <summary>
        /// Gets or sets the DAM Ids for each volume.
        /// </summary>
        /// <value>
        /// The DAM Ids.
        /// </value>
        public ReadOnlyCollection<string> DamIds { get; set; } = new List<string>().AsReadOnly();
    }
}
