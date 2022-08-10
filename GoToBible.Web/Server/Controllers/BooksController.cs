// -----------------------------------------------------------------------
// <copyright file="BooksController.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using GoToBible.Model;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The books controller.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Route("v1/[controller]")]
    public class BooksController : ControllerBase
    {
        /// <summary>
        /// The providers.
        /// </summary>
        private readonly IEnumerable<IProvider> providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="BooksController" /> class.
        /// </summary>
        /// <param name="providers">The providers.</param>
        public BooksController(IEnumerable<IProvider> providers) => this.providers = providers;

        /// <summary>
        /// GET: <c>/v1/Books?translation={translation_id}&amp;provider={provider_id}&amp;includeChapters={true_or_false}</c>.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="translation">The translation.</param>
        /// <param name="includeChapters">If set to <c>true</c>, include chapters. This can include significantly more data.</param>
        /// <returns>
        /// The list of the books in the translation.
        /// </returns>
        [HttpGet]
        public async IAsyncEnumerable<Book> Get(string provider, string translation, bool includeChapters)
        {
            IProvider? bookProvider = this.providers.SingleOrDefault(p => p.Id == provider);
            if (bookProvider is not null)
            {
                await foreach (Book book in bookProvider.GetBooksAsync(translation, includeChapters))
                {
                    yield return book;
                }
            }
        }
    }
}
