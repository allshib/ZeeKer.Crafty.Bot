using Microsoft.AspNetCore.Mvc;
using ZeeKer.Crafty.Infrastructure.Clients;

namespace ZeeKer.Crafty.Bot.Controllers;

[ApiController]
[Route("crafty")]
public class CraftyStatusController : ControllerBase
{
    private readonly ICraftyControllerClient _craftyControllerClient;
    private readonly ILogger<CraftyStatusController> _logger;

    public CraftyStatusController(ICraftyControllerClient craftyControllerClient, ILogger<CraftyStatusController> logger)
    {
        _craftyControllerClient = craftyControllerClient;
        _logger = logger;
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatisticAsync(CancellationToken cancellationToken)
    {
        try
        {
            var serverStatistics = await _craftyControllerClient.GetServerStatisticsAsync(cancellationToken);

            return Ok(serverStatistics);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to retrieve CraftyController data.");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Failed to retrieve data from CraftyController." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "CraftyController configuration error.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}
