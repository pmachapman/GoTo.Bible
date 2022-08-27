// -----------------------------------------------------------------------
// <copyright file="RenderFontTests.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="RenderFont"/> class.
/// </summary>
[TestClass]
public class RenderFontTests
{
    /// <summary>
    /// Tests the empty object.
    /// </summary>
    [TestMethod]
    public void TestEmpty()
    {
        RenderFont renderFont = new RenderFont();
        Assert.IsFalse(renderFont.Bold);
        Assert.AreEqual(string.Empty, renderFont.FamilyName);
        Assert.IsFalse(renderFont.Italic);
        Assert.AreEqual(0, renderFont.SizeInPoints);
        Assert.IsFalse(renderFont.Strikeout);
        Assert.IsFalse(renderFont.Underline);
    }
}
