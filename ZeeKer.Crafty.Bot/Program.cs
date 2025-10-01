using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using ZeeKer.Crafty.Bot.Messaging;
using ZeeKer.Crafty.Bot.Services;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Client;
using ZeeKer.Crafty.Storage;
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

builder.Services.AddStorage(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

using var scope = app.Services.CreateScope();
scope.ServiceProvider.Migrate();

app.Services.GetRequiredService<ITelegramNotifier>();

app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Run();
