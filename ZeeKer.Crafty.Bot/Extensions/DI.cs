using Microsoft.Extensions.Options;
using Telegram.Bot;
using ZeeKer.Crafty.Bot.Messaging;
using ZeeKer.Crafty.Bot.Services;
using ZeeKer.Crafty.Configuration;

namespace ZeeKer.Crafty.Bot.Extensions;

public static class DI
{
    public static IServiceCollection AddBotServices(this IServiceCollection services)
    {
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
            return new TelegramBotClient(options.Token);
        });

        services.AddSingleton<ITelegramNotifier, TelegramNotifier>();
        services.AddSingleton<ServerMessageBuilder>();
        services.AddHostedService<CraftyStatusBroadcastService>();


        return services;
    }
}

