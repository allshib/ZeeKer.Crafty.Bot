using ZeeKer.Crafty.Dtos;

namespace ZeeKer.Crafty.Infrastructure.Clients;

public interface ICraftyControllerClient
{
    Task<IReadOnlyCollection<ServerStatisticsDto>> GetServerStatisticsAsync(CancellationToken cancellationToken = default);
}
