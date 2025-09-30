using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZeeKer.Crafty.Infrastructure.Persistence;

public sealed class TelegramBotDbContextFactory : IDesignTimeDbContextFactory<TelegramBotDbContext>
{
    public TelegramBotDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TelegramBotDbContext>();
        optionsBuilder.UseSqlite("Data Source=telegram-bot.db");

        return new TelegramBotDbContext(optionsBuilder.Options);
    }
}
