using System.Globalization;
using System.Text;
using System.Text.Json;
using ZeeKer.Crafty.Abstractions.Models;

namespace ZeeKer.Crafty.Bot.Messaging;

public sealed class ServerMessageBuilder
{
    private static readonly string[] FlagLabels =
    [
        "Updating",
        "Waiting start",
        "First run",
        "Crashed",
        "Downloading"
    ];
    public enum ServerEventType
    {
        Started,
        Stopped,
        Crashed,
        Killed,
        Unknown
    }
    public string BuildServerEventMessage(string serverName, ServerEventType eventType)
    {
        string emoji;
        string text;

        switch (eventType)
        {
            case ServerEventType.Started:
                emoji = "🟢";
                text = $"Сервер *{serverName}* был успешно запущен.";
                break;

            case ServerEventType.Stopped:
                emoji = "🛑";
                text = $"Сервер *{serverName}* был остановлен.";
                break;

            case ServerEventType.Crashed:
                emoji = "💥";
                text = $"Сервер *{serverName}* вышел из строя!";
                break;

            case ServerEventType.Killed:
                emoji = "⚡";
                text = $"Сервер *{serverName}* был принудительно остановлен.";
                break;

            default:
                emoji = "ℹ️";
                text = $"Получено событие для сервера *{serverName}*.";
                break;
        }

        var moscowZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Russian Standard Time" : "Europe/Moscow");
        var moscowTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, moscowZone);

        return $"{emoji} {text}\n🕒 {moscowTime:dd.MM.yyyy HH:mm:ss}";
    }

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
            return "ℹ️ Нет доступной статистики по серверам.";
        }

        var totalPlayers = stats.Sum(static stat => stat.Online);
        var moscowZone = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "Russian Standard Time" : "Europe/Moscow");
        var moscowTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, moscowZone);

        var builder = new StringBuilder();
        builder.AppendLine("🌐 *Crafty Server Summary*");
        builder.AppendLine($"🌐 Статистика создана {moscowTime:dd.MM.yyyy HH:mm:ss}");
        builder.AppendLine($"🖥️ Всего серверов: *{stats.Count}*");
        builder.AppendLine($"👥 Игроков онлайн: *{totalPlayers}*");
        builder.AppendLine("────────────────────");

        foreach (var stat in stats.OrderBy(GetServerName, StringComparer.OrdinalIgnoreCase))
        {
            var serverName = GetServerName(stat);
            var statusEmoji = stat.Running ? "✅" : "❌";

            builder.AppendLine($"*{serverName}* {statusEmoji}");
            builder.AppendLine($"👥 Игроки: {stat.Online}/{stat.MaxPlayers?.ToString(CultureInfo.InvariantCulture) ?? "?"}");
            builder.AppendLine($"🌍 Мир: {FormatWorld(stat)}");
            builder.AppendLine($"🖥️ CPU: {FormatPercentage(stat.Cpu)}");
            builder.AppendLine($"💾 Память: {FormatMemory(stat)}");
            builder.AppendLine($"📦 Версия: {(!string.IsNullOrWhiteSpace(stat.Version) ? stat.Version : "n/a")}");
            builder.AppendLine($"⏱️ Старт: {(!string.IsNullOrWhiteSpace(stat.Started) ? stat.Started : "n/a")}");
            builder.AppendLine($"⚑ Флаги: {FormatFlags(stat)}");
            builder.AppendLine("────────────────────");
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
