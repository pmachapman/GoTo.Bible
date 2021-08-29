// -----------------------------------------------------------------------
// <copyright file="ChapterTests.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model
{
    using GoToBible.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the <see cref="Chapter"/> class.
    /// </summary>
    [TestClass]
    public class ChapterTests
    {
        /// <summary>
        /// Tests the empty object.
        /// </summary>
        [TestMethod]
        public void TestEmpty()
        {
            Chapter chapter = new Chapter();
            Assert.AreEqual(chapter.Book, string.Empty);
            Assert.AreEqual(chapter.ChapterNumber, 0);
            Assert.AreEqual(chapter.Copyright, string.Empty);
            Assert.AreEqual(chapter.NextChapterReference.ToString(), string.Empty);
            Assert.AreEqual(chapter.PreviousChapterReference.ToString(), string.Empty);
            Assert.IsFalse(chapter.SupportsItalics);
            Assert.AreEqual(chapter.Text, string.Empty);
            Assert.AreEqual(chapter.Translation, string.Empty);
        }
    }
}
