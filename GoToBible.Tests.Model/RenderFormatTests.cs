// -----------------------------------------------------------------------
// <copyright file="RenderFormatTests.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="RenderFormat"/> enumeration.
/// </summary>
[TestClass]
public class RenderFormatTests
{
    /// <summary>
    /// Tests <see cref="RenderFormat.Accordance"/>.
    /// </summary>
    [TestMethod]
    public void TestAccordance() => Assert.AreEqual(2, (int)RenderFormat.Accordance);

    /// <summary>
    /// Tests <see cref="RenderFormat.Apparatus"/>.
    /// </summary>
    [TestMethod]
    public void TestApparatus() => Assert.AreEqual(3, (int)RenderFormat.Apparatus);

    /// <summary>
    /// Tests <see cref="RenderFormat.Html"/>.
    /// </summary>
    [TestMethod]
    public void TestHtml() => Assert.AreEqual(1, (int)RenderFormat.Html);

    /// <summary>
    /// Tests <see cref="RenderFormat.Spreadsheet"/>.
    /// </summary>
    [TestMethod]
    public void TestSpreadsheet() => Assert.AreEqual(4, (int)RenderFormat.Spreadsheet);

    /// <summary>
    /// Tests <see cref="RenderFormat.Text"/>.
    /// </summary>
    [TestMethod]
    public void TestText() => Assert.AreEqual(0, (int)RenderFormat.Text);
}
