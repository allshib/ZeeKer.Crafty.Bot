using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZeeKer.Crafty.Messaging;
using ZeeKer.Crafty.Storage.Repositories;

namespace ZeeKer.Crafty.Storage
{
    public static class DI
    {

        public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration, string contentRootPath)
        {
            services.AddDbContextFactory<TelegramBotDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("TelegramBot")
                    ?? throw new InvalidOperationException("Connection string 'TelegramBot' is not configured.");

                var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);

                if (!Path.IsPathRooted(sqliteBuilder.DataSource))
                {
                    sqliteBuilder.DataSource = Path.Combine(contentRootPath, sqliteBuilder.DataSource);
                }

                options.UseSqlite(sqliteBuilder.ConnectionString);
            });
            services.AddScoped<ITelegramChatStateRepository, SqliteTelegramChatStateRepository>();

            return services;
        }

        public static void Migrate(this IServiceProvider serviceProvider)
        {
            var factory = serviceProvider.GetRequiredService<IDbContextFactory<TelegramBotDbContext>>();
            using var dbContext = factory.CreateDbContext();
            dbContext.Database.Migrate();
            
        }
    }
}
