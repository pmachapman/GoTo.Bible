// -----------------------------------------------------------------------
// <copyright file="PassageReferenceTests.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="PassageReference"/> class.
/// </summary>
[TestClass]
public class PassageReferenceTests
{
    /// <summary>
    /// Tests an invalid <see cref="PassageReference"/>.
    /// </summary>
    [TestMethod]
    public void TestInvalid()
    {
        PassageReference passageReference = new PassageReference();
        Assert.IsFalse(passageReference.ChapterReference.IsValid);
        Assert.AreEqual(string.Empty, passageReference.Display);
        Assert.AreEqual(0, passageReference.HighlightedVerses.Length);
        Assert.IsFalse(passageReference.IsValid);
    }

    /// <summary>
    /// Tests a valid <see cref="PassageReference"/>.
    /// </summary>
    [TestMethod]
    public void TestValid()
    {
        PassageReference passageReference = new PassageReference
        {
            ChapterReference = new ChapterReference("Psalm 151"),
        };
        Assert.IsTrue(passageReference.ChapterReference.IsValid);
        Assert.AreEqual(string.Empty, passageReference.Display);
        Assert.AreEqual(0, passageReference.HighlightedVerses.Length);
        Assert.IsTrue(passageReference.IsValid);
    }
}
