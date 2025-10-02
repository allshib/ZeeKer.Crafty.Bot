namespace ZeeKer.Crafty.Bot.Messaging;

public interface ITelegramNotifier
{
    /// <summary>
    /// ��������� ��������� ��������� �� ���� �����.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateStaticMessage(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// ���������� ��������� �� ��� ����.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendMessage(string message, CancellationToken cancellationToken = default);
}
