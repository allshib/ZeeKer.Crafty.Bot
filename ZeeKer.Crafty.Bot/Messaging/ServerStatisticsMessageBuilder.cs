using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using ZeeKer.Crafty.Dtos;

namespace ZeeKer.Crafty.Bot.Messaging;

public sealed class ServerStatisticsMessageBuilder
{
    private static readonly string[] FlagLabels =
    [
        "Updating",
        "Waiting start",
        "First run",
        "Crashed",
        "Downloading"
    ];

    public string Build(IEnumerable<ServerStatisticsDto> statistics)
    {
        if (statistics is null)
        {
            throw new ArgumentNullException(nameof(statistics));
        }

        var stats = statistics
            .Where(static stat => stat is not null)
            .ToList();

        if (stats.Count == 0)
        {
            return "No server statistics available.";
        }

        var totalPlayers = stats.Sum(static stat => stat.Online);

        var builder = new StringBuilder();
        builder.AppendLine("Crafty Server Summary");
        builder.AppendLine($"Total servers: {stats.Count}");
        builder.AppendLine($"Total players online: {totalPlayers}");
        builder.AppendLine();

        foreach (var stat in stats.OrderBy(GetServerName, StringComparer.OrdinalIgnoreCase))
        {
            var serverName = GetServerName(stat);
            builder.Append("- ")
                .Append(serverName)
                .Append(" (")
                .Append(stat.Running ? "Running" : "Stopped")
                .AppendLine(")");

            builder.Append("  Players: ")
                .Append(stat.Online)
                .Append('/')
                .AppendLine(stat.MaxPlayers?.ToString(CultureInfo.InvariantCulture) ?? "?");

            builder.Append("  World: ")
                .AppendLine(FormatWorld(stat));

            builder.Append("  CPU: ")
                .AppendLine(FormatPercentage(stat.Cpu));

            builder.Append("  Memory: ")
                .AppendLine(FormatMemory(stat));

            builder.Append("  Version: ")
                .AppendLine(!string.IsNullOrWhiteSpace(stat.Version) ? stat.Version : "n/a");

            builder.Append("  Started: ")
                .AppendLine(!string.IsNullOrWhiteSpace(stat.Started) ? stat.Started : "n/a");

            builder.Append("  Flags: ")
                .AppendLine(FormatFlags(stat));

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string GetServerName(ServerStatisticsDto statistics)
    {
        if (statistics.Server.ValueKind == JsonValueKind.Object)
        {
            if (TryGetPropertyAsString(statistics.Server, "server_name", out var serverName) && !string.IsNullOrWhiteSpace(serverName))
            {
                return serverName!;
            }

            if (TryGetPropertyAsString(statistics.Server, "name", out var name) && !string.IsNullOrWhiteSpace(name))
            {
                return name!;
            }
        }

        if (!string.IsNullOrWhiteSpace(statistics.Description))
        {
            return statistics.Description!;
        }

        return $"Server #{statistics.StatsId}";
    }

    private static bool TryGetPropertyAsString(JsonElement element, string propertyName, out string? value)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return true;
        }

        value = null;
        return false;
    }

    private static string FormatWorld(ServerStatisticsDto statistics)
    {
        var hasWorldName = !string.IsNullOrWhiteSpace(statistics.WorldName);
        var hasWorldSize = !string.IsNullOrWhiteSpace(statistics.WorldSize);

        if (!hasWorldName && !hasWorldSize)
        {
            return "n/a";
        }

        if (hasWorldName && hasWorldSize)
        {
            return $"{statistics.WorldName} ({statistics.WorldSize})";
        }

        return hasWorldName ? statistics.WorldName! : statistics.WorldSize!;
    }

    private static string FormatPercentage(double? value)
    {
        if (!value.HasValue)
        {
            return "n/a";
        }

        return FormattableString.Invariant($"{value.Value:0.##}%");
    }

    private static string FormatMemory(ServerStatisticsDto statistics)
    {
        var hasMemoryValue = !string.IsNullOrWhiteSpace(statistics.Memory);
        var memoryPercent = statistics.MemoryPercent.HasValue
            ? FormattableString.Invariant($"{statistics.MemoryPercent.Value:0.##}%")
            : null;

        return (hasMemoryValue, memoryPercent) switch
        {
            (false, null) => "n/a",
            (true, null) => statistics.Memory!,
            (false, not null) => memoryPercent!,
            (true, not null) => $"{statistics.Memory} ({memoryPercent})"
        };
    }

    private static string FormatFlags(ServerStatisticsDto statistics)
    {
        var flags = new List<string>(5);

        if (statistics.Updating)
        {
            flags.Add(FlagLabels[0]);
        }

        if (statistics.WaitingStart)
        {
            flags.Add(FlagLabels[1]);
        }

        if (statistics.FirstRun)
        {
            flags.Add(FlagLabels[2]);
        }

        if (statistics.Crashed)
        {
            flags.Add(FlagLabels[3]);
        }

        if (statistics.Downloading)
        {
            flags.Add(FlagLabels[4]);
        }

        return flags.Count == 0 ? "None" : string.Join(", ", flags);
    }
}
