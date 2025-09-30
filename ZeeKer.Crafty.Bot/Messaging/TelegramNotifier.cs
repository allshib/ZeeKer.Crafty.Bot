using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using ZeeKer.Crafty.Configuration;

namespace ZeeKer.Crafty.Bot.Messaging;

public interface ITelegramNotifier
{
    Task SendMessageAsync(string message, CancellationToken cancellationToken = default);
}

public sealed class TelegramNotifier : ITelegramNotifier
{
    private readonly ITelegramBotClient _client;
    private readonly TelegramBotOptions _options;
    private readonly ILogger<TelegramNotifier> _logger;

    public TelegramNotifier(
        ITelegramBotClient client,
        IOptions<TelegramBotOptions> options,
        ILogger<TelegramNotifier> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.SendTextMessageAsync(
                chatId: _options.ChatId,
                text: message,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram message.");
            throw;
        }
    }
}
