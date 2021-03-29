// -----------------------------------------------------------------------
// <copyright file="Laodiceans.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GoToBible.Model;

    /// <summary>
    /// Provides the Epistle to the Laodiceans.
    /// </summary>
    /// <seealso cref="IProvider" />
    public class Laodiceans : IProvider
    {
        /// <inheritdoc/>
        public string Id => nameof(Laodiceans);

        /// <inheritdoc/>
        public string Name => "Epistle to the Laodiceans";

        /// <inheritdoc/>
        public bool SupportsItalics => true;

        /// <inheritdoc/>
        public void Dispose() => GC.SuppressFinalize(this);

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<Book> GetBooksAsync(string translation, bool includeChapters)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (!includeChapters)
            {
                yield return new Book
                {
                    Name = "Laodiceans",
                };
            }
            else
            {
                yield return new Book
                {
                    Chapters = new List<ChapterReference>
                    {
                        new ChapterReference("Laodiceans", 1),
                    }
                    .AsReadOnly(),
                    Name = "Laodiceans",
                };
            }
        }

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Chapter> GetChapterAsync(string translation, string book, int chapterNumber)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Set up the chapter
            Chapter chapter = new Chapter
            {
                Book = book,
                ChapterNumber = chapterNumber,
                NextChapterReference = new ChapterReference(),
                PreviousChapterReference = new ChapterReference(),
                Translation = translation,
            };

            // Only allow one book at this stage
            if (book.ToLowerInvariant() == "laodiceans" && chapterNumber == 1)
            {
                if (translation == "LAO-ANG")
                {
                    chapter.Text = @"
1  Poul, apostle, not of men, ne by man, but bi Jhesu Crist, to the britheren that ben at Laodice,
2  grace to &#541;ou, and pees of God the fadir, and of the Lord Jhesu Crist.
3  I do thankyngis to my God bi al my preier, that &#541;e be dwelling and lastyng in him, abiding the biheest in the day of doom.
4  For neithir the veyn spekyng of summe vnwise men hath lettide &#541;ou, the whiche wolden turne &#541;ou fro the treuthe of the gospel, that is prechid of me.
5  And now hem that ben of me, to the profi&#541;t of truthe of the gospel, God schal make disseruyng, and doyng benygnyte of werkis, and helthe of euerlasting lijf.
6  And now my boondis ben open, which Y suffre in Crist Jhesu, in whiche Y glade and ioie.
7  And that is to me to euerlastyng helthe, that this same thing be doon by &#541;oure preiers, and mynystryng of the Holi Goost, either bi lijf, either bi deeth.
8  Forsothe to me it is lijf to lyue in Crist, and to die ioie.
9  And his mercy schal do in &#541;ou the same thing, that &#541;e moun haue the same loue, and that &#541;e be of oo will.
10  Therfore, &#541;e weel biloued britheren, holde &#541;e, and do &#541;e in the dreede of God, as &#541;e han herde the presence of me; and lijf schal be to &#541;ou withouten eende.
11  Sotheli it is God that worchith in &#541;ou.
12  And, my weel biloued britheren, do &#541;e without eny withdrawyng what euer thingis &#541;e don.
13  Joie &#541;e in Crist, and eschewe &#541;e men defoulid in lucre, [either foul wynnyng].
14  Be alle &#541;oure askyngis open anentis God, and be &#541;e stidefast in the witt of Crist.
15  And do &#541;e tho thingis that ben hool,
16  and trewe, and chaast, and iust, and able to be loued; and kepe &#541;e in herte tho thingis that &#541;e haue herd and take; and pees schal be to &#541;ou.
18  Alle holi men greten &#541;ou weel.
19  The grace of oure Lord Jhesu Crist be with &#541;oure spirit.
20  And do &#541;e that pistil of Colocensis to be red to you.
";
                }
                else if (translation == "LAO-ENG")
                {
                    chapter.Text = @"
1  Paul, an apostle not from men nor through man, but through Jesus Christ, to the brethren who are of Laodicea. 
2  Grace to you and peace from God the Father and the Lord Jesus Christ. 
3  I give thanks to Christ through all my prayers, that you are continuing in him and persevering in his works, looking forward to the promise [of salvation] in the day of judgment. 
4  Neither does the vain prattling of some intruders beguile you, that they may divert you from the truth of the Gospel which is preached by me. 
5  And now God will cause that those who are [disciples] of mine will continue serving, to the increase of the truth of the Gospel, and performing goodness and the work of salvation of eternal life. 
6  And now well known are my bonds which I suffer in Christ, in which I rejoice and am glad. 
7  And this to me is for everlasting salvation, which also is wrought by your prayers, and by the superintendance of the Holy Spirit, whether through life or through death. 
8  For to me, to live is to be in Christ, and to die is joy. 
9  And likewise he will work his mercy in you that you may have the same love, and may be of one mind. 
10  Therefore, dearly beloved, as you have heard in my presence, so hold fast and work in the fear of God, and it will be life eternal for you. 
11  For it is God who works in you. 
12  And do without retreating whatever you do. 
13  And for the rest, dearly beloved, rejoice in Christ, and beware of those who are sordid in wordly gain. 
14  Let all your petitions be made openly before God, and be firm in the thinking of Christ. 
15  And do those things that are sound, and true, and sober, and just, and amiable. 
16  And what you have heard and received, retain in your heart. And peace shall be with you. 
18  The saints salute you. 
19  The grace of the Lord Jesus be with your spirit. 
20  And cause [this letter] to be read to the Colossians and that of the Colossians to you. 
";
                }
                else if (translation == "LAO-GRK")
                {
                    chapter.Text = @"
1  ΠΑΥΛΟς ἀπόστολος οὐκ ἀπ̓ ἀνθρώπων οὐδὲ δἰ ἀνθρώπου ἀλλὰ διὰ Ἰησοῦ Χριστοῦ, τοῖς ἀδελφοῖς τοῖς οὖσιν ἐν Λαοδικείᾳ. 
2  Χάρις ὑμῖν καὶ εἰρήνη ἀπὸ Θεοῦ πατρὸς καὶ Κυρίου Ἰησοῦ Χριστοῦ. 
3  Εὐχαριστῶ τῷ Χριστῷ ἐν πάσῃ δεήσει μου, ὅτι ἐστὲ εν αὐτῷ μένοντες καὶ προσκαρτεροῦντες τοῖς ἔργοις αὐτοῦ, ἀπεκδεχόμενοι τὴν ἐπαγγελίαν εἰς ἡμέραν κρίσεως. 
4  Μηδὲ ὑμᾶς ἐξαπατήσωσιν ματαιολογίαι τινῶν διδασκόντων ἵνα ἀποστρέψωσιν ὑμᾶς ἀπὸ τῆς ἀληθείας τοῦ εὐαγγελίου τοῦ εὐαγγελισθέντος ὑπ̓ ἐμοῦ. 
5  καὶ νῦν ποιήσει ὁ Θεὸς ἵνα τὰ ἐξ ἐμοῦ εἰς προκοπὴν τῆς ἀληθείας τοῦ εὐαγγελίου... λατρεύοντες καὶ ποιοῦντες χρηστότητα ἔργων τῶν τῆς σωτηρίας [καὶ] τῆς αἰωνίου ζωῆς. 
6  καὶ νῦν φανεροὶ οἱ δεσμοί μου, οὓς ὑπομένω ἐν Χριστῷ, ἐν οἷς χαίρω καὶ ἀγαλλιῶμαι. 
7  καὶ τοῦτό ἐστίν μοι εἰς σωτηρίαν ἀΐδιον, ὃ καὶ ἀπέβη διὰ τῆς ὑμῶν δεήσεως καὶ ἐπιχορηγίας πνεύματος ἁγίου, εἴτε διὰ ζωῆς εἴτε διὰ θανάτου. 
8  ἐμοὶ γὰρ τὸ ζῆν ἐν Χριστῷ καὶ τὸ ἀποθανεῖν χαρά. 
9  καὶ τὸ αὐτὸ ποιήσει [καὶ] ἐν ὑμῖν διὰ τοῦ ἐλέους αὐτοῦ, ἵνα τὴν αὐτὴν ἀγάπην ἔχητε, σύμψυχοι ὄντες. 
10  ὥστε, ἀγαπητοί, καθὼς ὑπηκούσατε ἐν τῇ παρουσίᾳ μου, οὕτως μνημονεύοντες μετὰ φόβου Κυρίου ἐργάζεσθε, καὶ ἔσται ὑμῖν ζωὴ εἰς τὸν αἰῶνα· 
11  Θεὸς γάρ ἐστιν ὁ ἐνεργῶν ἐν ὑμῖν. 
12  καὶ ποιεῖτε χωρὶς διαλογισμῶν ὅ τι ἐὰν ποιῆτε. 
13  Καὶ τὸ λοιπόν, ἀγαπητοί, χαίρετε ἐν Χριστῷ. βλέπετε δὲ τοὺς αἰσχροκερδεῖς. 
14  πάντα τὰ αἰτήματα ὑμῶν γνωριζέσθω πρὸς τὸν Θεόν. καὶ ἑδραῖοι γίνεσθε ἐν τῷ νοῒ τοῦ Χριστοῦ. 
15  ὅσα τε ὁλόκληρα καὶ ἀληθῆ καὶ σεμνὰ καὶ δίκαια καὶ προσφιλῆ, ταῦτα πράσσετε. 
16  ἇ καὶ ἠκούσατε καὶ παρελάβετε, ἐν τῇ καρδίᾳ κρατεῖτε, καὶ ἡ εἰρήνη ἔσται μεθ̓ ὑμῶν. 
18  Ἀσπάζονται ὑμᾶς οἱ ἅγιοι. 
19  Ἡ χάρις τοῦ Κυρίου Ἰησοῦ Χριστοῦ μετὰ τοῦ πνεύματος ὑμῶν. 
20  καὶ ποιήσατε ἵνα τοῖς Κολασσαεῦσιν ἀναγνωσθῇ, καὶ ἡ τῶν Κολασσαέων ἵνα καὶ ὑμῖν. 
";
                }
                else if (translation == "LAO-LAT")
                {
                    chapter.Text = @"
1  Paulus Apostolus non ab hominibus neque per hominem sed per Ihesum Christum, fratribus qui sunt Laodiciae. 
2  Gratia vobis et pax a Deo patre et Domino Ihesu Christo. 
3  Gratias ago Christo per omnem orationem meam, quod permanentes estis in eo et perseverantes in operibus eius, promissum expectantes in diem iudicii. 
4  Neque destituant vos quorundam vaniloquia insinuantium, ut vos avertant a veritate evangelii quod a me praedicatur. 
5  Et nunc faciet Deus ut qui sunt ex me ad profectum veritatis evangelii deservientes et facientes benignitatem operum que salutis vitae aeternae. 
6  Et nunc palam sunt vincula mea quae patior in Christo; quibus laetor et gaudeo. 
7  Et hoc mihi est ad salutem perpetuam; quod ipsum factum orationibus vestris et administrante Spiritu sancto, sive per vitam sive per mortem. 
8  Est enim mihi vivere in Christo et mori gaudium. 
9  Et id ipsum in vobis faciet misericordia sua, ut eandem dilectionem habeatis et sitis unianimes. 
10  Ergo, dilectissimi, ut audistis praesentia mei, ita retinete et facite in timore Dei, et erit vobis vita in aeternum: 
11  Est enim Deus qui operatur in vos. 
12  Et facite sine retractu quaecumque facitis. 
13  Et quod est [reliquum], dilectissimi, gaudete in Christo; et praecavete sordidos in lucro. 
14  Omnes sint petitiones vestrae palam apud Deum; et estote firmi in sensu Christi. 
15  Et quae integra et vera et pudica et iusta et amabilia, facite. 
16  Et quae audistis et accepistis in corde retinete; et erit vobis pax. 
18  Salutant vos sancti. 
19  Gratia Domini Ihesu cum spiritu vestro. 
20  Et facite legi Colosensibus et Colosensium vobis. 
";
                }
            }

            return chapter;
        }

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<Translation> GetTranslationsAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield return new Translation
            {
                Author = "John Wycliffe",
                CanBeExported = true,
                Code = "LAO-ANG",
                Copyright = "Public Domain",
                Language = "Old English",
                Name = "Epistle to the Laodiceans (Old English)",
                Provider = this.Id,
                Year = 2021,
            };
            yield return new Translation
            {
                Author = "Peter Chapman",
                CanBeExported = true,
                Code = "LAO-ENG",
                Copyright = "Public Domain",
                Language = "English",
                Name = "Epistle to the Laodiceans (English)",
                Provider = this.Id,
                Year = 2021,
            };
            yield return new Translation
            {
                Author = "Joseph Barber Lightfoot",
                CanBeExported = true,
                Code = "LAO-GRK",
                Copyright = "Public Domain",
                Language = "Greek",
                Name = "Epistle to the Laodiceans (Greek)",
                Provider = this.Id,
                Year = 2021,
            };
            yield return new Translation
            {
                Author = "Pseudepigraphical",
                CanBeExported = true,
                Code = "LAO-LAT",
                Copyright = "Public Domain",
                Language = "Latin",
                Name = "Epistle to the Laodiceans (Latin)",
                Provider = this.Id,
                Year = 2021,
            };
        }
    }
}
