using System.Text.Json;
using System.Text.Json.Serialization;
using ZeeKer.Crafty.Abstractions.Converters;

namespace ZeeKer.Crafty.Abstractions.Models;

public sealed record ServerStatisticsDto
{
    [JsonPropertyName("stats_id")]
    public int StatsId { get; init; }

    [JsonPropertyName("created")]
    public DateTime Created { get; init; }

    [JsonPropertyName("server_id")]
    public JsonElement Server { get; init; }

    [JsonPropertyName("started")]
    public string? Started { get; init; }

    [JsonPropertyName("running")]
    public bool Running { get; init; }

    [JsonPropertyName("cpu")]
    public double? Cpu { get; init; }

    [JsonPropertyName("mem")]
    [JsonConverter(typeof(NumberToStringConverter))]
    public string? Memory { get; init; }

    [JsonPropertyName("mem_percent")]
    public double? MemoryPercent { get; init; }

    [JsonPropertyName("world_name")]
    public string? WorldName { get; init; }

    [JsonPropertyName("world_size")]
    public string? WorldSize { get; init; }

    [JsonPropertyName("server_port")]
    public int? ServerPort { get; init; }

    [JsonPropertyName("int_ping_results")]
    public string? InternalPingResults { get; init; }

    [JsonPropertyName("online")]
    public int Online { get; init; }

    [JsonPropertyName("max")]
    public int? MaxPlayers { get; init; }

    [JsonPropertyName("players")]
    public JsonElement Players { get; init; }

    [JsonPropertyName("desc")]
    public string? Description { get; init; }

    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("updating")]
    public bool Updating { get; init; }

    [JsonPropertyName("waiting_start")]
    public bool WaitingStart { get; init; }

    [JsonPropertyName("first_run")]
    public bool FirstRun { get; init; }

    [JsonPropertyName("crashed")]
    public bool Crashed { get; init; }

    [JsonPropertyName("downloading")]
    public bool Downloading { get; init; }
}
