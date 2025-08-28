// -----------------------------------------------------------------------
// <copyright file="InterlinearModeTests.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="InterlinearMode"/> class.
/// </summary>
[TestClass]
public class InterlinearModeTests
{
    /// <summary>
    /// Tests the None value.
    /// </summary>
    [TestMethod]
    public void TestNone() => Assert.AreEqual(0, (int)InterlinearMode.None);

    /// <summary>
    /// Tests the IgnoresCase value.
    /// </summary>
    [TestMethod]
    public void TestIgnoresCase() => Assert.AreEqual(1, (int)InterlinearMode.IgnoresCase);

    /// <summary>
    /// Tests the IgnoresDiacritics value.
    /// </summary>
    [TestMethod]
    public void TestIgnoresDiacritics() =>
        Assert.AreEqual(2, (int)InterlinearMode.IgnoresDiacritics);

    /// <summary>
    /// Tests the IgnoresCase and IgnoresDiacritics value.
    /// </summary>
    [TestMethod]
    public void TestIgnoresCaseAndDiacritics() =>
        Assert.AreEqual(3, (int)(InterlinearMode.IgnoresCase | InterlinearMode.IgnoresDiacritics));

    /// <summary>
    /// Tests the IgnoresPunctuation value.
    /// </summary>
    [TestMethod]
    public void TestIgnoresPunctuation() =>
        Assert.AreEqual(4, (int)InterlinearMode.IgnoresPunctuation);

    /// <summary>
    /// Tests the IgnoresCase and IgnoresPunctuation value.
    /// </summary>
    [TestMethod]
    public void TestIgnoresCaseAndPunctuation() =>
        Assert.AreEqual(5, (int)(InterlinearMode.IgnoresCase | InterlinearMode.IgnoresPunctuation));

    /// <summary>
    /// Tests the IgnoresDiacritics and IgnoresPunctuation value.
    /// </summary>
    [TestMethod]
    public void TestIgnoresDiacriticsAndPunctuation() =>
        Assert.AreEqual(
            6,
            (int)(InterlinearMode.IgnoresDiacritics | InterlinearMode.IgnoresPunctuation)
        );

    /// <summary>
    /// Tests the IgnoresCase, IgnoresDiacritics, and IgnoresPunctuation value.
    /// </summary>
    [TestMethod]
    public void TestIgnoresCaseDiacriticsAndPunctuation() =>
        Assert.AreEqual(
            7,
            (int)(
                InterlinearMode.IgnoresCase
                | InterlinearMode.IgnoresDiacritics
                | InterlinearMode.IgnoresPunctuation
            )
        );
}
