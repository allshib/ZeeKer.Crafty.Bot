using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Infrastructure.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.Configure<CraftyControllerOptions>(builder.Configuration.GetSection("CraftyController"));

builder.Services.AddHttpClient<ICraftyControllerClient, CraftyControllerClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<CraftyControllerOptions>>().Value;

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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Run();
