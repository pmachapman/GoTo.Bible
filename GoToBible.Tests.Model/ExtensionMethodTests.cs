// -----------------------------------------------------------------------
// <copyright file="ExtensionMethodTests.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model
{
    using System;
    using System.Linq;
    using GoToBible.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the <see cref="ExtensionMethods"/> class.
    /// </summary>
    [TestClass]
    public class ExtensionMethodTests
    {
        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsPassageReference(ChapterReference)"/>.
        /// </summary>
        [TestMethod]
        public void TestChapterReferenceAsPassageReference()
        {
            ChapterReference chapterReference = new ChapterReference("1 John", 1);
            PassageReference expected = new PassageReference()
            {
                ChapterReference = chapterReference,
                Display = "1 John 1",
                HighlightedVerses = Array.Empty<string>(),
            };
            PassageReference actual = chapterReference.AsPassageReference();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsPassageReference(string, int)"/>.
        /// </summary>
        [TestMethod]
        public void TestStringAsPassageReference()
        {
            PassageReference expected = new PassageReference()
            {
                ChapterReference = new ChapterReference("1 John", 1),
                Display = "1 John 1:3,6-7,9-12",
                HighlightedVerses = new string[] { "3", "6", "-", "7", "9", "-", "12" },
            };
            PassageReference actual = "1 John 1:3,6-7,9-12".AsPassageReference();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsPassageReference(string, int)"/>.
        /// </summary>
        [TestMethod]
        public void TestStringAsPassageReferenceOneChapterBookIntroduction()
        {
            PassageReference expected = new PassageReference()
            {
                ChapterReference = new ChapterReference("2 John", 0),
                Display = "2 John 0",
            };
            PassageReference actual = "2 John 0".AsPassageReference();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsPassageReference(string, int)"/>.
        /// </summary>
        [TestMethod]
        public void TestStringAsPassageReferenceOneChapterBookNoColon()
        {
            PassageReference expected = new PassageReference()
            {
                ChapterReference = new ChapterReference("2 John", 1),
                Display = "2 John 1",
            };
            PassageReference actual = "2 John 1".AsPassageReference();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsPassageReference(string, int)"/>.
        /// </summary>
        [TestMethod]
        public void TestStringAsPassageReferenceOneChapterBookNoColonWithVerse()
        {
            PassageReference expected = new PassageReference()
            {
                ChapterReference = new ChapterReference("2 John", 1),
                Display = "2 John 1:2",
                HighlightedVerses = new string[] { "2" },
            };
            PassageReference actual = "2 John 2".AsPassageReference();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsPassageReference(string, int)"/>.
        /// </summary>
        [TestMethod]
        public void TestStringAsPassageReferenceOneChapterBookNoColonWithRange()
        {
            PassageReference expected = new PassageReference()
            {
                ChapterReference = new ChapterReference("2 John", 1),
                Display = "2 John 1:1-2",
                HighlightedVerses = new string[] { "1", "-", "2" },
            };
            PassageReference actual = "2 John 1-2".AsPassageReference();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsPassageReference(string, int)"/>.
        /// </summary>
        [TestMethod]
        public void TestStringAsPassageReferenceOneChapterBookWithColon()
        {
            PassageReference expected = new PassageReference()
            {
                ChapterReference = new ChapterReference("2 John", 1),
                Display = "2 John 1:1",
                HighlightedVerses = new string[] { "1" },
            };
            PassageReference actual = "2 John 1:1".AsPassageReference();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsPassageReference(string, int)"/>.
        /// </summary>
        [TestMethod]
        public void TestStringAsPassageReferenceWithLetters()
        {
            PassageReference expected = new PassageReference()
            {
                ChapterReference = new ChapterReference("1 Kings", 12),
                Display = "1 Kings 12:24b-24g,24y-25",
                HighlightedVerses = new string[] { "24b", "-", "24g", "24y", "-", "25" },
            };
            PassageReference actual = "1 Kings 12:24b-24g,24y-25".AsPassageReference();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsUrl"/> for an empty <see cref="RenderingParameters"/>.
        /// </summary>
        [TestMethod]
        public void TestAsUrl_RenderingParametersEmpty()
        {
            RenderingParameters renderingParameters = new RenderingParameters();
            Assert.AreEqual(new Uri("https://goto.bible/"), renderingParameters.AsUrl());
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsUrl"/> for a <see cref="RenderingParameters"/> with one translation.
        /// </summary>
        [TestMethod]
        public void TestAsUrl_RenderingParametersOneTranslation()
        {
            RenderingParameters renderingParameters = new RenderingParameters();
            renderingParameters.PassageReference.Display = "1 John 1:3,6-7";
            renderingParameters.PrimaryTranslation = "ESV";
            Assert.AreEqual(new Uri("https://goto.bible/1.John.1_3~6-7/ESV"), renderingParameters.AsUrl());
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.AsUrl"/> for a <see cref="RenderingParameters"/> with two translations.
        /// </summary>
        [TestMethod]
        public void TestAsUrl_RenderingParametersTwoTranslations()
        {
            RenderingParameters renderingParameters = new RenderingParameters();
            renderingParameters.PassageReference.Display = "1 John 1:3,6-7";
            renderingParameters.PrimaryTranslation = "ESV";
            renderingParameters.SecondaryTranslation = "NET";
            Assert.AreEqual(new Uri("https://goto.bible/1.John.1_3~6-7/ESV/NET"), renderingParameters.AsUrl());
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.DecodePassageFromUrl"/>.
        /// </summary>
        [TestMethod]
        public void TestDecodePassageFromUrl() => Assert.AreEqual("1 John 1:3,6-7", "/1.John.1_3~6-7/".DecodePassageFromUrl());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.EncodePassageForUrl"/>.
        /// </summary>
        [TestMethod]
        public void TestEncodePassageFromUrl() => Assert.AreEqual("1.John.1_3~6-7", "1 John 1:3,6-7".EncodePassageForUrl());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.GetBook"/>.
        /// </summary>
        [TestMethod]
        public void TestGetBook() => Assert.AreEqual("1 john", "1jn1:1".GetBook());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.GetBook"/> for a book with brackets.
        /// </summary>
        [TestMethod]
        public void TestGetBookWithBrackets() => Assert.AreEqual("esther (greek)", "est(greek)1:1".GetBook());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.GetRanges"/>.
        /// </summary>
        [TestMethod]
        public void TestGetRanges()
        {
            string[] expected = new string[] { "1:1", "1:2", "...", "1:3", "1:4", "1:5", "...", "1:6" };
            Assert.IsTrue("1john1:1,2-3,4,5-6".GetRanges().SequenceEqual(expected));
        }

        /// <summary>
        /// Tests <see cref="ExtensionMethods.NormaliseCommas"/>.
        /// </summary>
        [TestMethod]
        public void TestNormaliseCommas() => Assert.AreEqual("1john1:1;1john1:2-3;1john1:4;1john1:5-6", "1john1:1,2-3,4,5-6".NormaliseCommas());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.NormaliseSingleChapterReference"/> for a single chapter reference.
        /// </summary>
        [TestMethod]
        public void TestNormaliseSingleChapterReference() => Assert.AreEqual("jude1:1", "jude1".NormaliseSingleChapterReference());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.NormaliseSingleChapterReference"/> for a single chapter reference with a chapter already specified.
        /// </summary>
        [TestMethod]
        public void TestNormaliseSingleChapterReferenceWithChapterAlreadySpecified() => Assert.AreEqual("jude1:1", "jude1:1".NormaliseSingleChapterReference());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.NormaliseSingleChapterReference"/> for a book beginning with a number.
        /// </summary>
        [TestMethod]
        public void TestNormaliseSingleChapterReferenceForBookBeginningWithNumber() => Assert.AreEqual("2john", "2john".NormaliseSingleChapterReference());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.RenderCss"/> for blank <see cref="RenderingParameters"/>.
        /// </summary>
        [TestMethod]
        public void TestRenderCssBlank() => Assert.AreNotEqual(string.Empty, new RenderingParameters().RenderCss());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.SanitisePassageReference"/>.
        /// </summary>
        [TestMethod]
        public void TestSanitisePassageReference() => Assert.AreEqual("1john1:1-2", "1 John 1.1‐2".SanitisePassageReference());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.ToHtml(RenderColour)"/> for black.
        /// </summary>
        [TestMethod]
        public void TestToHtml_Black() => Assert.AreEqual("#000000", new RenderColour { R = 0, G = 0, B = 0 }.ToHtml());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.ToHtml(RenderColour)"/> for blue.
        /// </summary>
        [TestMethod]
        public void TestToHtml_Blue() => Assert.AreEqual("#0000FF", new RenderColour { R = 0, G = 0, B = 255 }.ToHtml());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.ToHtml(RenderColour)"/> for green.
        /// </summary>
        [TestMethod]
        public void TestToHtml_Green() => Assert.AreEqual("#00FF00", new RenderColour { R = 0, G = 255, B = 0 }.ToHtml());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.ToHtml(RenderColour)"/> for red.
        /// </summary>
        [TestMethod]
        public void TestToHtml_Red() => Assert.AreEqual("#FF0000", new RenderColour { R = 255, G = 0, B = 0 }.ToHtml());

        /// <summary>
        /// Tests <see cref="ExtensionMethods.ToHtml(RenderColour)"/> for white.
        /// </summary>
        [TestMethod]
        public void TestToHtml_White() => Assert.AreEqual("#FFFFFF", new RenderColour { R = 255, G = 255, B = 255 }.ToHtml());
    }
}
