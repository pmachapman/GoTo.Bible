// -----------------------------------------------------------------------
// <copyright file="RenderingParametersTests.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model
{
    using GoToBible.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the <see cref="RenderingParameters"/> class.
    /// </summary>
    [TestClass]
    public class RenderingParametersTests
    {
        /// <summary>
        /// Tests the empty object.
        /// </summary>
        [TestMethod]
        public void TestEmpty()
        {
            RenderingParameters renderingParameters = new RenderingParameters();
            Assert.AreEqual(renderingParameters.BackgroundColour, Default.BackgroundColour);
            Assert.AreEqual(renderingParameters.Font, Default.Font);
            Assert.AreEqual(renderingParameters.ForegroundColour, Default.ForegroundColour);
            Assert.AreEqual(renderingParameters.Format, RenderFormat.Html);
            Assert.AreEqual(renderingParameters.HighlightColour, Default.HighlightColour);
            Assert.IsFalse(renderingParameters.InterlinearIgnoresCase);
            Assert.IsFalse(renderingParameters.InterlinearIgnoresDiacritics);
            Assert.IsFalse(renderingParameters.InterlinearIgnoresPunctuation);
            Assert.IsFalse(renderingParameters.IsDebug);
            Assert.AreEqual(renderingParameters.PassageReference, Default.PassageReference);
            Assert.AreEqual(renderingParameters.PrimaryProvider, string.Empty);
            Assert.AreEqual(renderingParameters.PrimaryTranslation, string.Empty);
            Assert.IsTrue(renderingParameters.RenderItalics);
            Assert.IsNull(renderingParameters.SecondaryProvider);
            Assert.IsNull(renderingParameters.SecondaryTranslation);
        }
    }
}
