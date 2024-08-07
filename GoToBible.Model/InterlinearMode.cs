﻿// -----------------------------------------------------------------------
// <copyright file="InterlinearMode.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Model;

using System;

/// <summary>
/// The interlinear mode.
/// </summary>
[Flags]
public enum InterlinearMode
{
    /// <summary>
    /// No special interlinear settings.
    /// </summary>
    None = 0,

    /// <summary>
    /// The interlinear ignores case.
    /// </summary>
    IgnoresCase = 1,

    /// <summary>
    /// The interlinear ignores diacritics.
    /// </summary>
    IgnoresDiacritics = 2,

    /// <summary>
    /// The interlinear ignores punctuation.
    /// </summary>
    IgnoresPunctuation = 4,
}
