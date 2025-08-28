// -----------------------------------------------------------------------
// <copyright file="RenderedPassageTests.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="RenderedPassage"/> class.
/// </summary>
[TestClass]
public class RenderedPassageTests
{
    /// <summary>
    /// Tests the empty object.
    /// </summary>
    [TestMethod]
    public void TestEmpty()
    {
        RenderedPassage renderedPassage = new RenderedPassage();
        Assert.AreEqual(string.Empty, renderedPassage.Content);
        Assert.IsFalse(renderedPassage.NextPassage.IsValid);
        Assert.IsFalse(renderedPassage.PreviousPassage.IsValid);
        Assert.IsFalse(renderedPassage.Suggestions.IgnoreCaseDiacriticsAndPunctuation);
        Assert.IsNull(renderedPassage.Suggestions.NavigateToChapter);
    }
}
