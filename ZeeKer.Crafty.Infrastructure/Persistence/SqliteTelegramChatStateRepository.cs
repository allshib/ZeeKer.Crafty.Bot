using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZeeKer.Crafty.Infrastructure.Persistence.Entities;
using ZeeKer.Crafty.Messaging;

namespace ZeeKer.Crafty.Infrastructure.Persistence;

public sealed class SqliteTelegramChatStateRepository(IDbContextFactory<TelegramBotDbContext> contextFactory)
    : ITelegramChatStateRepository
{
    private readonly IDbContextFactory<TelegramBotDbContext> _contextFactory = contextFactory;

    public async Task<IReadOnlyCollection<TelegramChatState>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entities = await dbContext.TelegramChatStates
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    public async Task UpsertAsync(TelegramChatState state, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.TelegramChatStates
            .FirstOrDefaultAsync(x => x.ChatId == state.ChatId, cancellationToken);

        if (entity is null)
        {
            entity = new TelegramChatStateEntity
            {
                ChatId = state.ChatId,
                LastMessageId = state.LastMessageId,
            };

            await dbContext.TelegramChatStates.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.LastMessageId = state.LastMessageId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TelegramChatState MapToDomain(TelegramChatStateEntity entity) =>
        new(entity.ChatId, entity.LastMessageId);
}
