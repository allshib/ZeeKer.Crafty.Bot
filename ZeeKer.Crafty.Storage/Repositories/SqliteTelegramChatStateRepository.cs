using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.Threading;
using ZeeKer.Crafty.Messaging;
using ZeeKer.Crafty.Storage.Entities;

namespace ZeeKer.Crafty.Storage.Repositories;

public sealed class SqliteTelegramChatStateRepository(IDbContextFactory<TelegramBotDbContext> contextFactory)
    : ITelegramChatStateRepository
{
    private readonly IDbContextFactory<TelegramBotDbContext> _contextFactory = contextFactory;

    public async Task<IReadOnlyCollection<TelegramChatState>> GetAll(CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entities = await dbContext.TelegramChatStates
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    public async Task Upsert(TelegramChatState state, CancellationToken cancellationToken)
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
    public async Task Delete(long chatId, CancellationToken token)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(token);

        var entity = await dbContext.TelegramChatStates
            .FirstOrDefaultAsync(x => x.ChatId == chatId, token);

        if (entity is null)
            return;

        dbContext.TelegramChatStates.Remove(entity);

        await dbContext.SaveChangesAsync(token);
    }

    private static TelegramChatState MapToDomain(TelegramChatStateEntity entity) =>
        new(entity.ChatId, entity.LastMessageId);

    
}
