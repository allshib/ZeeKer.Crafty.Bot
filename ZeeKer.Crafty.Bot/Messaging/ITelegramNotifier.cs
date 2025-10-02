namespace ZeeKer.Crafty.Bot.Messaging;

public interface ITelegramNotifier
{
    /// <summary>
    /// Обновляет статичное сообщение во всех чатах.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateStaticMessage(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправляет сообщение во все чаты.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendMessage(string message, CancellationToken cancellationToken = default);
}
