using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
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
    private readonly HashSet<long> chatIds = [];
    public TelegramNotifier(
        ITelegramBotClient client,
        IOptions<TelegramBotOptions> options,
        ILogger<TelegramNotifier> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;

        _client.StartReceiving(async (bot, update, ct) =>
            {
                if (update.Message is { } message)
                {
                    var chatId = message.Chat.Id;
                    //Console.WriteLine($"ChatId: {chatId}");

                    //await bot.SendTextMessageAsync(chatId, "Привет! Это твой ChatId.", cancellationToken: ct);
                }
            },
            (bot, exception, ct) =>
            {
                logger.LogError(exception, "Ошибка");
            });

    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var chatId in chatIds)
            {
                await _client.SendTextMessageAsync(
                    chatId: chatId,
                    text: message,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram message.");
            throw;
        }
    }
}
