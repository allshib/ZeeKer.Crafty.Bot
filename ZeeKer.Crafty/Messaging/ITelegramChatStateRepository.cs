namespace ZeeKer.Crafty.Messaging;

public interface ITelegramChatStateRepository
{
    Task<IReadOnlyCollection<TelegramChatState>> GetAll(CancellationToken token);

    Task Upsert(TelegramChatState state, CancellationToken token);

    Task Delete(long chatId, CancellationToken token);
}
