using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HetznerCloud.Core;

/// <summary>
/// The Hetzner Cloud API returns money amounts as decimal strings (for example "17.4900000000").
/// This converter allows such fields to be deserialized into <see cref="decimal"/> properties
/// while still supporting plain numeric JSON values.
/// </summary>
public sealed class DecimalStringConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDecimal(),
            JsonTokenType.String => decimal.TryParse(reader.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : throw new JsonException($"Cannot parse '{reader.GetString()}' as a decimal."),
            _ => throw new JsonException($"Unexpected token '{reader.TokenType}' when reading a decimal.")
        };
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
