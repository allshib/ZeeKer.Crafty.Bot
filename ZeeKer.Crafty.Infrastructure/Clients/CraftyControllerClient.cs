using Microsoft.Extensions.Options;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        using var response = await _httpClient.GetAsync($"{options.BaseUrl}/api/v2/servers", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var serversResponse = await response.Content.ReadFromJsonAsync<CraftyResponse<List<ServerDto>>>(cancellationToken: cancellationToken);
        if (serversResponse?.Data is not { Count: > 0 } servers)
            return [];

        var statsTasks = servers
            .Where(server => server.ServerId != Guid.Empty)
            .Select(server => GetServerStatisticsInternalAsync(server.ServerId, cancellationToken))
            .ToList();

        if (statsTasks.Count == 0)
            return [];

        var statsResults = await Task.WhenAll(statsTasks);

        return statsResults
            .Where(stat => stat is not null)
            .Select(stat => stat!)
            .ToArray();
    }

    private async Task<ServerStatisticsDto?> GetServerStatisticsInternalAsync(Guid serverId, CancellationToken cancellationToken)
    {
        var statsEndpoint = $"/api/v2/servers/{serverId}/stats";
        using var response = await _httpClient.GetAsync(statsEndpoint, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var statsResponse = await response.Content.ReadFromJsonAsync<CraftyResponse<ServerStatisticsDto>>(cancellationToken);
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
