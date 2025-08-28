// -----------------------------------------------------------------------
// <copyright file="LanguageComparerTests.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using System;
using System.Linq;
using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="LanguageComparer"/> class.
/// </summary>
[TestClass]
public class LanguageComparerTests
{
    /// <summary>
    /// Tests the sort order of the <see cref="LanguageComparer"/>.
    /// </summary>
    [TestMethod]
    public void TestSortOrder()
    {
        string[] actual =
        [
            "German",
            "Latin",
            "Aramaic",
            "Greek",
            "French",
            "English",
            "Ugaritic",
            "Hebrew",
        ];
        string[] expected =
        [
            "English",
            "Greek",
            "Hebrew",
            "Latin",
            "Aramaic",
            "French",
            "German",
            "Ugaritic",
        ];
        Array.Sort(actual, new LanguageComparer());
        Assert.IsTrue(actual.SequenceEqual(expected));
    }
}
