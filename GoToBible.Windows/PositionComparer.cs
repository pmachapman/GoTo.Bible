// -----------------------------------------------------------------------
// <copyright file="PositionComparer.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System;
using System.Collections.Generic;

/// <summary>
/// A string comparer based on the position of a string in the list specified in the constructor.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
public class PositionComparer<T>(IList<T> orderBy) : IComparer<T>
    where T : IComparable<T>
{
    /// <summary>
    /// The list to order by.
    /// </summary>
    private readonly IList<T> orderBy = orderBy;

    /// <inheritdoc />
    public int Compare(T? x, T? y)
    {
        if (x is not null && y is not null && this.orderBy.Contains(x) && this.orderBy.Contains(y))
        {
            return this.orderBy.IndexOf(x).CompareTo(this.orderBy.IndexOf(y));
        }
        else if (
            x is not null
            && y is not null
            && !this.orderBy.Contains(x)
            && !this.orderBy.Contains(y)
        )
        {
            return x.CompareTo(y);
        }
        else if (
            x is not null
            && this.orderBy.Contains(x)
            && (y is null || !this.orderBy.Contains(y))
        )
        {
            return 1;
        }
        else if (
            y is not null
            && this.orderBy.Contains(y)
            && (x is null || !this.orderBy.Contains(x))
        )
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}
