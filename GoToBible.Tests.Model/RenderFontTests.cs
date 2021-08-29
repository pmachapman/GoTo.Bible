// -----------------------------------------------------------------------
// <copyright file="RenderFontTests.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model
{
    using GoToBible.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the <see cref="RenderFont"/> class.
    /// </summary>
    [TestClass]
    public class RenderFontTests
    {
        /// <summary>
        /// Tests the empty object.
        /// </summary>
        [TestMethod]
        public void TestEmpty()
        {
            RenderFont renderFont = new RenderFont();
            Assert.IsFalse(renderFont.Bold);
            Assert.AreEqual(renderFont.FamilyName, string.Empty);
            Assert.IsFalse(renderFont.Italic);
            Assert.AreEqual(renderFont.SizeInPoints, 0);
            Assert.IsFalse(renderFont.Strikeout);
            Assert.IsFalse(renderFont.Underline);
        }
    }
}
