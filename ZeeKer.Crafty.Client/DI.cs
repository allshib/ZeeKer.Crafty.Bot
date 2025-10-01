using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using ZeeKer.Crafty.Abstractions.Configuration;
using ZeeKer.Crafty.Abstractions.Services;

namespace ZeeKer.Crafty.Client;

public static class DI
{
    public static IServiceCollection AddCraftyClient(this IServiceCollection services, CraftyControllerOptions options)
    {

        services.AddHttpClient<ICraftyControllerClient, CraftyControllerClient>((sp, client) =>
        {
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
                {
                    throw new InvalidOperationException("CraftyController BaseUrl is invalid.");
                }

                client.BaseAddress = baseUri;
            }

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.ApiKey);
            }
        });
        return services;
    }
}

