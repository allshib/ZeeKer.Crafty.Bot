using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using System.Net.Http.Headers;
using ZeeKer.Crafty.Bot.Messaging;
using ZeeKer.Crafty.Bot.Services;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Infrastructure.Persistence;
using ZeeKer.Crafty.Messaging;
using ZeeKer.Crafty.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.Configure<TelegramBotOptions>(
    builder.Configuration.GetSection("TelegramBot"));
builder.Services.AddOptions<TelegramBotOptions>()
    .ValidateDataAnnotations();

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
    return new TelegramBotClient(options.Token);
});

builder.Services.AddCraftyClient(builder.Configuration);

builder.Services.AddSingleton<ITelegramNotifier, TelegramNotifier>();
builder.Services.AddSingleton<ServerStatisticsMessageBuilder>();
builder.Services.AddHostedService<CraftyStatusBroadcastService>();

builder.Services.AddDbContextFactory<TelegramBotDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("TelegramBot")
        ?? throw new InvalidOperationException("Connection string 'TelegramBot' is not configured.");

    var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);

    if (!Path.IsPathRooted(sqliteBuilder.DataSource))
    {
        sqliteBuilder.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqliteBuilder.DataSource);
    }

    options.UseSqlite(sqliteBuilder.ConnectionString);
});
builder.Services.AddScoped<ITelegramChatStateRepository, SqliteTelegramChatStateRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TelegramBotDbContext>>();
    using var dbContext = factory.CreateDbContext();
    dbContext.Database.Migrate();
}

app.Services.GetRequiredService<ITelegramNotifier>();

app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Run();
