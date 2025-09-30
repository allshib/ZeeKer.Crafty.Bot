namespace ZeeKer.Crafty.Infrastructure.Clients;

public interface ICraftyControllerClient
{
    Task<int> GetTotalOnlineAsync(CancellationToken cancellationToken = default);
}
