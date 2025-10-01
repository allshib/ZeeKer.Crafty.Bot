using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace ZeeKer.Crafty.Abstractions.Converters;

public class NumberToStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value);
    }
}
