// -----------------------------------------------------------------------
// <copyright file="StatisticsContext.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Models
{
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The statistics data context.
    /// </summary>
    public class StatisticsContext : DbContext
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="StatisticsContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public StatisticsContext(DbContextOptions<StatisticsContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the statistics.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        public DbSet<Statistics> Statistics { get; set; } = default!;
    }
}
