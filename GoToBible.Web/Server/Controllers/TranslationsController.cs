// -----------------------------------------------------------------------
// <copyright file="TranslationsController.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Controllers;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using GoToBible.Model;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The translations controller.
/// </summary>
/// <seealso cref="ControllerBase" />
[ApiController]
[Route("v1/[controller]")]
[Route("[controller]")]
public class TranslationsController : ControllerBase
{
    /// <summary>
    /// A list of blocked translations that cause unnecessary duplicates.
    /// </summary>
    private static readonly ReadOnlyCollection<string> BlockedTranslations = new List<string>
    {
        "BibleApi-685d1470fe4d5c3b-01",
        "BibleApi-6bab4d6c61b31b80-01",
        "BibleApi-7142879509583d59-02",
        "BibleApi-7142879509583d59-03",
        "BibleApi-7142879509583d59-04",
        "BibleApi-926aa5efbc5e04e2-01",
        "BibleApi-9879dbb7cfe39e4d-02",
        "BibleApi-9879dbb7cfe39e4d-03",
        "BibleApi-9879dbb7cfe39e4d-04",
        "BibleApi-bba9f40183526463-01",
        "BibleApi-de4e12af7f28f599-02",
        "BibleApi-f72b840c855f362c-04",
        "BibliaApi-asv",
        "BibliaApi-kjv",
        "BibliaApi-kjv1900",
        "BibliaApi-kjvapoc",
        "DigitalBiblePlatformApi-AAHWBTN2ET",
        "DigitalBiblePlatformApi-EN1ESV",
        "DigitalBiblePlatformApi-ENGASV",
        "DigitalBiblePlatformApi-ENGESH",
        "DigitalBiblePlatformApi-ENGKJV",
        "DigitalBiblePlatformApi-ENGREV",
        "DigitalBiblePlatformApi-ENGWEB",
    }.AsReadOnly();

    /// <summary>
    /// Name substitutions to help users of the web application.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> NameSubstitutions = new Dictionary<string, string>
    {
        { "The Holy Bible, American Standard Version", "American Standard Version" },
        { "English Standard Version®", "English Standard Version (2007)" },
        { "NAS New American Standard Bible", "New American Standard Bible (1995)" },
        { "King James (Authorised) Version (Ecumenical)", "King James Version" },
    };

    /// <summary>
    /// The providers.
    /// </summary>
    private readonly IEnumerable<IProvider> providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationsController" /> class.
    /// </summary>
    /// <param name="providers">The providers.</param>
    public TranslationsController(IEnumerable<IProvider> providers) => this.providers = providers;

    /// <summary>
    /// GET: <c>/v1/Translations</c>.
    /// </summary>
    /// <returns>
    /// The list of available translations.
    /// </returns>
    [HttpGet]
    public async IAsyncEnumerable<Translation> Get()
    {
        foreach (IProvider provider in this.providers)
        {
            await foreach (Translation translation in provider.GetTranslationsAsync())
            {
                // Clean up any names we are displaying
                if (NameSubstitutions.TryGetValue(translation.Name, out string? translationName))
                {
                    translation.Name = translationName;
                }

                // Make sure this isn't a blocked translation
                if (!BlockedTranslations.Contains($"{translation.Provider}-{translation.Code}"))
                {
                    yield return translation;
                }
            }
        }
    }
}
