using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
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
    private readonly ITelegramBotClient client;
    private readonly TelegramBotOptions options;
    private readonly ILogger<TelegramNotifier> logger;

    // chatId -> last messageId
    private readonly Dictionary<long, int> _lastMessages = new();

    public TelegramNotifier(
        ITelegramBotClient client,
        IOptions<TelegramBotOptions> options,
        ILogger<TelegramNotifier> logger)
    {
        this.client = client;
        this.options = options.Value;
        this.logger = logger;

        this.client.StartReceiving(async (bot, update, ct) =>
        {
            if (update.Message is { } message)
            {
                if (message.Text != "/showstatistic")
                    return;

                var chatId = message.Chat.Id;
                if (!_lastMessages.ContainsKey(chatId))
                    _lastMessages[chatId] = 0;

                var sent = await this.client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вывод статистики активирован",
                        cancellationToken: ct);
            }
        },
            (bot, exception, ct) =>
            {
                logger.LogError(exception, "Ошибка");
            });
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        foreach (var kvp in _lastMessages.ToList())
        {
            var chatId = kvp.Key;
            var messageId = kvp.Value;

            try
            {
                if (messageId > 0)
                {
                    // пробуем редактировать существующее сообщение
                    await client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: message,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    // если ещё нет — отправляем новое
                    var sent = await client.SendTextMessageAsync(
                        chatId: chatId,
                        text: message,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);

                    _lastMessages[chatId] = sent.MessageId;
                }
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.ErrorCode == 400 || ex.ErrorCode == 404)
            {
                // сообщение удалили или оно невалидно -> отправляем заново
                var sent = await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: message,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken);

                _lastMessages[chatId] = sent.MessageId;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при отправке/редактировании сообщения");
            }
        }
    }
}