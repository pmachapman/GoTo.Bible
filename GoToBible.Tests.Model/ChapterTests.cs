// -----------------------------------------------------------------------
// <copyright file="ChapterTests.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
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
            Assert.AreEqual(string.Empty, chapter.Book);
            Assert.AreEqual(0, chapter.ChapterNumber);
            Assert.AreEqual(string.Empty, chapter.Copyright);
            Assert.AreEqual(string.Empty, chapter.NextChapterReference.ToString());
            Assert.AreEqual(string.Empty, chapter.PreviousChapterReference.ToString());
            Assert.IsFalse(chapter.SupportsItalics);
            Assert.AreEqual(string.Empty, chapter.Text);
            Assert.AreEqual(string.Empty, chapter.Translation);
        }
    }
}
