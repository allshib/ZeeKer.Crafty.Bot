namespace ZeeKer.Crafty.Infrastructure.Persistence.Entities;
public sealed class TelegramChatStateEntity
{
    public long ChatId { get; set; }

    public int LastMessageId { get; set; }
}
