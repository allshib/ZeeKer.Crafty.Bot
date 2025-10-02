using System.Text.Json;
using Xunit;
using ZeeKer.Crafty.Abstractions.Models;
using ZeeKer.Crafty.Bot.Messaging;

namespace ZeeKer.Crafty.Bot.Tests.Messaging;

public sealed class ServerStatisticsMessageBuilderTests
{
    [Fact]
    public void Build_ShouldFormatSummaryAndServerBlocks()
    {
        // Arrange
        var statistics = new[]
        {
            CreateStatistics(
                statsId: 1,
                serverJson: "{\"server_name\":\"Beta\"}",
                online: 5,
                maxPlayers: null,
                running: false,
                worldName: null,
                worldSize: null,
                cpu: null,
                memory: null,
                memoryPercent: null,
                version: null,
                started: null,
                flags: (updating: false, waitingStart: false, firstRun: false, crashed: false, downloading: false)),
            CreateStatistics(
                statsId: 2,
                serverJson: "{\"server_name\":\"Alpha\"}",
                online: 10,
                maxPlayers: 20,
                running: true,
                worldName: "Earth",
                worldSize: "1.2 GB",
                cpu: 42.5,
                memory: "1.5 GB",
                memoryPercent: 75,
                version: "1.20.4",
                started: "2024-05-01T10:00:00Z",
                flags: (updating: true, waitingStart: false, firstRun: false, crashed: false, downloading: false))
        };

        var builder = new ServerMessageBuilder();

        // Act
        var result = builder.Build(statistics);

        // Assert
        var expected = """
Crafty Server Summary
Total servers: 2
Total players online: 15

- Alpha (Running)
  Players: 10/20
  World: Earth (1.2 GB)
  CPU: 42.5%
  Memory: 1.5 GB (75%)
  Version: 1.20.4
  Started: 2024-05-01T10:00:00Z
  Flags: Updating

- Beta (Stopped)
  Players: 5/?
  World: n/a
  CPU: n/a
  Memory: n/a
  Version: n/a
  Started: n/a
  Flags: None
""";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Build_WhenStatisticsEmpty_ShouldReturnPlaceholder()
    {
        var builder = new ServerMessageBuilder();

        var result = builder.Build([]);

        Assert.Equal("No server statistics available.", result);
    }

    private static ServerStatisticsDto CreateStatistics(
        int statsId,
        string serverJson,
        int online,
        int? maxPlayers,
        bool running,
        string? worldName,
        string? worldSize,
        double? cpu,
        string? memory,
        double? memoryPercent,
        string? version,
        string? started,
        (bool updating, bool waitingStart, bool firstRun, bool crashed, bool downloading) flags)
    {
        return new ServerStatisticsDto
        {
            StatsId = statsId,
            Created = DateTime.UtcNow,
            Server = ParseJson(serverJson),
            Started = started,
            Running = running,
            Cpu = cpu,
            Memory = memory,
            MemoryPercent = memoryPercent,
            WorldName = worldName,
            WorldSize = worldSize,
            ServerPort = 25565,
            InternalPingResults = null,
            Online = online,
            MaxPlayers = maxPlayers,
            Players = ParseJson("[]"),
            Description = null,
            Version = version,
            Updating = flags.updating,
            WaitingStart = flags.waitingStart,
            FirstRun = flags.firstRun,
            Crashed = flags.crashed,
            Downloading = flags.downloading
        };
    }

    private static JsonElement ParseJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
