// -----------------------------------------------------------------------
// <copyright file="RenderingSuggestionsTests.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="RenderingSuggestions"/> class.
/// </summary>
[TestClass]
public class RenderingSuggestionsTests
{
    /// <summary>
    /// Tests the empty object.
    /// </summary>
    [TestMethod]
    public void TestEmpty()
    {
        RenderingSuggestions renderingSuggestions = new RenderingSuggestions();
        Assert.IsFalse(renderingSuggestions.IgnoreCaseDiacriticsAndPunctuation);
        Assert.IsNull(renderingSuggestions.NavigateToChapter);
    }
}
