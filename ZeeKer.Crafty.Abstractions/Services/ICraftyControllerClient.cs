using ZeeKer.Crafty.Abstractions.Models;

namespace ZeeKer.Crafty.Abstractions.Services;

public interface ICraftyControllerClient
{
    Task<IReadOnlyCollection<ServerStatisticsDto>> GetServerStatisticsAsync(CancellationToken cancellationToken = default);
}
