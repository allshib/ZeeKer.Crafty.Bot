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


        this.client.StartReceiving(async (bot, update, ct) =>
        {
            if (update.Message is { } message)                
                await HandleMessage(message, ct);
        },
            (bot, exception, ct) =>
            {
                logger.LogError(exception, "Ошибка");
            });
    }

    private async Task HandleMessage(Message message,  CancellationToken ct)
    {
        switch (message.Text)
        {
            case "/showstatistic":

                var chatId = message.Chat.Id;

                var sent = await this.client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вывод статистики активирован",
                        cancellationToken: ct);

                await Upsert(new(chatId, sent.MessageId), ct);
                break;
            case "/dontshowstatistic":

                var msg = await this.client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Вывод статистики Отключен",
                        cancellationToken: ct);

                await Delete(message.Chat.Id, ct);
                break;
        }
        
    }

    private async Task Upsert(TelegramChatState state, CancellationToken token)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var chatStateRepository = scope.ServiceProvider.GetRequiredService<ITelegramChatStateRepository>();
        await chatStateRepository.Upsert(state, token);
    }

    private async Task Delete(long chatId, CancellationToken token)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var chatStateRepository = scope.ServiceProvider.GetRequiredService<ITelegramChatStateRepository>();
        await chatStateRepository.Delete(chatId, token);
    }

    public async Task UpdateStaticMessage(string message, CancellationToken cancellationToken = default)
   {
        using var scope = serviceScopeFactory.CreateScope();
        var chatStateRepository = scope.ServiceProvider.GetRequiredService<ITelegramChatStateRepository>();

        var states = await chatStateRepository.GetAll(cancellationToken);

        foreach (var state in states)
        {
            var chatId = state.ChatId;
            var messageId = state.LastMessageId;

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

                await chatStateRepository.Upsert(new(chatId, sent.MessageId), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при отправке/редактировании сообщения");
            }
        }
    }

    public async Task SendMessage(string message, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var chatStateRepository = scope.ServiceProvider.GetRequiredService<ITelegramChatStateRepository>();
        var states = await chatStateRepository.GetAll(cancellationToken);

        foreach (var chatId in states.Select(x=>x.ChatId))
        {
            try
            {
                var sent = await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: message,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken);
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при отправке сообщения");
            }
        }
    }
}
