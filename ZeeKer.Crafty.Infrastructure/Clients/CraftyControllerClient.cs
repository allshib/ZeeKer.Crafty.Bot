using System.Linq;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Dtos;

namespace ZeeKer.Crafty.Infrastructure.Clients;

public sealed class CraftyControllerClient : ICraftyControllerClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<CraftyControllerOptions> _optionsMonitor;

    public CraftyControllerClient(HttpClient httpClient, IOptionsMonitor<CraftyControllerOptions> optionsMonitor)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
    }

    public async Task<int> GetTotalOnlineAsync(CancellationToken cancellationToken = default)
    {
        var options = _optionsMonitor.CurrentValue;

        if (string.IsNullOrWhiteSpace(options.ServersEndpoint))
        {
            throw new InvalidOperationException("CraftyController options are not configured correctly.");
        }

        if (_httpClient.BaseAddress is null && !Uri.IsWellFormedUriString(options.ServersEndpoint, UriKind.Absolute))
        {
            throw new InvalidOperationException("CraftyController base address is not configured correctly.");
        }

        using var response = await _httpClient.GetAsync(options.ServersEndpoint, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"CraftyController responded with {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {errorBody}",
                null,
                response.StatusCode);
        }

        var servers = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ServerDto>>(cancellationToken: cancellationToken);
        if (servers is null)
        {
            return 0;
        }

        return servers.Sum(server => server?.PlayersOnline ?? 0);
    }
}
