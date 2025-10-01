using System.Text.Json.Serialization;

namespace ZeeKer.Crafty.Abstractions.Models;

public sealed record CraftyResponse<T>
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }
}
