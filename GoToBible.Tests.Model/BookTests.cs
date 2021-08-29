// -----------------------------------------------------------------------
// <copyright file="BookTests.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model
{
    using GoToBible.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the <see cref="Book"/> class.
    /// </summary>
    [TestClass]
    public class BookTests
    {
        /// <summary>
        /// Tests the empty object.
        /// </summary>
        [TestMethod]
        public void TestEmpty()
        {
            Book book = new Book();
            Assert.AreEqual(book.Name, string.Empty);
            Assert.AreEqual(book.Chapters.Count, 0);
            Assert.AreEqual(book.ToString(), string.Empty);
        }

        /// <summary>
        /// Tests the <see cref="Book.Name"/> property.
        /// </summary>
        [TestMethod]
        public void TestName()
        {
            Book book = new Book()
            {
                Name = "Genesis",
            };
            Assert.AreEqual(book.Name, "Genesis");
            Assert.AreEqual(book.Chapters.Count, 0);
            Assert.AreEqual(book.ToString(), "Genesis");
        }
    }
}
