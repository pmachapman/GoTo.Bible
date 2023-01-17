// -----------------------------------------------------------------------
// <copyright file="RenderColourTests.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="RenderColour"/> class.
/// </summary>
[TestClass]
public class RenderColourTests
{
    /// <summary>
    /// Tests the empty object.
    /// </summary>
    [TestMethod]
    public void TestEmpty()
    {
        RenderColour renderColour = new RenderColour();
        Assert.AreEqual(0, renderColour.R);
        Assert.AreEqual(0, renderColour.G);
        Assert.AreEqual(0, renderColour.B);
    }
}
