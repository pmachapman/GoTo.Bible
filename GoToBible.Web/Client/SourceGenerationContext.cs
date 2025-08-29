// -----------------------------------------------------------------------
// <copyright file="SourceGenerationContext.cs" company="Conglomo">
// Copyright 2020-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Client;

using System.Text.Json.Serialization;
using GoToBible.Model;

[JsonSerializable(typeof(RenderedPassage))]
[JsonSerializable(typeof(RenderingParameters))]
[JsonSerializable(typeof(Translation[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class SourceGenerationContext : JsonSerializerContext;
