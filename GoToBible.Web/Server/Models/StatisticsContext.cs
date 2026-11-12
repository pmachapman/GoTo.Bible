// -----------------------------------------------------------------------
// <copyright file="StatisticsContext.cs" company="Conglomo">
// Copyright 2020-2026 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Models;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The statistics data context.
/// </summary>
/// <param name="options">The statistics database context options.</param>
public class StatisticsContext(DbContextOptions<StatisticsContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the statistics.
    /// </summary>
    /// <value>
    /// The statistics.
    /// </value>
    public DbSet<Statistics> Statistics { get; set; } = default!;
}
