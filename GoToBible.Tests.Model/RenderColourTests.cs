// -----------------------------------------------------------------------
// <copyright file="RenderColourTests.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model
{
    using GoToBible.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the <see cref="RenderColour"/> class.
    /// </summary>
    [TestClass]
    public class RenderColourTests
    {
        /// <summary>
        /// Tests the empty object.
        /// </summary>
        [TestMethod]
        public void TestEmpty()
        {
            RenderColour renderColour = new RenderColour();
            Assert.AreEqual(renderColour.R, 0);
            Assert.AreEqual(renderColour.G, 0);
            Assert.AreEqual(renderColour.B, 0);
        }
    }
}
