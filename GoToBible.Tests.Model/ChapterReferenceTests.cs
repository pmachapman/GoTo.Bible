// -----------------------------------------------------------------------
// <copyright file="ChapterReferenceTests.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

using GoToBible.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests the <see cref="ChapterReference"/> class.
/// </summary>
[TestClass]
public class ChapterReferenceTests
{
    /// <summary>
    /// Tests the empty constructor.
    /// </summary>
    [TestMethod]
    public void TestEmptyConstructor()
    {
        ChapterReference chapterReference = new ChapterReference();
        Assert.AreEqual(string.Empty, chapterReference.Book);
        Assert.AreEqual(0, chapterReference.ChapterNumber);
        Assert.AreEqual(string.Empty, chapterReference.ToString());
        Assert.IsFalse(chapterReference.IsValid);
    }

    /// <summary>
    /// Tests an invalid constructor.
    /// </summary>
    [TestMethod]
    public void TestBookAndChapterInvalidConstructor()
    {
        ChapterReference chapterReference = new ChapterReference(string.Empty);
        Assert.AreEqual(string.Empty, chapterReference.Book);
        Assert.AreEqual(0, chapterReference.ChapterNumber, 0);
        Assert.AreEqual(string.Empty, chapterReference.ToString());
        Assert.IsFalse(chapterReference.IsValid);
    }

    /// <summary>
    /// Tests an invalid chapter constructor.
    /// </summary>
    [TestMethod]
    public void TestBookAndChapterInvalidChapterConstructor()
    {
        ChapterReference chapterReference = new ChapterReference("Genesis Fifty");
        Assert.AreEqual("Genesis", chapterReference.Book);
        Assert.AreEqual(0, chapterReference.ChapterNumber);
        Assert.AreEqual("Genesis 0", chapterReference.ToString());
        Assert.IsTrue(chapterReference.IsValid);
    }

    /// <summary>
    /// Tests a missing chapter constructor.
    /// </summary>
    [TestMethod]
    public void TestBookAndChapterMissingChapterConstructor()
    {
        ChapterReference chapterReference = new ChapterReference("Genesis");
        Assert.AreEqual("Genesis", chapterReference.Book);
        Assert.AreEqual(0, chapterReference.ChapterNumber);
        Assert.AreEqual("Genesis 0", chapterReference.ToString());
        Assert.IsTrue(chapterReference.IsValid);
    }

    /// <summary>
    /// Tests a chapter constructor for a book that starts with a number and missing a chapter.
    /// </summary>
    [TestMethod]
    public void TestBookAndChapterBookStartsWithNumberMissingChapterConstructor()
    {
        ChapterReference chapterReference = new ChapterReference("2 John");
        Assert.AreEqual("2 John", chapterReference.Book);
        Assert.AreEqual(0, chapterReference.ChapterNumber);
        Assert.AreEqual("2 John 0", chapterReference.ToString());
        Assert.IsTrue(chapterReference.IsValid);
    }

    /// <summary>
    /// Tests a valid chapter constructor.
    /// </summary>
    [TestMethod]
    public void TestBookAndChapterValidConstructor()
    {
        ChapterReference chapterReference = new ChapterReference("Genesis 50");
        Assert.AreEqual("Genesis", chapterReference.Book);
        Assert.AreEqual(50, chapterReference.ChapterNumber);
        Assert.AreEqual("Genesis 50", chapterReference.ToString());
        Assert.IsTrue(chapterReference.IsValid);
    }

    /// <summary>
    /// Tests an invalid two parameter constructor.
    /// </summary>
    [TestMethod]
    public void TestTwoParameterInvalidConstructor()
    {
        ChapterReference chapterReference = new ChapterReference(string.Empty, 0);
        Assert.AreEqual(string.Empty, chapterReference.Book);
        Assert.AreEqual(0, chapterReference.ChapterNumber, 0);
        Assert.AreEqual(string.Empty, chapterReference.ToString());
        Assert.IsFalse(chapterReference.IsValid);
    }

    /// <summary>
    /// Tests an invalid two parameter constructor for Psalm 151.
    /// </summary>
    [TestMethod]
    public void TestTwoParameterPsalm151Constructor()
    {
        ChapterReference chapterReference = new ChapterReference("Psalm 151", 0);
        Assert.AreEqual("Psalm", chapterReference.Book);
        Assert.AreEqual(151, chapterReference.ChapterNumber);
        Assert.AreEqual("Psalm 151", chapterReference.ToString());
        Assert.IsTrue(chapterReference.IsValid);
    }

    /// <summary>
    /// Tests a valid two parameter constructor.
    /// </summary>
    [TestMethod]
    public void TestTwoParameterValidConstructor()
    {
        ChapterReference chapterReference = new ChapterReference("Genesis", 50);
        Assert.AreEqual("Genesis", chapterReference.Book);
        Assert.AreEqual(50, chapterReference.ChapterNumber);
        Assert.AreEqual("Genesis 50", chapterReference.ToString());
        Assert.IsTrue(chapterReference.IsValid);
    }
}
