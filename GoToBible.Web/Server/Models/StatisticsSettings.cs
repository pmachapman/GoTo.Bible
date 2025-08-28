// -----------------------------------------------------------------------
// <copyright file="StatisticsSettings.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Models;

/// <summary>
/// Statistics Configuration Settings.
/// </summary>
public class StatisticsSettings
{
    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    /// <value>
    /// The connection string.
    /// </value>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the statistics database provider.
    /// </summary>
    /// <value>
    /// The name of the statistics database provider.
    /// </value>
    /// <remarks>This may be <c>MSSQL</c>, <c>MSSQL</c>, or <c>MARIADB</c>.</remarks>
    public string? DatabaseProvider { get; set; }

    /// <summary>
    /// Gets or sets the database version.
    /// </summary>
    /// <value>
    /// The database version.
    /// </value>
    /// <remarks>
    /// Only required for MySQL and MariaDB. This may be null/empty, in which case the latest supported version will be used.
    /// </remarks>
    public string? DatabaseVersion { get; set; }
}
