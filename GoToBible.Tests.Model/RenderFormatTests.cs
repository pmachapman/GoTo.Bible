// -----------------------------------------------------------------------
// <copyright file="RenderFormatTests.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
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
    /// Tests <see cref="RenderFormat.Html"/>.
    /// </summary>
    [TestMethod]
    public void TestHtml() => Assert.AreEqual(1, (int)RenderFormat.Html);

    /// <summary>
    /// Tests <see cref="RenderFormat.Text"/>.
    /// </summary>
    [TestMethod]
    public void TestText() => Assert.AreEqual(0, (int)RenderFormat.Text);
}
