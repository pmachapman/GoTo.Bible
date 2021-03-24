// -----------------------------------------------------------------------
// <copyright file="TranslationsController.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Controllers
{
    using System.Collections.Generic;
    using GoToBible.Model;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The translations controller.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Route("[controller]")]
    public class TranslationsController : ControllerBase
    {
        /// <summary>
        /// The providers.
        /// </summary>
        private readonly IEnumerable<IProvider> providers;

        /// <summary>
        /// Initialises a new instance of the <see cref="TranslationsController" /> class.
        /// </summary>
        /// <param name="providers">The providers.</param>
        public TranslationsController(IEnumerable<IProvider> providers)
        {
            this.providers = providers;
        }

        /// <summary>
        /// GET: <c>/Translations</c>.
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
                    yield return translation;
                }
            }
        }
    }
}
