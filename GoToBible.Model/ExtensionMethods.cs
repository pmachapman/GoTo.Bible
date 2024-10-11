// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Extension Methods to the Model.
/// </summary>
public static partial class ExtensionMethods
{
#pragma warning disable format
    /// <summary>
    /// The lengths of each chapter in each book.
    /// </summary>
    private static readonly OrderedDictionary BookLengths = new OrderedDictionary
    {
        ["genesis"] = new[] { 31, 25, 24, 26, 32, 22, 24, 22, 29, 32, 32, 20, 18, 24, 21, 16, 27, 33, 38, 18, 34, 24, 20, 67, 34, 35, 46, 22, 35, 43, 55, 32, 20, 31, 29, 43, 36, 30, 23, 23, 57, 38, 34, 34, 28, 34, 31, 22, 33, 26 },
        ["exodus"] = new[] { 22, 25, 22, 31, 23, 30, 25, 32, 35, 29, 10, 51, 22, 31, 27, 36, 16, 27, 25, 26, 36, 31, 33, 18, 40, 37, 21, 43, 46, 38, 18, 35, 23, 35, 35, 38, 29, 31, 43, 38 },
        ["leviticus"] = new[] { 17, 16, 17, 35, 19, 30, 38, 36, 24, 20, 47, 8, 59, 57, 33, 34, 16, 30, 37, 27, 24, 33, 44, 23, 55, 46, 34 },
        ["numbers"] = new[] { 54, 34, 51, 49, 31, 27, 89, 26, 23, 36, 35, 16, 33, 45, 41, 50, 13, 32, 22, 29, 35, 41, 30, 25, 18, 65, 23, 31, 40, 16, 54, 42, 56, 29, 34, 13 },
        ["deuteronomy"] = new[] { 46, 37, 29, 49, 33, 25, 26, 20, 29, 22, 32, 32, 18, 29, 23, 22, 20, 22, 21, 20, 23, 30, 25, 22, 19, 19, 26, 68, 29, 20, 30, 52, 29, 12 },
        ["joshua"] = new[] { 18, 24, 17, 24, 15, 27, 26, 35, 27, 43, 23, 24, 33, 15, 63, 10, 18, 28, 51, 9, 45, 34, 16, 33 },
        ["judges"] = new[] { 36, 23, 31, 24, 31, 40, 25, 35, 57, 18, 40, 15, 25, 20, 20, 31, 13, 31, 30, 48, 25 },
        ["ruth"] = new[] { 22, 23, 18, 22 },
        ["1 samuel"] = new[] { 28, 36, 21, 22, 12, 21, 17, 22, 27, 27, 15, 25, 23, 52, 35, 23, 58, 30, 24, 42, 15, 23, 29, 22, 44, 25, 12, 25, 11, 31, 13 },
        ["2 samuel"] = new[] { 27, 32, 39, 12, 25, 23, 29, 18, 13, 19, 27, 31, 39, 33, 37, 23, 29, 33, 43, 26, 22, 51, 39, 25 },
        ["1 kings"] = new[] { 53, 46, 28, 34, 18, 38, 51, 66, 28, 29, 43, 33, 34, 31, 34, 34, 24, 46, 21, 43, 29, 53 },
        ["2 kings"] = new[] { 18, 25, 27, 44, 27, 33, 20, 29, 37, 36, 21, 21, 25, 29, 38, 20, 41, 37, 37, 21, 26, 20, 37, 20, 30 },
        ["1 chronicles"] = new[] { 54, 55, 24, 43, 26, 81, 40, 40, 44, 14, 47, 40, 14, 17, 29, 43, 27, 17, 19, 8, 30, 19, 32, 31, 31, 32, 34, 21, 30 },
        ["2 chronicles"] = new[] { 17, 18, 17, 22, 14, 42, 22, 18, 31, 19, 23, 16, 22, 15, 19, 14, 19, 34, 11, 37, 20, 12, 21, 27, 28, 23, 9, 27, 36, 27, 21, 33, 25, 33, 27, 23 },
        ["ezra"] = new[] { 11, 70, 13, 24, 17, 22, 28, 36, 15, 44 },
        ["nehemiah"] = new[] { 11, 20, 32, 23, 19, 19, 73, 18, 38, 39, 36, 47, 31 },
        ["esther"] = new[] { 22, 23, 15, 17, 14, 14, 10, 17, 32, 3, 17, 8, 30, 16, 24, 10 },
        ["job"] = new[] { 22, 13, 26, 21, 27, 30, 21, 22, 35, 22, 20, 25, 28, 22, 35, 22, 16, 21, 29, 29, 34, 30, 17, 25, 6, 14, 23, 28, 25, 31, 40, 22, 33, 37, 16, 33, 24, 41, 30, 24, 34, 17 },
        ["psalm"] = new[] { 6, 12, 8, 8, 12, 10, 17, 9, 20, 18, 7, 8, 6, 7, 5, 11, 15, 50, 14, 9, 13, 31, 6, 10, 22, 12, 14, 9, 11, 12, 24, 11, 22, 22, 28, 12, 40, 22, 13, 17, 13, 11, 5, 26, 17, 11, 9, 14, 20, 23, 19, 9, 6, 7, 23, 13, 11, 11, 17, 12, 8, 12, 11, 10, 13, 20, 7, 35, 36, 5, 24, 20, 28, 23, 10, 12, 20, 72, 13, 19, 16, 8, 18, 12, 13, 17, 7, 18, 52, 17, 16, 15, 5, 23, 11, 13, 12, 9, 9, 5, 8, 28, 22, 35, 45, 48, 43, 13, 31, 7, 10, 10, 9, 8, 18, 19, 2, 29, 176, 7, 8, 9, 4, 8, 5, 6, 5, 6, 8, 8, 3, 18, 3, 3, 21, 26, 9, 8, 24, 13, 10, 7, 12, 15, 21, 10, 20, 14, 9, 6, 7 },
        ["proverbs"] = new[] { 33, 22, 35, 27, 23, 35, 27, 36, 18, 32, 31, 28, 25, 35, 33, 33, 28, 24, 29, 30, 31, 29, 35, 34, 28, 28, 27, 28, 27, 33, 31 },
        ["ecclesiastes"] = new[] { 18, 26, 22, 16, 20, 12, 29, 17, 18, 20, 10, 14 },
        ["song of solomon"] = new[] { 17, 17, 11, 16, 16, 13, 13, 14 },
        ["isaiah"] = new[] { 31, 22, 26, 6, 30, 13, 25, 22, 21, 34, 16, 6, 22, 32, 9, 14, 14, 7, 25, 6, 17, 25, 18, 23, 12, 21, 13, 29, 24, 33, 9, 20, 24, 17, 10, 22, 38, 22, 8, 31, 29, 25, 28, 28, 25, 13, 15, 22, 26, 11, 23, 15, 12, 17, 13, 12, 21, 14, 21, 22, 11, 12, 19, 12, 25, 24 },
        ["jeremiah"] = new[] { 19, 37, 25, 31, 31, 30, 34, 22, 26, 25, 23, 17, 27, 22, 21, 21, 27, 23, 15, 18, 14, 30, 40, 10, 38, 24, 22, 17, 32, 24, 40, 44, 26, 22, 19, 32, 21, 28, 18, 16, 18, 22, 13, 30, 5, 28, 7, 47, 39, 46, 64, 34 },
        ["lamentations"] = new[] { 22, 22, 66, 22, 22 },
        ["ezekiel"] = new[] { 28, 10, 27, 17, 17, 14, 27, 18, 11, 22, 25, 28, 23, 23, 8, 63, 24, 32, 14, 49, 32, 31, 49, 27, 17, 21, 36, 26, 21, 26, 18, 32, 33, 31, 15, 38, 28, 23, 29, 49, 26, 20, 27, 31, 25, 24, 23, 35 },
        ["daniel"] = new[] { 21, 49, 30, 37, 31, 28, 28, 27, 27, 21, 45, 13, 64, 42 },
        ["hosea"] = new[] { 11, 23, 5, 19, 15, 11, 16, 14, 17, 15, 12, 14, 16, 9 },
        ["joel"] = new[] { 20, 32, 21 },
        ["amos"] = new[] { 15, 16, 15, 13, 27, 14, 17, 14, 15 },
        ["obadiah"] = new[] { 21 },
        ["jonah"] = new[] { 17, 10, 10, 11 },
        ["micah"] = new[] { 16, 13, 12, 13, 15, 16, 20 },
        ["nahum"] = new[] { 15, 13, 19 },
        ["habakkuk"] = new[] { 17, 20, 19 },
        ["zephaniah"] = new[] { 18, 15, 20 },
        ["haggai"] = new[] { 15, 23 },
        ["zechariah"] = new[] { 21, 13, 10, 14, 11, 15, 14, 23, 17, 12, 17, 14, 9, 21 },
        ["malachi"] = new[] { 14, 17, 18, 6 },
        ["matthew"] = new[] { 25, 23, 17, 25, 48, 34, 29, 34, 38, 42, 30, 50, 58, 36, 39, 28, 27, 35, 30, 34, 46, 46, 39, 51, 46, 75, 66, 20 },
        ["mark"] = new[] { 45, 28, 35, 41, 43, 56, 37, 38, 50, 52, 33, 44, 37, 72, 47, 20 },
        ["luke"] = new[] { 80, 52, 38, 44, 39, 49, 50, 56, 62, 42, 54, 59, 35, 35, 32, 31, 37, 43, 48, 47, 38, 71, 56, 53 },
        ["john"] = new[] { 51, 25, 36, 54, 47, 71, 53, 59, 41, 42, 57, 50, 38, 31, 27, 33, 26, 40, 42, 31, 25 },
        ["acts"] = new[] { 26, 47, 26, 37, 42, 15, 60, 40, 43, 48, 30, 25, 52, 28, 41, 40, 34, 28, 41, 38, 40, 30, 35, 27, 27, 32, 44, 31 },
        ["romans"] = new[] { 32, 29, 31, 25, 21, 23, 25, 39, 33, 21, 36, 21, 14, 23, 33, 27 },
        ["1 corinthians"] = new[] { 31, 16, 23, 21, 13, 20, 40, 13, 27, 33, 34, 31, 13, 40, 58, 24 },
        ["2 corinthians"] = new[] { 24, 17, 18, 18, 21, 18, 16, 24, 15, 18, 33, 21, 14 },
        ["galatians"] = new[] { 24, 21, 29, 31, 26, 18 },
        ["ephesians"] = new[] { 23, 22, 21, 32, 33, 24 },
        ["philippians"] = new[] { 30, 30, 21, 23 },
        ["colossians"] = new[] { 29, 23, 25, 18 },
        ["1 thessalonians"] = new[] { 10, 20, 13, 18, 28 },
        ["2 thessalonians"] = new[] { 12, 17, 18 },
        ["1 timothy"] = new[] { 20, 15, 16, 16, 25, 21 },
        ["2 timothy"] = new[] { 18, 26, 17, 22 },
        ["titus"] = new[] { 16, 15, 15 },
        ["philemon"] = new[] { 25 },
        ["hebrews"] = new[] { 14, 18, 19, 16, 14, 20, 28, 13, 28, 39, 40, 29, 25 },
        ["james"] = new[] { 27, 26, 18, 17, 20 },
        ["1 peter"] = new[] { 25, 25, 22, 19, 14 },
        ["2 peter"] = new[] { 21, 22, 18 },
        ["1 john"] = new[] { 10, 29, 24, 21, 21 },
        ["2 john"] = new[] { 13 },
        ["3 john"] = new[] { 15 },
        ["jude"] = new[] { 25 },
        ["revelation"] = new[] { 20, 29, 22, 11, 14, 17, 17, 13, 21, 11, 19, 17, 18, 20, 8, 21, 18, 24, 21, 15, 27, 21 },
        ["1 esdras"] = new[] { 58, 30, 24, 63, 73, 34, 15, 96, 55 },
        ["2 esdras"] = new[] { 40, 48, 36, 52, 56, 59, 70, 63, 47, 59, 46, 51, 58, 48, 63, 78 },
        ["tobit"] = new[] { 22, 14, 17, 21, 22, 18, 17, 21, 6, 14, 18, 22, 18, 15 },
        ["judith"] = new[] { 16, 28, 10, 15, 24, 21, 32, 36, 14, 23, 23, 20, 20, 19, 14, 25 },
        ["esther (greek)"] = new[] { 22, 23, 15, 17, 14, 14, 10, 17, 32, 3, 1 },
        ["wisdom"] = new[] { 16, 24, 19, 20, 23, 25, 30, 21, 18, 21, 26, 27, 19, 31, 19, 29, 21, 25, 22 },
        ["ecclesiasticus"] = new[] { 29, 18, 30, 31, 17, 37, 36, 19, 18, 30, 34, 18, 25, 27, 20, 28, 27, 33, 26, 30, 28, 27, 27, 31, 25, 20, 30, 26, 28, 25, 31, 24, 33, 26, 24, 27, 30, 34, 35, 30, 24, 25, 35, 23, 26, 20, 25, 25, 16, 29, 30 },
        ["baruch"] = new[] { 22, 35, 38, 37, 9, 73 },
        ["epistle of jeremy"] = new[] { 73 },
        ["daniel (greek)"] = new[] { 21, 49, 30, 37, 31, 28, 28, 27, 27, 21, 45, 13, 64, 42 },
        ["song of the three"] = new[] { 45 },
        ["susanna"] = new[] { 64 },
        ["bel and the dragon"] = new[] { 42 },
        ["manasseh"] = new[] { 15 },
        ["1 maccabees"] = new[] { 63, 70, 59, 61, 68, 63, 50, 32, 73, 89, 74, 53, 53, 49, 41, 24 },
        ["2 maccabees"] = new[] { 36, 32, 40, 50, 27, 31, 42, 36, 29, 38, 38, 46, 26, 46, 39 },
        ["3 maccabees"] = new[] { 29, 33, 30, 21, 51, 41, 23 },
        ["4 maccabees"] = new[] { 35, 24, 21, 26, 38, 35, 23, 29, 32, 21, 27, 19, 27, 20, 32, 25, 24, 24 },
        ["laodiceans"] = new[] { 20 },
    };

    /// <summary>
    /// The book name map.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string[]> BookNameMap = new Dictionary<string, string[]>
    {
        ["genesis"] = ["ge", "gn", "gen", "genesis"],
        ["exodus"] = ["ex", "exo", "exd", "exod", "exodus"],
        ["leviticus"] = ["leviticus", "levi", "le", "lv", "lev"],
        ["numbers"] = ["nb", "num", "nm", "numbers", "nu", "numb", "number"],
        ["deuteronomy"] = ["de", "deut", "dueteronomy", "du", "deu", "dt", "deuteronomy"],
        ["joshua"] = ["josh", "js", "jos", "joshua", "jsh"],
        ["judges"] = ["juges", "jdg", "jg", "jgs", "jdgs", "judgs", "judg", "judges"],
        ["ruth"] = ["rt", "rth", "ru", "ruth", "rh"],
        ["1 samuel"] = ["1saml", "firstsam", "isamuel", "1samuel", "ism", "1sm", "firsts", "isam", "sa", "1stsam", "1stsamuel", "firstsamuel", "fstsam", "fstsaml", "firstsm", "frstsam", "1sa", "1stsm", "sam", "1sam", "1stsaml", "frstsa", "saml", "frstsamuel", "firstsa", "1sts", "1s", "1stsa", "firstsaml", "samuel", "isaml", "fstsa", "frstsm", "fsts", "frstsaml", "fstsm", "frsts", "sm", "fstsamuel"],
        ["2 samuel"] = ["scsamuel", "secs", "secsam", "iis", "scndsam", "secondsm", "secondsaml", "seconds", "2ndsaml", "2nds", "2samuel", "sesm", "scndsamuel", "2ndsamuel", "iism", "secsm", "secsa", "2sm", "scsam", "iisam", "scsaml", "scndsa", "scs", "2ndsam", "secondsam", "iisaml", "2ndsm", "2saml", "scnds", "2sam", "secsamuel", "secsaml", "scndsaml", "2sa", "sesaml", "sesa", "secondsamuel", "scsm", "iisa", "scsa", "ses", "iisamuel", "sesamuel", "2ndsa", "2s", "scndsm", "sesam", "secondsa"],
        ["1 kings"] = ["fstking", "1stks", "kg", "1stking", "frstk", "kgs", "1kgs", "frstkgs", "1stkings", "fstkg", "firstkn", "1kings", "fstk", "1stkgs", "1kg", "1stkn", "iki", "firstkgs", "1king", "frstkings", "iking", "frstks", "ikings", "1ki", "king", "kn", "frstkg", "firstk", "kings", "ikgs", "1k", "fstki", "1stkg", "fstkn", "ki", "ikn", "firstks", "k", "1stki", "fstkings", "1kn", "firstkg", "frstki", "firstki", "fstkgs", "frstkn", "fstks", "1ks", "firstking", "firstkings", "ikg", "iks", "ik", "ks", "1stk", "frstking"],
        ["2 kings"] = ["iikg", "iikn", "seks", "scndkings", "2ks", "scndki", "secki", "scndk", "seki", "secondks", "scndkg", "secondki", "2ndk", "sckgs", "secondking", "seckings", "scks", "scking", "iiki", "2ki", "seckg", "2ndkg", "secks", "sek", "2ndkings", "iikings", "2kg", "seck", "secondkg", "secondk", "scndkn", "scndks", "iiking", "2ndkgs", "2ndking", "secondkgs", "iik", "sekgs", "secondkings", "sekg", "sckings", "secking", "iiks", "2kings", "secondkn", "sekings", "scndking", "seking", "scki", "2ndks", "2kgs", "sckg", "2king", "iikgs", "sekn", "sckn", "2kn", "2k", "seckgs", "scndkgs", "2ndki", "seckn", "2ndkn", "sck"],
        ["1 chronicles"] = ["firstch", "ichrs", "frstchronicles", "firstchro", "chro", "firstchronicle", "1chr", "firstchron", "1chronicl", "fstchs", "1chro", "fstchronic", "frstchro", "1chronicles", "1ch", "fstchrs", "ichron", "frstchronicle", "ichronicl", "1stchronicle", "firstchrs", "fstchronicl", "1stchro", "1stchroni", "1stchr", "ich", "1stchs", "1chronicle", "ichronicle", "ichro", "fstchr", "chr", "firstchr", "fstchronicle", "chronicl", "frstchronic", "chron", "1chs", "firstchroni", "1stchronicles", "fstchron", "fstchronicles", "ichronic", "firstchronicles", "1chronic", "frstchron", "firstchronicl", "1stchrs", "chrs", "ichroni", "fstchroni", "chronic", "frstchrs", "firstchs", "1stchronic", "frstchr", "chs", "fstch", "frstch", "1stchron", "fstchro", "ichr", "firstchronic", "frstchs", "1chroni", "frstchroni", "1stch", "frstchronicl", "1chrs", "ichronicles", "chronicle", "1stchronicl", "1chron", "chronicles", "chroni", "ichs"],
        ["2 chronicles"] = ["iich", "scchronicles", "scndchronicl", "iichronicl", "2chro", "2chronicles", "secchs", "iichrs", "sechr", "2chronicle", "iichro", "sechron", "scndchro", "2chr", "secondchron", "sech", "sechronicle", "scchronic", "secch", "secondchronicle", "scchs", "scchronicl", "scndchroni", "scndchron", "2chron", "secchr", "2chs", "2ndchron", "scchrs", "scndchr", "secchronic", "secchron", "scndchrs", "sechronic", "2ndchronicl", "sechrs", "sechronicles", "scchronicle", "secondchro", "scndchronic", "sechro", "secondch", "secchronicl", "2ndchronic", "2ndchronicle", "scndchronicle", "2ndchronicles", "secondchronicl", "sechronicl", "2chronicl", "scchr", "secondchrs", "iichron", "scchroni", "2ndchro", "secondchs", "iichronic", "scchron", "sechs", "iichr", "scch", "2chronic", "secchronicle", "secchronicles", "secondchr", "iichronicles", "2chrs", "scndch", "secondchroni", "secondchronicles", "2ndch", "secchroni", "secchro", "2ch", "2ndchr", "iichs", "2chroni", "scchro", "iichroni", "secondchronic", "secchrs", "scndchs", "2ndchs", "2ndchroni", "scndchronicles", "iichronicle", "sechroni", "2ndchrs"],
        ["ezra"] = ["ezra", "ezr", "1ezra", "1ezr"],
        ["nehemiah"] = ["ne", "nehemiah", "neh", "2ezra", "2ezr"],
        ["esther"] = ["esth", "est", "es", "esther"],
        ["job"] = ["job", "jb"],
        ["psalm"] = ["psalms", "psm", "psalm", "pslm", "pss", "ps", "psa"],
        ["proverbs"] = ["proverbs", "proverb", "pro", "pr", "prv", "prov", "prvbs"],
        ["ecclesiastes"] = ["ecclesiaste", "ecll", "q", "ecl", "eccl", "qoh", "ecclesiastes", "eclesiastes", "ecc", "ec", "eccles", "qoheleth", "qo"],
        ["song of solomon"] = ["song of solomon", "songofsolomon", "song of sol", "songofsol", "the song of solomon", "thesongofsolomon", "the song of sol", "thesongofsol", "ss", "canticle of canticles", "conticleofcanticles", "canticles", "song of songs", "songofsongs", "song of", "songof", "solomon", "song", "sng", "sos", "so", "sg"],
        ["isaiah"] = ["isah", "isai", "isaiah", "isa"],
        ["jeremiah"] = ["je", "jer", "jere", "jr", "jeremaih", "jeremiah"],
        ["lamentations"] = ["lamentations", "la", "lam", "lamentation", "lm"],
        ["ezekiel"] = ["ezk", "ezekiel", "ez", "ezek", "eze"],
        ["daniel"] = ["dn", "dan", "da", "daniel"],
        ["hosea"] = ["hos", "hs", "ho", "hosea"],
        ["joel"] = ["joe", "joel", "jo", "jl"],
        ["amos"] = ["am", "ams", "amos", "amo"],
        ["obadiah"] = ["obadaiah", "ob", "obad", "obadiah", "obd", "oba", "obadaih"],
        ["jonah"] = ["jonah", "jnh", "jon"],
        ["micah"] = ["mica", "mic", "mi", "micah"],
        ["nahum"] = ["nah", "na", "nahum"],
        ["habakkuk"] = ["hbk", "habbakkuk", "hab", "habakuk", "habbakuk", "hk", "habakkuk"],
        ["zephaniah"] = ["zp", "zep", "zephaniah", "zeph"],
        ["haggai"] = ["hagai", "haggai", "haggia", "hagg", "hag", "hg"],
        ["zechariah"] = ["zc", "zachariah", "zechariah", "zecharaiah", "zech", "zec"],
        ["malachi"] = ["mal", "ml", "malachi", "ma"],
        ["matthew"] = ["matthew", "matt", "mt", "mat", "mathew"],
        ["mark"] = ["mk", "mar", "mark", "mr", "mrk"],
        ["luke"] = ["lk", "lu", "luke", "luk"],
        ["john"] = ["jn", "joh", "john", "j", "jhn"],
        ["acts"] = ["act", "ac", "acts"],
        ["romans"] = ["rm", "roman", "romans", "ro", "rom"],
        ["1 corinthians"] = ["1stcorinthian", "frstcorinth", "icortn", "frstcortn", "fstcori", "fstcorinth", "frstcorinthians", "icorin", "frstco", "corinthi", "icorinthians", "icorinthian", "frstcorinthi", "1stcori", "corin", "frstcorinthian", "firstcorinthian", "corinthians", "1stcor", "firstcorinth", "fstcorinthi", "cor", "fstcor", "corinth", "firstcorin", "fstcorinthians", "firstco", "1stcorinthians", "1corth", "1cortn", "corinthian", "corth", "firstcori", "ico", "icorinthi", "frstcorin", "cori", "icori", "firstcorth", "fstcortn", "co", "1cor", "firstcor", "1stcorinth", "fstcorin", "frstcorth", "icorth", "fstco", "1stcortn", "1stcorin", "1cori", "fstcorth", "icorinth", "firstcorinthi", "icor", "fstcorinthian", "1stcorth", "firstcorinthians", "1corinth", "frstcor", "firstcortn", "cortn", "1stcorinthi", "1corinthians", "1corinthi", "1stco", "1corin", "frstcori", "1co", "1corinthian"],
        ["2 corinthians"] = ["2corinth", "seccorinthian", "secondcorinthi", "secondcorinthians", "sccorinthians", "sccorinth", "2corinthi", "secorinth", "2corin", "2cor", "scndcorth", "scndco", "secondcor", "scndcorinthi", "scndcorin", "seccorin", "iicori", "secondcorinth", "iicorinth", "2ndcorinth", "sccorth", "2co", "sccori", "secor", "secco", "secondco", "iicor", "secori", "2ndcorin", "secortn", "scndcori", "secondcorin", "iicorth", "2cortn", "iicorinthian", "2ndcorinthi", "seccorinthians", "2ndco", "secondcori", "secorinthi", "iicorin", "2corth", "scndcorinthians", "sccor", "2ndcorinthians", "2ndcortn", "seccorinth", "2ndcorinthian", "iicorinthians", "seccortn", "2ndcor", "seccorinthi", "2ndcori", "seco", "secondcortn", "scndcortn", "2corinthian", "secondcorinthian", "scndcor", "secorinthian", "sccorin", "iico", "sccorinthian", "2corinthians", "secorth", "secorin", "scndcorinthian", "2cori", "seccor", "2ndcorth", "scndcorinth", "iicorinthi", "iicortn", "secondcorth", "sccortn", "scco", "seccori", "sccorinthi", "secorinthians", "seccorth"],
        ["galatians"] = ["galat", "galatians", "ga", "galatian", "gal", "galations", "gl"],
        ["ephesians"] = ["ephe", "ephesians", "eph", "ephs", "ephesian", "ephes", "ep"],
        ["philippians"] = ["ph", "philipians", "php", "phi", "phillippians", "philippians", "phl", "pp", "philip", "phil", "phillipians", "philipp"],
        ["colossians"] = ["cl", "co", "colossian", "col", "colosians", "cs", "colossians"],
        ["1 thessalonians"] = ["1stthessalonions", "1stth", "firstthessalonians", "thessalonians", "1thessalonions", "firstthessalonions", "firstthessal", "frstthes", "thessalonions", "frstthess", "firstth", "frstthessal", "fstthessalonians", "1thessalonians", "ithessalonions", "fstthes", "fstthessalonions", "frstth", "1stthess", "thessal", "1stthessal", "ithes", "1thessal", "1thess", "1thes", "fstth", "ith", "frstthessalonions", "ithessalonians", "th", "frstthessalonians", "1stthessalonians", "thes", "fstthessal", "ithessal", "firstthes", "1stthes", "firstthess", "ithess", "1th", "fstthess", "thess"],
        ["2 thessalonians"] = ["scthess", "scthes", "iith", "secthess", "sethessal", "2thes", "scndth", "secondthessalonions", "secondthess", "secondthessal", "secondthes", "iithessalonions", "secthessalonians", "secondth", "sethessalonians", "2thess", "scndthessalonions", "iithessal", "scthessal", "seth", "2ndthess", "2thessalonians", "secthessalonions", "scth", "2ndthessalonians", "scthessalonians", "scndthessalonians", "iithessalonians", "scndthessal", "iithes", "2thessal", "2ndth", "secthes", "scndthes", "sethess", "2ndthessalonions", "2ndthessal", "2ndthes", "2thessalonions", "secondthessalonians", "secth", "secthessal", "scthessalonions", "iithess", "sethes", "sethessalonions", "scndthess", "2th"],
        ["1 timothy"] = ["tim", "1sttimothy", "firsttimoth", "itimoth", "itimothy", "iti", "fsttimothy", "frstti", "fsttm", "tm", "1sttimoth", "timoth", "1stti", "itim", "1ti", "frsttimothy", "fsttimoth", "firstti", "1tim", "fsttim", "1timothy", "itm", "timothy", "firsttimothy", "firsttm", "1sttm", "1tm", "firsttim", "frsttm", "1sttim", "fstti", "frsttim", "1timoth", "frsttimoth"],
        ["2 timothy"] = ["sctim", "sectimothy", "2ndtimoth", "iitimoth", "scndtm", "2ti", "sctimothy", "secondti", "seti", "iiti", "setim", "2ndtm", "secti", "sectimoth", "scndtimoth", "iitimothy", "scti", "setimothy", "sectim", "sectm", "2ndtimothy", "iitm", "2tim", "2timoth", "setimoth", "2ndtim", "sctimoth", "secondtimothy", "2ndti", "setm", "iitim", "scndti", "scndtimothy", "scndtim", "secondtim", "2tm", "secondtm", "2timothy", "secondtimoth", "sctm"],
        ["titus"] = ["tit", "titus", "ti", "tt"],
        ["philemon"] = ["phillemon", "phlm", "pm", "phm", "philemon", "philem", "philm", "phile"],
        ["hebrews"] = ["hebrew", "hebrews", "he", "hb", "heb"],
        ["james"] = ["jms", "jm", "ja", "jas", "jam", "james", "jame"],
        ["1 peter"] = ["ptr", "1p", "frstpete", "firstpe", "1stpet", "firstp", "1peter", "1stpete", "1stpetr", "fstpete", "petr", "firstpete", "fstpeter", "pet", "fstpe", "pete", "frstpe", "firstpetr", "frstptr", "iptr", "1pt", "ipe", "firstptr", "fstpt", "pe", "ip", "1stpt", "ipt", "fstptr", "1ptr", "frstpetr", "1stp", "fstpet", "peter", "frstpeter", "1pe", "frstpet", "1petr", "fstp", "1stpe", "pt", "ipet", "fstpetr", "1stptr", "frstp", "firstpeter", "firstpt", "firstpet", "frstpt", "1pete", "ipeter", "1stpeter", "ipete", "p", "ipetr", "1pet"],
        ["2 peter"] = ["secpe", "scptr", "2pt", "sepeter", "secptr", "iipete", "2ndptr", "secpetr", "secondp", "scpe", "scndpt", "2ndpe", "sep", "scp", "secondpeter", "iipetr", "2ndpt", "2petr", "sept", "scndpeter", "scpete", "scndpete", "sepete", "scpetr", "scpet", "secpet", "scpeter", "2ndpetr", "septr", "sepetr", "2ndpet", "sepe", "scndpet", "secpt", "scndpetr", "sepet", "2p", "scndpe", "2peter", "2pete", "2ndp", "scndptr", "secpeter", "secp", "2pe", "iipt", "iipeter", "scndp", "iiptr", "2ptr", "2pet", "secondpt", "secondpetr", "2ndpete", "secondpete", "2ndpeter", "iipet", "secpete", "scpt", "iip", "secondpet", "secondpe", "secondptr", "iipe"],
        ["1 john"] = ["1jn", "frstj", "1stjo", "1jhn", "firstj", "firstjn", "frstjo", "1stjoh", "1john", "1stjn", "firstjoh", "frstjoh", "fstjohn", "1stjohn", "firstjo", "fstj", "ijo", "fstjhn", "1stjhn", "1joh", "firstjhn", "frstjn", "fstjn", "ijhn", "ijn", "frstjohn", "firstjohn", "1jo", "ijohn", "fstjo", "1stj", "frstjhn", "ij", "fstjoh", "1j", "ijoh"],
        ["2 john"] = ["sejhn", "scndjhn", "scj", "sejoh", "2ndjhn", "secondjohn", "secondjo", "secjhn", "2jhn", "sejo", "secondjn", "scjhn", "sej", "iijoh", "iij", "2ndjo", "2jn", "iijhn", "2jo", "scjo", "scjoh", "secondjhn", "secjn", "scndjn", "iijn", "scjohn", "2ndjoh", "2j", "2ndj", "secondj", "iijohn", "secjohn", "2joh", "iijo", "2ndjohn", "scndjohn", "scndj", "scndjoh", "scjn", "secjoh", "sejohn", "secj", "scndjo", "2ndjn", "2john", "sejn", "secondjoh", "secjo"],
        ["3 john"] = ["iiijoh", "3rdj", "3jn", "iiijn", "3joh", "iiijohn", "3rdjo", "thjoh", "3rdjohn", "thirjohn", "thijo", "thirdj", "thirjn", "thirjoh", "thirdjohn", "thijoh", "iiijhn", "3jo", "thijn", "thijohn", "thjo", "thirj", "3john", "thijhn", "thirdjoh", "thirdjn", "thirjo", "3rdjhn", "3rdjoh", "thij", "thirjhn", "thjhn", "3j", "thjohn", "3rdjn", "thirdjo", "iiijo", "iiij", "thirdjhn", "thjn", "3jhn", "thj"],
        ["jude"] = ["ju", "jude", "jd", "jud"],
        ["revelation"] = ["rv", "revelations", "therevelation", "re", "rev", "revelation"],
        ["1 esdras"] = ["1es", "1esdras", "3ezra", "3ezr"],
        ["2 esdras"] = ["2es", "2esdras", "4ezra", "4ezr"],
        ["tobit"] = ["tob", "tobit"],
        ["judith"] = ["jdt", "judith", "judit"],
        ["esther (greek)"] = ["esg", "esther(greek)", "est(greek)"],
        ["wisdom"] = ["wis", "wisdom", "wisdomofsolomon"],
        ["ecclesiasticus"] = ["sir", "ecclesiasticus", "sirach", "wisdomofbensirach"],
        ["baruch"] = ["bar", "baruch", "1baruch", "1bar"],
        ["epistle of jeremy"] = ["lje", "epistleofjeremy", "letterofjeremiah"],
        ["daniel (greek)"] = ["dag", "daniel(greek)", "dan(greek)"],
        ["song of the three"] = ["s3y", "songofthethree", "songofthethreechildren", "songofthe3children", "songofthethreeyoungmen", "songofthe3youngmen"],
        ["susanna"] = ["sus", "susanna"],
        ["bel and the dragon"] = ["bel", "belandthedragon"],
        ["manasseh"] = ["man", "manasseh", "prayerofmanasseh"],
        ["1 maccabees"] = ["1ma", "1maccabees", "1mac", "1macc", "ima", "imaccabees", "imac", "imacc"],
        ["2 maccabees"] = ["2ma", "2maccabees", "2mac", "2macc", "iima", "iimaccabees", "iimac", "iimacc"],
        ["3 maccabees"] = ["3ma", "3maccabees", "3mac", "3macc", "iiima", "iiimaccabees", "iiimac", "iiimacc"],
        ["4 maccabees"] = ["4ma", "4maccabees", "4mac", "4macc", "ivma", "ivmaccabees", "ivmac", "icmacc"],
        ["laodiceans"] = ["lao", "laodiceans"],
    };
#pragma warning restore format

    /// <summary>
    /// Builds a <see cref="PassageReference" /> from a <see cref="ChapterReference" />.
    /// </summary>
    /// <param name="chapterReference">The chapter reference.</param>
    /// <returns>
    /// The passage reference.
    /// </returns>
    public static PassageReference AsPassageReference(this ChapterReference chapterReference) =>
        chapterReference.ToString().AsPassageReference();

    /// <summary>
    /// Builds a <see cref="PassageReference" /> from a <see cref="string" />.
    /// </summary>
    /// <param name="passage">The passage.</param>
    /// <param name="defaultChapter">The default chapter if none is specified in the passage.</param>
    /// <returns>
    /// The passage reference.
    /// </returns>
    public static PassageReference AsPassageReference(this string passage, int defaultChapter = 1)
    {
        // Declare variables
        PassageReference passageReference = new PassageReference();

        // Check input
        if (string.IsNullOrWhiteSpace(passage))
        {
            return passageReference;
        }

        // Sanitise the passage reference for retrieving the book and verse
        string sanitisedPassage = passage.SanitisePassageReference();

        // Get the book
        string book = sanitisedPassage.GetBook();
        if (!string.IsNullOrWhiteSpace(book) && BookLengths[book] is int[] chapters)
        {
            if (chapters.Length == 1)
            {
                sanitisedPassage = sanitisedPassage.NormaliseSingleChapterReference();
            }

            string[] ranges = sanitisedPassage.GetRanges();
            foreach (string range in ranges)
            {
                if (!int.TryParse(range.Split(':').First(), out int chapter))
                {
                    chapter = defaultChapter;
                }

                // Set the chapter reference
                book = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(book);
                passageReference = new PassageReference
                {
                    ChapterReference = new ChapterReference(book, chapter)
                };

                // Do not highlight the first verse of a one chapter book if there is no colon and there is not a range of verses
                bool highlightVerses =
                    sanitisedPassage.Contains(':', StringComparison.OrdinalIgnoreCase)
                    && chapter > 0;
                if (
                    ranges.Length == 1
                    && chapters.Length == 1
                    && chapter == 1
                    && !passage.Contains(':', StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(range.Split(':').Last(), out int verse)
                    && verse == 1
                )
                {
                    // We can highlight verses other than 1 if there is no colon
                    highlightVerses = false;
                }

                if (highlightVerses)
                {
                    List<string> highlightedVerses = [];
                    StringBuilder sb = new StringBuilder();
                    foreach (string displayRange in ranges)
                    {
                        string displayRangeVerse;
                        if (displayRange == "...")
                        {
                            // Convert ellipsis to hyphen
                            highlightedVerses.Add("-");
                            sb.Append('-');
                            continue;
                        }
                        else if (displayRange.Contains(':'))
                        {
                            string[] displayRangeParts = displayRange.Split(
                                ":",
                                StringSplitOptions.RemoveEmptyEntries
                            );
                            if (
                                int.TryParse(displayRangeParts.First(), out int displayRangeChapter)
                                && displayRangeChapter == chapter
                            )
                            {
                                Match verseNumberMatch = VerseNumberRegex()
                                    .Match(displayRangeParts.Last().Trim());
                                if (verseNumberMatch.Success)
                                {
                                    displayRangeVerse = verseNumberMatch.Value;
                                }
                                else
                                {
                                    // No match
                                    continue;
                                }
                            }
                            else
                            {
                                // Invalid chapter in reference
                                continue;
                            }
                        }
                        else
                        {
                            Match verseNumberMatch = VerseNumberRegex().Match(displayRange.Trim());
                            if (verseNumberMatch.Success)
                            {
                                displayRangeVerse = verseNumberMatch.Value;
                            }
                            else
                            {
                                // No match
                                continue;
                            }
                        }

                        if (sb.Length > 0 && sb[^1] != '-')
                        {
                            sb.Append(',');
                        }

                        highlightedVerses.Add(displayRangeVerse);
                        sb.Append(displayRangeVerse);
                    }

                    // Set the display and highlighted verses
                    passageReference = new PassageReference
                    {
                        ChapterReference = passageReference.ChapterReference,
                        Display = $"{book} {chapter}:{sb}",
                        HighlightedVerses = [.. highlightedVerses],
                    };
                }
                else
                {
                    passageReference = new PassageReference
                    {
                        ChapterReference = passageReference.ChapterReference,
                        Display = $"{book} {chapter}",
                    };
                }

                break;
            }
        }

        return passageReference;
    }

    /// <summary>
    /// Generates a url for the Rendering Parameters.
    /// </summary>
    /// <param name="parameters">The rendering parameters.</param>
    /// <param name="uriKind">Kind of the URI.</param>
    /// <returns>
    /// The url.
    /// </returns>
    public static Uri AsUrl(this RenderingParameters parameters, UriKind uriKind = UriKind.Absolute)
    {
        StringBuilder sb = new StringBuilder();
        if (uriKind == UriKind.Relative)
        {
            sb.Append('/');
        }
        else
        {
            sb.Append("https://goto.bible/");
        }

        string passage = parameters.PassageReference.Display.EncodePassageForUrl();
        if (!string.IsNullOrWhiteSpace(passage))
        {
            sb.Append(passage);
            sb.Append('/');
            sb.Append(Uri.EscapeDataString(parameters.PrimaryTranslation));
            if (!string.IsNullOrWhiteSpace(parameters.SecondaryTranslation))
            {
                sb.Append('/');
                sb.Append(Uri.EscapeDataString(parameters.SecondaryTranslation));
                InterlinearMode mode = parameters.GetInterlinearMode();
                if (mode != InterlinearMode.None)
                {
                    sb.Append($"?settings={(int)mode}");
                }
            }
        }

        return new Uri(sb.ToString(), uriKind);
    }

    /// <summary>
    /// Decodes the passage from a URL.
    /// </summary>
    /// <param name="segment">The URL segment.</param>
    /// <returns>
    /// A string suitable for passing to the <c>RenderPassage</c> API.
    /// </returns>
    /// <remarks>
    /// For example: "1.John.1_3~6-7" will be decoded to "1 John 1:3,6-7".
    /// </remarks>
    public static string DecodePassageFromUrl(this string segment)
    {
        // Check input
        if (string.IsNullOrWhiteSpace(segment))
        {
            return string.Empty;
        }

        // Strip invalid characters
        string passage = StripInvalidCharactersRegex().Replace(segment, string.Empty);
        passage = passage.Replace(".", " ", StringComparison.OrdinalIgnoreCase);
        passage = passage.Replace("_", ":", StringComparison.OrdinalIgnoreCase);
        passage = passage.Replace("~", ",", StringComparison.OrdinalIgnoreCase);
        return passage;
    }

    /// <summary>
    /// Encodes the passage for a URL.
    /// </summary>
    /// <param name="passage">The passage.</param>
    /// <returns>
    /// A string suitable for including in a page URL.
    /// </returns>
    /// <remarks>
    /// For example: "1 John 1:3,6-7" will be encoded as "1.John.1_3~6-7".
    /// </remarks>
    public static string EncodePassageForUrl(this string passage)
    {
        // Check input
        if (string.IsNullOrWhiteSpace(passage))
        {
            return string.Empty;
        }

        // Strip invalid characters
        string segment = StripInvalidCharactersRegex().Replace(passage, string.Empty);
        segment = segment.Replace(" ", ".", StringComparison.OrdinalIgnoreCase);
        segment = segment.Replace(":", "_", StringComparison.OrdinalIgnoreCase);
        segment = segment.Replace(",", "~", StringComparison.OrdinalIgnoreCase);
        return segment;
    }

    /// <summary>
    /// Gets the interlinear mode.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The interlinear mode.</returns>
    public static InterlinearMode GetInterlinearMode(this RenderingParameters parameters)
    {
        InterlinearMode result = parameters.InterlinearIgnoresCase
            ? InterlinearMode.IgnoresCase
            : InterlinearMode.None;
        result |= parameters.InterlinearIgnoresDiacritics
            ? InterlinearMode.IgnoresDiacritics
            : InterlinearMode.None;
        result |= parameters.InterlinearIgnoresPunctuation
            ? InterlinearMode.IgnoresPunctuation
            : InterlinearMode.None;
        return result;
    }

    /// <summary>
    /// Renders the CSS.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>
    /// The CSS code.
    /// </returns>
    public static string RenderCss(this RenderingParameters parameters)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("body{");
        sb.Append($"background-color:{parameters.BackgroundColour.ToHtml()};");
        sb.Append($"color:{parameters.ForegroundColour.ToHtml()};");
        sb.Append(parameters.Font.Bold ? "font-weight:bold;" : "font-weight:normal;");
        sb.Append(parameters.Font.Italic ? "font-style:italic;" : "font-style:normal;");

        if (parameters.Font is { Strikeout: true, Underline: true })
        {
            sb.Append("text-decoration:underline line-through;");
        }
        else if (parameters.Font.Strikeout)
        {
            sb.Append("text-decoration:line-through;");
        }
        else if (parameters.Font.Underline)
        {
            sb.Append("text-decoration:underline;");
        }
        else
        {
            sb.Append("text-decoration:none;");
        }

        sb.Append($"font-size:{parameters.Font.SizeInPoints}pt;");
        sb.Append($"font-family:{parameters.Font.FamilyName}}}");
        if (parameters.Font.Italic)
        {
            sb.Append("em{font-style:normal}");
        }

        sb.Append(
            $"sup{{font-size:{parameters.Font.SizeInPoints * 0.75f}pt;font-weight:bold}}.sup{{font-weight:bold}}"
        );
        sb.Append(".occurrence{vertical-align:super;font-size:smaller}");
        sb.Append(
            $".copyright{{border-top:1px solid {parameters.ForegroundColour.ToHtml()};font-size:{Math.Round(parameters.Font.SizeInPoints * 0.75, 2)}pt}}"
        );
        sb.Append(
            ".supsub{display:inline-flex;flex-direction:column;justify-content:space-between;vertical-align:middle;font-size:50%}"
        );
        sb.Append($"mark{{background-color:{parameters.HighlightColour.ToHtml()}}}");
        return sb.ToString();
    }

    /// <summary>
    /// Converts a <see cref="RenderColour" /> to an HTML string.
    /// </summary>
    /// <param name="colour">The colour.</param>
    /// <returns>
    /// The HTML colour value.
    /// </returns>
    public static string ToHtml(this RenderColour colour) =>
        "#" + colour.R.ToString("X2") + colour.G.ToString("X2") + colour.B.ToString("X2");

    /// <summary>
    /// Gets the name of the book from a passage reference string.
    /// </summary>
    /// <param name="passage">The passage. This must be lower case and without spaces.</param>
    /// <returns>
    /// The book name from the passage reference.
    /// </returns>
    internal static string GetBook(this string passage)
    {
        // Prepare the passage reference for the regex
        passage = passage.Replace(":", string.Empty);

        // Get the book part from the passage reference
        int index = BookPartRegex().Match(passage).Index;
        if (index == 0)
        {
            // This is a book name
            index = passage.Length;
        }
        else
        {
            index++;
        }

        string bookPart = passage[..index];

        // Get the book matching the user's input
        foreach ((string book, string[] aliases) in BookNameMap)
        {
            if (aliases.Contains(bookPart) || book == passage)
            {
                return book;
            }
        }

        // Default to blank
        return string.Empty;
    }

    /// <summary>
    /// Gets the verse ranges from a sanitised passage reference.
    /// </summary>
    /// <param name="passage">The passage.</param>
    /// <returns>The verse ranges.</returns>
    /// <remarks>The string must be lower case and without spaces.</remarks>
    internal static string[] GetRanges(this string passage)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(passage) || passage.Length == 1)
        {
            return [];
        }

        passage = passage[1..];
        string rangePart = passage[RangePartRegex().Match(passage).Index..];
        rangePart = rangePart.NormaliseCommas();
        string[] semiParts = rangePart.Split(';');
        rangePart = semiParts[0];
        semiParts = semiParts.Skip(1).ToArray();

        List<string> ranges = [];

        string[] dashParts = rangePart.Split('-');
        string[] startParts = dashParts[0].Split(':');

        ranges.Add(dashParts[0] + (startParts.Length == 2 ? string.Empty : ":1"));

        if (dashParts.Length == 2)
        {
            string[] endParts = dashParts[1].Split(':');
            bool hasVerse = endParts.Length == 2;

            // Mark that we are to fill in this range
            ranges.Add("...");
            if (startParts.Length == 2)
            {
                ranges.Add((hasVerse ? string.Empty : startParts[0] + ":") + dashParts[1]);
            }
            else
            {
                ranges.Add(dashParts[1] + (hasVerse ? string.Empty : ":1"));
            }
        }

        for (int i = 0; i < semiParts.Length; i++)
        {
            if (!semiParts[i].Split('-')[0].Contains(':') && !rangePart.Contains(':'))
            {
                semiParts[i] += ":1";
            }

            ranges.AddRange($"Book{semiParts[i]}".GetRanges());
        }

        return [.. ranges];
    }

    /// <summary>
    /// Normalises the commas in a range part.
    /// </summary>
    /// <param name="rangePart">The range part.</param>
    /// <returns>
    /// The range part with commas normalised.
    /// </returns>
    internal static string NormaliseCommas(this string rangePart)
    {
        string[] parts = rangePart.Split(',');

        for (int i = 1; i < parts.Length; i++)
        {
            if (parts[i].Contains(':'))
            {
                continue;
            }

            if (!parts[0].Contains(':'))
            {
                if (!parts[i].Contains('-'))
                {
                    parts[i] = parts[i] + ':' + parts[0].Split(':')[0].Split('-')[0];
                }
            }
            else
            {
                parts[i] = parts[0].Split(':')[0].Split('-')[0] + ':' + parts[i];
            }
        }

        return string.Join(";", parts);
    }

    /// <summary>
    /// Normalises a single chapter reference.
    /// </summary>
    /// <param name="passage">The passage. This must be lower case and without spaces.</param>
    /// <returns>
    /// The single chapter reference normalised.
    /// </returns>
    internal static string NormaliseSingleChapterReference(this string passage)
    {
        string[] semiParts = passage.Split(';');
        if (!semiParts[0].Contains(':'))
        {
            int numberStart = NumberStartRegex().Match(semiParts[0]).Index + 1;
            if (numberStart > 1)
            {
                string before = semiParts[0][..numberStart];
                string after = semiParts[0][numberStart..];

                // A special exemption for the introduction
                semiParts[0] =
                    semiParts[0][numberStart] == '0' ? $"{before}0:{after}" : $"{before}1:{after}";
            }
        }

        return string.Join(";", semiParts);
    }

    /// <summary>
    /// Sanitises the passage reference.
    /// </summary>
    /// <param name="passage">The passage.</param>
    /// <returns>
    /// The passage reference ready for cleaning.
    /// </returns>
    internal static string SanitisePassageReference(this string passage) =>
        DashRegex()
            .Replace(passage.Replace(" ", string.Empty).Replace('.', ':').ToLowerInvariant(), "-");

    /// <summary>
    /// The book part regular expression.
    /// </summary>
    /// <returns>The regular expression to find the book part.</returns>
    [GeneratedRegex(@"[\w\\(\\)]\d", RegexOptions.Compiled)]
    private static partial Regex BookPartRegex();

    /// <summary>
    /// The dash replacement regular expression.
    /// </summary>
    /// <returns>The regular expression to find dashes.</returns>
    [GeneratedRegex("[‐‑‒–—-]", RegexOptions.Compiled)]
    private static partial Regex DashRegex();

    /// <summary>
    /// The number start regular expression.
    /// </summary>
    /// <returns>The regular expression to find the start of a number.</returns>
    [GeneratedRegex(@"[\w\s]\d", RegexOptions.Compiled)]
    private static partial Regex NumberStartRegex();

    /// <summary>
    /// The range part regular expression.
    /// </summary>
    /// <returns>The regular expression for finding a range part.</returns>
    [GeneratedRegex(@"\d", RegexOptions.Compiled)]
    private static partial Regex RangePartRegex();

    /// <summary>
    /// The strip invalid characters regular expression.
    /// </summary>
    /// <returns>The strip invalid characters regular expression.</returns>
    [GeneratedRegex(@"[^a-zA-Z0-9()\. _~:,-]", RegexOptions.Compiled)]
    private static partial Regex StripInvalidCharactersRegex();

    /// <summary>
    /// The verse number regular expression.
    /// </summary>
    /// <returns>The verse number regular expression.</returns>
    /// <remarks>
    /// This includes support for verses with letters.
    /// </remarks>
    [GeneratedRegex("[0-9]+[a-z]?", RegexOptions.Compiled)]
    private static partial Regex VerseNumberRegex();
}
