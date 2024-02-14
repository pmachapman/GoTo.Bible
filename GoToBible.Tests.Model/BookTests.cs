// -----------------------------------------------------------------------
// <copyright file="BookTests.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

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
        Assert.AreEqual(string.Empty, book.Name);
        Assert.AreEqual(0, book.Chapters.Count);
        Assert.AreEqual(string.Empty, book.ToString());
    }

    /// <summary>
    /// Tests the <see cref="Book.Name"/> property.
    /// </summary>
    [TestMethod]
    public void TestName()
    {
        Book book = new Book { Name = "Genesis", };
        Assert.AreEqual("Genesis", book.Name);
        Assert.AreEqual(0, book.Chapters.Count);
        Assert.AreEqual("Genesis", book.ToString());
    }
}
