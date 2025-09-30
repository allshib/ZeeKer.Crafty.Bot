using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using System.Net.Http.Headers;
using ZeeKer.Crafty.Bot.Messaging;
using ZeeKer.Crafty.Bot.Services;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Infrastructure.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CraftyControllerOptions>(
    builder.Configuration.GetSection("CraftyController"));
builder.Services.Configure<TelegramBotOptions>(
    builder.Configuration.GetSection("TelegramBot"));
builder.Services.AddOptions<TelegramBotOptions>()
    .ValidateDataAnnotations();

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
    return new TelegramBotClient(options.Token);
});

builder.Services.AddSingleton<ITelegramNotifier, TelegramNotifier>();
builder.Services.AddSingleton<ServerStatisticsMessageBuilder>();
builder.Services.AddHostedService<CraftyStatusBroadcastService>();

builder.Services.AddHttpClient<ICraftyControllerClient, CraftyControllerClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<CraftyControllerOptions>>().Value;

    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("CraftyController BaseUrl is invalid.");
        }

        client.BaseAddress = baseUri;
    }

    if (!string.IsNullOrWhiteSpace(options.ApiKey))
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.ApiKey);
    }
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Run();
