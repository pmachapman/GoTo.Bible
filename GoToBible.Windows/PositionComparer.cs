// -----------------------------------------------------------------------
// <copyright file="PositionComparer.cs" company="Conglomo">
// Copyright 2020-2026 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows;

using System;
using System.Collections.Generic;

/// <summary>
/// A string comparer based on the position of a string in the list specified in the constructor.
/// </summary>
/// <param name="orderBy">The list to order by.</param>
/// <typeparam name="T">The type.</typeparam>
public class PositionComparer<T>(IList<T> orderBy) : IComparer<T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    public int Compare(T? x, T? y)
    {
        if (x is not null && y is not null && orderBy.Contains(x) && orderBy.Contains(y))
        {
            return orderBy.IndexOf(x).CompareTo(orderBy.IndexOf(y));
        }
        else if (
            x is not null
            && y is not null
            && !orderBy.Contains(x)
            && !orderBy.Contains(y)
        )
        {
            return x.CompareTo(y);
        }
        else if (
            x is not null
            && orderBy.Contains(x)
            && (y is null || !orderBy.Contains(y))
        )
        {
            return 1;
        }
        else if (
            y is not null
            && orderBy.Contains(y)
            && (x is null || !orderBy.Contains(x))
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
