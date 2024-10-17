// -----------------------------------------------------------------------
// <copyright file="TranslationsController.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Controllers;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using GoToBible.Model;
using GoToBible.Client;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The translations controller.
/// </summary>
/// <seealso cref="ControllerBase" />
[ApiController]
[Route("v1/[controller]")]
[Route("[controller]")]
public class TranslationsController(IEnumerable<IProvider> providers) : ControllerBase
{
    /// <summary>
    /// The providers.
    /// </summary>
    private readonly IEnumerable<IProvider> providers = providers;

    /// <summary>
    /// GET: <c>/v1/Translations</c>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The list of available translations.
    /// </returns>
    [HttpGet]
    public async IAsyncEnumerable<Translation> Get(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (IProvider provider in this.providers)
        {
            await foreach (
                Translation translation in provider.GetTranslationsAsync(cancellationToken)
            )
            {
                // Clean up any names we are displaying
                if (
                    ApiProvider.NameSubstitutions.TryGetValue(
                        translation.Name,
                        out string? translationName
                    )
                )
                {
                    translation.Name = translationName;
                }

                // Make sure this isn't a blocked translation
                if (
                    !ApiProvider.BlockedTranslations.Contains(
                        $"{translation.Provider}-{translation.Code}"
                    )
                )
                {
                    yield return translation;
                }
            }
        }
    }
}
