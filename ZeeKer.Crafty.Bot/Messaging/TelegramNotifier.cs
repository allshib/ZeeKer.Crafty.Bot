using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Messaging;

namespace ZeeKer.Crafty.Bot.Messaging;

public sealed class TelegramNotifier : ITelegramNotifier
{
    private readonly ITelegramBotClient client;
    private readonly TelegramBotOptions options;
    private readonly ILogger<TelegramNotifier> logger;
    private readonly ITelegramChatStateRepository chatStateRepository;

    // chatId -> last messageId
    private readonly ConcurrentDictionary<long, int> _lastMessages;

    public TelegramNotifier(
        ITelegramBotClient client,
        IOptions<TelegramBotOptions> options,
        ILogger<TelegramNotifier> logger,
        ITelegramChatStateRepository chatStateRepository)
    {
        this.client = client;
        this.options = options.Value;
        this.logger = logger;
        this.chatStateRepository = chatStateRepository;

        var existingStates = chatStateRepository
            .GetAllAsync(CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        _lastMessages = new ConcurrentDictionary<long, int>(
            existingStates.ToDictionary(state => state.ChatId, state => state.LastMessageId));

        this.client.StartReceiving(async (bot, update, ct) =>
        {
            if (update.Message is { } message)
            {
                if (message.Text != "/showstatistic")
                    return;

                var chatId = message.Chat.Id;
                if (_lastMessages.TryAdd(chatId, 0))
                {
                    await chatStateRepository.UpsertAsync(
                        new TelegramChatState(chatId, 0),
                        ct);
                }

                var sent = await this.client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вывод статистики активирован",
                        cancellationToken: ct);

                _lastMessages[chatId] = sent.MessageId;
                await chatStateRepository.UpsertAsync(
                    new TelegramChatState(chatId, sent.MessageId),
                    ct);
            }
        },
            (bot, exception, ct) =>
            {
                logger.LogError(exception, "Ошибка");
            });
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        foreach (var kvp in _lastMessages.ToArray())
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

                    await chatStateRepository.UpsertAsync(
                        new TelegramChatState(chatId, messageId),
                        cancellationToken);
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

                    await chatStateRepository.UpsertAsync(
                        new TelegramChatState(chatId, sent.MessageId),
                        cancellationToken);
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

                await chatStateRepository.UpsertAsync(
                    new TelegramChatState(chatId, sent.MessageId),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при отправке/редактировании сообщения");
            }
        }
    }
}
