using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;
using ZeeKer.Crafty.Abstractions.Services;
using ZeeKer.Crafty.Bot.Messaging;
using ZeeKer.Crafty.Configuration;

namespace ZeeKer.Crafty.Bot.Services;

public sealed class CraftyStatusBroadcastService : BackgroundService
{
    private readonly ICraftyControllerClient craftyControllerClient;
    private readonly ITelegramNotifier telegramNotifier;
    private readonly TelegramBotOptions options;
    private readonly ILogger<CraftyStatusBroadcastService> logger;
    private readonly ServerMessageBuilder messageBuilder;

    public CraftyStatusBroadcastService(
        ICraftyControllerClient craftyControllerClient,
        ITelegramNotifier telegramNotifier,
        IOptions<TelegramBotOptions> options,
        ILogger<CraftyStatusBroadcastService> logger,
        ServerMessageBuilder messageBuilder)
    {
        this.craftyControllerClient = craftyControllerClient;
        this.telegramNotifier = telegramNotifier;
        this.options = options.Value;
        this.logger = logger;
        this.messageBuilder = messageBuilder;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = Math.Max(1, options.UpdateIntervalMinutes);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        using var timer = new PeriodicTimer(interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await BroadcastStatusAsync(stoppingToken);

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task BroadcastStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await craftyControllerClient.GetServerStatisticsAsync(cancellationToken);

            var message = messageBuilder.Build(statistics);

            await telegramNotifier.UpdateStaticMessage(message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to retrieve Crafty server statistics.");
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex, "Failed to send Crafty status update to Telegram.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during Crafty status broadcast.");
        }
    }
}
