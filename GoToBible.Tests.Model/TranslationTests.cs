// -----------------------------------------------------------------------
// <copyright file="TranslationTests.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Tests.Model;

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
        Assert.AreEqual(string.Empty, translation.Code);
        Assert.IsFalse(translation.Commentary);
        Assert.IsNull(translation.Copyright);
        Assert.IsNull(translation.Dialect);
        Assert.IsNull(translation.Language);
        Assert.AreEqual(string.Empty, translation.Provider);
        Assert.AreEqual(string.Empty, translation.Name);
        Assert.AreEqual(0, translation.Year);
        Assert.AreEqual(string.Empty, translation.ToString());
    }

    /// <summary>
    /// Tests the <see cref="Translation"/> with a name specified.
    /// </summary>
    [TestMethod]
    public void TestName()
    {
        Translation translation = new Translation { Name = "KJV", };
        Assert.IsNull(translation.Author);
        Assert.IsFalse(translation.CanBeExported);
        Assert.AreEqual(string.Empty, translation.Code);
        Assert.IsFalse(translation.Commentary);
        Assert.IsNull(translation.Copyright);
        Assert.IsNull(translation.Dialect);
        Assert.IsNull(translation.Language);
        Assert.AreEqual(string.Empty, translation.Provider);
        Assert.AreEqual("KJV", translation.Name);
        Assert.AreEqual(0, translation.Year);
        Assert.AreEqual("KJV", translation.ToString());
    }

    /// <summary>
    /// Tests the <see cref="Translation"/> with a name and language specified.
    /// </summary>
    [TestMethod]
    public void TestNameAndLanguage()
    {
        Translation translation = new Translation { Language = "English", Name = "KJV", };
        Assert.IsNull(translation.Author);
        Assert.IsFalse(translation.CanBeExported);
        Assert.AreEqual(string.Empty, translation.Code);
        Assert.IsFalse(translation.Commentary);
        Assert.IsNull(translation.Copyright);
        Assert.IsNull(translation.Dialect);
        Assert.AreEqual("English", translation.Language);
        Assert.AreEqual(string.Empty, translation.Provider);
        Assert.AreEqual("KJV", translation.Name);
        Assert.AreEqual(0, translation.Year);
        Assert.AreEqual("English: KJV", translation.ToString());
    }
}
