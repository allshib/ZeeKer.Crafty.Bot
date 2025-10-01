using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Messaging;

namespace ZeeKer.Crafty.Bot.Messaging;

public sealed class TelegramNotifier : ITelegramNotifier
{
    private readonly ITelegramBotClient client;
    private readonly TelegramBotOptions options;
    private readonly ILogger<TelegramNotifier> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;

    // chatId -> last messageId
    private readonly ConcurrentDictionary<long, int> _lastMessages;

    public TelegramNotifier(
        ITelegramBotClient client,
        IOptions<TelegramBotOptions> options,
        ILogger<TelegramNotifier> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.client = client;
        this.options = options.Value;
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;

        using var scope = serviceScopeFactory.CreateScope();
        var chatStateRepository = scope.ServiceProvider.GetRequiredService<ITelegramChatStateRepository>();

        var existingStates = chatStateRepository
            .GetAll(CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        _lastMessages = new ConcurrentDictionary<long, int>(
            existingStates.ToDictionary(state => state.ChatId, state => state.LastMessageId));

        this.client.StartReceiving(async (bot, update, ct) =>
        {
            if (update.Message is { } message)                
                await HandleMessage(message, chatStateRepository, ct);
        },
            (bot, exception, ct) =>
            {
                logger.LogError(exception, "Ошибка");
            });
    }

    private async Task HandleMessage(Message message, ITelegramChatStateRepository chatStateRepository,  CancellationToken ct)
    {
        switch (message.Text)
        {
            case "/showstatistic":
                var chatId = message.Chat.Id;
                if (_lastMessages.TryAdd(chatId, 0))
                {
                    await chatStateRepository.Upsert(
                        new TelegramChatState(chatId, 0),
                        ct);
                }

                var sent = await this.client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вывод статистики активирован",
                        cancellationToken: ct);

                _lastMessages[chatId] = sent.MessageId;
                await chatStateRepository.Upsert(new(chatId, sent.MessageId), ct);
                break;
            case "/dontshowstatistic":

                _lastMessages.TryRemove(message.Chat.Id, out var val);

                var msg = await this.client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Вывод статистики Отключен",
                        cancellationToken: ct);

                await chatStateRepository.Delete(message.Chat.Id, ct);
                break;
        }
        
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var chatStateRepository = scope.ServiceProvider.GetRequiredService<ITelegramChatStateRepository>();

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

                    await chatStateRepository.Upsert(new(chatId, messageId), cancellationToken);
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

                    await chatStateRepository.Upsert(new(chatId, sent.MessageId), cancellationToken);
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

                await chatStateRepository.Upsert(new(chatId, sent.MessageId), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при отправке/редактировании сообщения");
            }
        }
    }
}
