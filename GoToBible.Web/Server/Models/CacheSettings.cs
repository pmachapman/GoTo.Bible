// -----------------------------------------------------------------------
// <copyright file="CacheSettings.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Models
{
    /// <summary>
    /// Cache Configuration Settings.
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// Gets or sets the name of the cache database provider.
        /// </summary>
        /// <value>
        /// The name of the cache database provider.
        /// </value>
        public string DatabaseProvider { get; set; } = string.Empty;
    }
}
