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

    public async Task<IReadOnlyCollection<ServerStatisticsDto>> GetServerStatisticsAsync(CancellationToken cancellationToken = default)
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
        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var serversResponse = await response.Content.ReadFromJsonAsync<CraftyResponse<List<ServerDto>>>(cancellationToken: cancellationToken);
        if (serversResponse?.Data is not { Count: > 0 } servers)
        {
            return Array.Empty<ServerStatisticsDto>();
        }

        var statsEndpointBase = options.ServersEndpoint.TrimEnd('/');

        var statsTasks = servers
            .Where(server => server.ServerId > 0)
            .Select(server => GetServerStatisticsInternalAsync(statsEndpointBase, server.ServerId, cancellationToken))
            .ToList();

        if (statsTasks.Count == 0)
        {
            return Array.Empty<ServerStatisticsDto>();
        }

        var statsResults = await Task.WhenAll(statsTasks);

        return statsResults
            .Where(stat => stat is not null)
            .Select(stat => stat!)
            .ToArray();
    }

    private async Task<ServerStatisticsDto?> GetServerStatisticsInternalAsync(string statsEndpointBase, int serverId, CancellationToken cancellationToken)
    {
        var statsEndpoint = $"{statsEndpointBase}/{serverId}/stats";
        using var response = await _httpClient.GetAsync(statsEndpoint, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var statsResponse = await response.Content.ReadFromJsonAsync<CraftyResponse<ServerStatisticsDto>>(cancellationToken: cancellationToken);
        return statsResponse?.Data;
    }

    private static async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var errorBody = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync(cancellationToken);

        throw new HttpRequestException(
            $"CraftyController responded with {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {errorBody}",
            null,
            response.StatusCode);
    }
}
