// -----------------------------------------------------------------------
// <copyright file="TranslationTests.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model
{
    using GoToBible.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the <see cref="Translation"/> class.
    /// </summary>
    [TestClass]
    public class TranslationTests
    {
        /// <summary>
        /// Tests the empty object.
        /// </summary>
        [TestMethod]
        public void TestEmpty()
        {
            Translation translation = new Translation();
            Assert.IsNull(translation.Author);
            Assert.IsFalse(translation.CanBeExported);
            Assert.AreEqual(translation.Code, string.Empty);
            Assert.IsFalse(translation.Commentary);
            Assert.IsNull(translation.Copyright);
            Assert.IsNull(translation.Dialect);
            Assert.IsNull(translation.Language);
            Assert.AreEqual(translation.Provider, string.Empty);
            Assert.AreEqual(translation.Name, string.Empty);
            Assert.AreEqual(translation.Year, 0);
            Assert.AreEqual(translation.ToString(), string.Empty);
        }

        /// <summary>
        /// Tests the <see cref="Translation"/> with a name specified.
        /// </summary>
        [TestMethod]
        public void TestName()
        {
            Translation translation = new Translation
            {
                Name = "KJV",
            };
            Assert.IsNull(translation.Author);
            Assert.IsFalse(translation.CanBeExported);
            Assert.AreEqual(translation.Code, string.Empty);
            Assert.IsFalse(translation.Commentary);
            Assert.IsNull(translation.Copyright);
            Assert.IsNull(translation.Dialect);
            Assert.IsNull(translation.Language);
            Assert.AreEqual(translation.Provider, string.Empty);
            Assert.AreEqual(translation.Name, "KJV");
            Assert.AreEqual(translation.Year, 0);
            Assert.AreEqual(translation.ToString(), "KJV");
        }

        /// <summary>
        /// Tests the <see cref="Translation"/> with a name and language specified.
        /// </summary>
        [TestMethod]
        public void TestNameAndLanguage()
        {
            Translation translation = new Translation
            {
                Language = "English",
                Name = "KJV",
            };
            Assert.IsNull(translation.Author);
            Assert.IsFalse(translation.CanBeExported);
            Assert.AreEqual(translation.Code, string.Empty);
            Assert.IsFalse(translation.Commentary);
            Assert.IsNull(translation.Copyright);
            Assert.IsNull(translation.Dialect);
            Assert.AreEqual(translation.Language, "English");
            Assert.AreEqual(translation.Provider, string.Empty);
            Assert.AreEqual(translation.Name, "KJV");
            Assert.AreEqual(translation.Year, 0);
            Assert.AreEqual(translation.ToString(), "English: KJV");
        }
    }
}
