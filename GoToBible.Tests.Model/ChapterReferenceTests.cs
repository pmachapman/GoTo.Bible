// -----------------------------------------------------------------------
// <copyright file="ChapterReferenceTests.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model
{
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
        public void TestEmpty()
        {
            ChapterReference chapterReference = new ChapterReference();
            Assert.AreEqual(chapterReference.Book, string.Empty);
            Assert.AreEqual(chapterReference.ChapterNumber, 0);
            Assert.AreEqual(chapterReference.ToString(), string.Empty);
            Assert.IsFalse(chapterReference.IsValid);
        }

        /// <summary>
        /// Tests an invalid constructor.
        /// </summary>
        [TestMethod]
        public void TestBookAndChapterInvalidConstructor()
        {
            ChapterReference chapterReference = new ChapterReference(string.Empty);
            Assert.AreEqual(chapterReference.Book, string.Empty);
            Assert.AreEqual(chapterReference.ChapterNumber, 0);
            Assert.AreEqual(chapterReference.ToString(), string.Empty);
            Assert.IsFalse(chapterReference.IsValid);
        }

        /// <summary>
        /// Tests an invalid chapter constructor.
        /// </summary>
        [TestMethod]
        public void TestBookAndChapterInvalidChapterConstructor()
        {
            ChapterReference chapterReference = new ChapterReference("Genesis Fifty");
            Assert.AreEqual(chapterReference.Book, "Genesis");
            Assert.AreEqual(chapterReference.ChapterNumber, 0);
            Assert.AreEqual(chapterReference.ToString(), "Genesis 0");
            Assert.IsTrue(chapterReference.IsValid);
        }

        /// <summary>
        /// Tests a missing chapter constructor.
        /// </summary>
        [TestMethod]
        public void TestBookAndChapterMissingChapterConstructor()
        {
            ChapterReference chapterReference = new ChapterReference("Genesis");
            Assert.AreEqual(chapterReference.Book, "Genesis");
            Assert.AreEqual(chapterReference.ChapterNumber, 0);
            Assert.AreEqual(chapterReference.ToString(), "Genesis 0");
            Assert.IsTrue(chapterReference.IsValid);
        }

        /// <summary>
        /// Tests a chapter constructor for a book that starts with a number and missing a chapter.
        /// </summary>
        [TestMethod]
        public void TestBookAndChapterBookStartsWithNumberMissingChapterConstructor()
        {
            ChapterReference chapterReference = new ChapterReference("2 John");
            Assert.AreEqual(chapterReference.Book, "2 John");
            Assert.AreEqual(chapterReference.ChapterNumber, 0);
            Assert.AreEqual(chapterReference.ToString(), "2 John 0");
            Assert.IsTrue(chapterReference.IsValid);
        }

        /// <summary>
        /// Tests a valid chapter constructor.
        /// </summary>
        [TestMethod]
        public void TestBookAndChapterValidChapterConstructor()
        {
            ChapterReference chapterReference = new ChapterReference("Genesis 50");
            Assert.AreEqual(chapterReference.Book, "Genesis");
            Assert.AreEqual(chapterReference.ChapterNumber, 50);
            Assert.AreEqual(chapterReference.ToString(), "Genesis 50");
            Assert.IsTrue(chapterReference.IsValid);
        }

        /// <summary>
        /// Tests an invalid two parameter constructor.
        /// </summary>
        [TestMethod]
        public void TestTwoParameterInvalidConstructor()
        {
            ChapterReference chapterReference = new ChapterReference(string.Empty, 0);
            Assert.AreEqual(chapterReference.Book, string.Empty);
            Assert.AreEqual(chapterReference.ChapterNumber, 0);
            Assert.AreEqual(chapterReference.ToString(), string.Empty);
            Assert.IsFalse(chapterReference.IsValid);
        }

        /// <summary>
        /// Tests an invalid two parameter constructor for Psalm 151.
        /// </summary>
        [TestMethod]
        public void TestTwoParameterPsalm151Constructor()
        {
            ChapterReference chapterReference = new ChapterReference("Psalm 151", 0);
            Assert.AreEqual(chapterReference.Book, "Psalm");
            Assert.AreEqual(chapterReference.ChapterNumber, 151);
            Assert.AreEqual(chapterReference.ToString(), "Psalm 151");
            Assert.IsTrue(chapterReference.IsValid);
        }

        /// <summary>
        /// Tests a valid two parameter constructor.
        /// </summary>
        [TestMethod]
        public void TestTwoParameterValidConstructor()
        {
            ChapterReference chapterReference = new ChapterReference("Genesis", 50);
            Assert.AreEqual(chapterReference.Book, "Genesis");
            Assert.AreEqual(chapterReference.ChapterNumber, 50);
            Assert.AreEqual(chapterReference.ToString(), "Genesis 50");
            Assert.IsTrue(chapterReference.IsValid);
        }
    }
}
