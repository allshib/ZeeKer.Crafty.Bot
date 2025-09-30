namespace ZeeKer.Crafty.Bot.Messaging;

public interface ITelegramNotifier
{
    Task SendMessageAsync(string message, CancellationToken cancellationToken = default);
}
