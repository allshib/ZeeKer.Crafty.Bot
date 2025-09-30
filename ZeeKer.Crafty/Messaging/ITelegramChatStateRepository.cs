using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ZeeKer.Crafty.Messaging;

public interface ITelegramChatStateRepository
{
    Task<IReadOnlyCollection<TelegramChatState>> GetAllAsync(CancellationToken cancellationToken);

    Task UpsertAsync(TelegramChatState state, CancellationToken cancellationToken);
}
