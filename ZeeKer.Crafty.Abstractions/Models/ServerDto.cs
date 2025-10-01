using System.Text.Json.Serialization;

namespace ZeeKer.Crafty.Abstractions.Models;

public sealed record ServerDto
{
    [JsonPropertyName("server_id")]
    public Guid ServerId { get; init; }

    [JsonPropertyName("server_name")]
    public required string Name { get; init; }
}
