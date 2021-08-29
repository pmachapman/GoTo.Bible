// -----------------------------------------------------------------------
// <copyright file="RenderFormatTests.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model
{
    using GoToBible.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the <see cref="RenderFormat"/> enumeration.
    /// </summary>
    [TestClass]
    public class RenderFormatTests
    {
        /// <summary>
        /// Tests <see cref="RenderFormat.Accordance"/>.
        /// </summary>
        [TestMethod]
        public void TestAccordance() => Assert.AreEqual((int)RenderFormat.Accordance, 2);

        /// <summary>
        /// Tests <see cref="RenderFormat.Html"/>.
        /// </summary>
        [TestMethod]
        public void TestHtml() => Assert.AreEqual((int)RenderFormat.Html, 1);

        /// <summary>
        /// Tests <see cref="RenderFormat.Text"/>.
        /// </summary>
        [TestMethod]
        public void TestText() => Assert.AreEqual((int)RenderFormat.Text, 0);
    }
}
