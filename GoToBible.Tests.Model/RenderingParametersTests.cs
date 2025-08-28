// -----------------------------------------------------------------------
// <copyright file="RenderingParametersTests.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="RenderingParameters"/> class.
/// </summary>
[TestClass]
public class RenderingParametersTests
{
    /// <summary>
    /// Tests the empty object.
    /// </summary>
    [TestMethod]
    public void TestEmpty()
    {
        RenderingParameters renderingParameters = new RenderingParameters();
        Assert.AreEqual(Default.BackgroundColour, renderingParameters.BackgroundColour);
        Assert.AreEqual(Default.Font, renderingParameters.Font);
        Assert.AreEqual(Default.ForegroundColour, renderingParameters.ForegroundColour);
        Assert.AreEqual(RenderFormat.Html, renderingParameters.Format);
        Assert.AreEqual(Default.HighlightColour, renderingParameters.HighlightColour);
        Assert.IsFalse(renderingParameters.InterlinearIgnoresCase);
        Assert.IsFalse(renderingParameters.InterlinearIgnoresDiacritics);
        Assert.IsFalse(renderingParameters.InterlinearIgnoresPunctuation);
        Assert.IsFalse(renderingParameters.IsDebug);
        Assert.AreEqual(Default.PassageReference, renderingParameters.PassageReference);
        Assert.AreEqual(string.Empty, renderingParameters.PrimaryProvider);
        Assert.AreEqual(string.Empty, renderingParameters.PrimaryTranslation);
        Assert.IsTrue(renderingParameters.RenderItalics);
        Assert.IsNull(renderingParameters.SecondaryProvider);
        Assert.IsNull(renderingParameters.SecondaryTranslation);
    }
}
