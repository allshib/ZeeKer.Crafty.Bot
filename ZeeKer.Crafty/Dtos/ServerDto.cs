using System.Text.Json.Serialization;

namespace ZeeKer.Crafty.Dtos;

public sealed record ServerDto
{
    [JsonPropertyName("server_id")]
    public int ServerId { get; init; }
}
