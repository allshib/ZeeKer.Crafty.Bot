using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;
using ZeeKer.Crafty.Bot.Messaging;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Infrastructure.Clients;

namespace ZeeKer.Crafty.Bot.Services;

public sealed class CraftyStatusBroadcastService : BackgroundService
{
    private readonly ICraftyControllerClient _craftyControllerClient;
    private readonly ITelegramNotifier _telegramNotifier;
    private readonly TelegramBotOptions _options;
    private readonly ILogger<CraftyStatusBroadcastService> _logger;
    private readonly ServerStatisticsMessageBuilder _messageBuilder;

    public CraftyStatusBroadcastService(
        ICraftyControllerClient craftyControllerClient,
        ITelegramNotifier telegramNotifier,
        IOptions<TelegramBotOptions> options,
        ILogger<CraftyStatusBroadcastService> logger,
        ServerStatisticsMessageBuilder messageBuilder)
    {
        _craftyControllerClient = craftyControllerClient;
        _telegramNotifier = telegramNotifier;
        _options = options.Value;
        _logger = logger;
        _messageBuilder = messageBuilder;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = Math.Max(1, _options.UpdateIntervalMinutes);
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
            var statistics = await _craftyControllerClient
                .GetServerStatisticsAsync(cancellationToken);

            var message = _messageBuilder.Build(statistics);

            await _telegramNotifier.SendMessageAsync(message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to retrieve Crafty server statistics.");
        }
        catch (ApiRequestException ex)
        {
            _logger.LogError(ex, "Failed to send Crafty status update to Telegram.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Crafty status broadcast.");
        }
    }
}
