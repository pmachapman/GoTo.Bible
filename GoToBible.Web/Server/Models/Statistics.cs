// -----------------------------------------------------------------------
// <copyright file="Statistics.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Models;

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// A statistics record.
/// </summary>
public class Statistics
{
    /// <summary>
    /// Gets or sets the accessed at timestamp (UTC).
    /// </summary>
    /// <value>
    /// The date and time the passage was accessed at in UTC.
    /// </value>
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the "forwarded for" header.
    /// </summary>
    /// <value>
    /// The IP address(es) that were forwarded.
    /// </value>
    public string? ForwardedFor { get; set; }

    /// <summary>
    /// Gets or sets the ip address.
    /// </summary>
    /// <value>
    /// The ip address.
    /// </value>
    [MaxLength(39)]
    public string? IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the passage rendered.
    /// </summary>
    /// <value>
    /// The passage rendered.
    /// </value>
    public string Passage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary provider.
    /// </summary>
    /// <value>
    /// The primary provider.
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
    /// Gets or sets the secondary provider.
    /// </summary>
    /// <value>
    /// The secondary provider.
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
