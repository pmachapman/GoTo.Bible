﻿// -----------------------------------------------------------------------
// <copyright file="StringConverter.cs" company="Conglomo">
// Copyright 2020-2024 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A JSON string converter that supports integers.
/// </summary>
/// <seealso cref="JsonConverter" />
/// <remarks>This is required for 1.0-1.1 API clients to access the 1.2+ API.</remarks>
public class StringConverter : JsonConverter<string?>
{
    /// <inheritdoc/>
    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) =>
        reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32().ToString(),
            JsonTokenType.String => reader.GetString(),
            _ => throw new JsonException(),
        };

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        string? value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value);
}
