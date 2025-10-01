using Microsoft.EntityFrameworkCore;
using ZeeKer.Crafty.Storage.Entities;

namespace ZeeKer.Crafty.Storage;

public sealed class TelegramBotDbContext(DbContextOptions<TelegramBotDbContext> options)
    : DbContext(options)
{
    public DbSet<TelegramChatStateEntity> TelegramChatStates => Set<TelegramChatStateEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TelegramChatStateEntity>();

        entity.ToTable("TelegramChatStates");
        entity.HasKey(e => e.ChatId);
        entity.HasIndex(e => e.ChatId)
            .IsUnique();
        entity.Property(e => e.ChatId)
            .ValueGeneratedNever();
        entity.Property(e => e.LastMessageId)
            .IsRequired();
    }
}
