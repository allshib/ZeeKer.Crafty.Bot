using Microsoft.AspNetCore.Mvc;
using ZeeKer.Crafty.Bot.Messaging;
using ZeeKer.Crafty.Bot.Models;

namespace ZeeKer.Crafty.Bot.Controllers;



[ApiController]
[Route("[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ITelegramNotifier notifier;
    private readonly ServerMessageBuilder stringBuilder;

    public WebhookController(ITelegramNotifier notifier, ServerMessageBuilder messageBuilder)
    {
        this.notifier = notifier;
        stringBuilder = messageBuilder;
    }

    [HttpPost("server-stopped")]
    public async Task<IActionResult> ServerDown([FromBody] WebhookPayload serverEvent, CancellationToken cancellationToken)
    {
        var serverName = serverEvent.Embeds?.FirstOrDefault()?.Title ?? "Unknown server";
        var stringEvent = stringBuilder.BuildServerEventMessage(serverName, ServerMessageBuilder.ServerEventType.Stopped);

        await notifier.SendMessage(stringEvent, cancellationToken);

        return Ok();
    }

    [HttpPost("server-started")]
    public async Task<IActionResult> ServerUp([FromBody] WebhookPayload serverEvent, CancellationToken cancellationToken)
    {
        var serverName = serverEvent.Embeds?.FirstOrDefault()?.Title ?? "Unknown server";
        var stringEvent = stringBuilder.BuildServerEventMessage(serverName, ServerMessageBuilder.ServerEventType.Started);

        await notifier.SendMessage(stringEvent, cancellationToken);

        return Ok();
    }
    [HttpPost("server-force-stopped")]
    public async Task<IActionResult> ServerForceUp([FromBody] WebhookPayload serverEvent, CancellationToken cancellationToken)
    {
        var serverName = serverEvent.Embeds?.FirstOrDefault()?.Title ?? "Unknown server";
        var stringEvent = stringBuilder.BuildServerEventMessage(serverName, ServerMessageBuilder.ServerEventType.Stopped);

        await notifier.SendMessage(stringEvent, cancellationToken);

        return Ok();
    }

    [HttpPost("server-crashed")]
    public async Task<IActionResult> ServerError([FromBody] WebhookPayload serverEvent, CancellationToken cancellationToken)
    {
        var serverName = serverEvent.Embeds?.FirstOrDefault()?.Title ?? "Unknown server";
        var stringEvent = stringBuilder.BuildServerEventMessage(serverName, ServerMessageBuilder.ServerEventType.Crashed);

        await notifier.SendMessage(stringEvent, cancellationToken);

        return Ok();
    }
}

