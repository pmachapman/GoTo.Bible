// -----------------------------------------------------------------------
// <copyright file="ApparatusRenderingParametersTests.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="ApparatusRenderingParameters"/> class.
/// </summary>
[TestClass]
public class ApparatusRenderingParametersTests
{
    /// <summary>
    /// Tests the empty object.
    /// </summary>
    [TestMethod]
    public void TestEmpty()
    {
        ApparatusRenderingParameters renderingParameters = new ApparatusRenderingParameters();
        Assert.AreEqual(Default.BackgroundColour, renderingParameters.BackgroundColour);
        Assert.AreEqual(Default.Font, renderingParameters.Font);
        Assert.AreEqual(Default.ForegroundColour, renderingParameters.ForegroundColour);
        Assert.AreEqual(RenderFormat.Html, renderingParameters.Format);
        Assert.AreEqual(Default.HighlightColour, renderingParameters.HighlightColour);
        Assert.IsFalse(renderingParameters.InterlinearIgnoresCase);
        Assert.IsFalse(renderingParameters.InterlinearIgnoresDiacritics);
        Assert.IsFalse(renderingParameters.InterlinearIgnoresPunctuation);
        Assert.IsFalse(renderingParameters.IsDebug);
        Assert.AreEqual(
            "<span class=\"occurrence\">%OCCURRENCE%</span>",
            renderingParameters.OccurrenceMarker
        );
        Assert.AreEqual("<em>Omit</em>", renderingParameters.OmissionMarker);
        Assert.AreEqual(Default.PassageReference, renderingParameters.PassageReference);
        Assert.AreEqual(string.Empty, renderingParameters.PrimaryProvider);
        Assert.AreEqual(string.Empty, renderingParameters.PrimaryTranslation);
        Assert.IsTrue(renderingParameters.RenderItalics);
        Assert.IsFalse(renderingParameters.RenderNeighbourForAddition);
        Assert.IsNull(renderingParameters.SecondaryProvider);
        Assert.IsNull(renderingParameters.SecondaryTranslation);
    }
}
