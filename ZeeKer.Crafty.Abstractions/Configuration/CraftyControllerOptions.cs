namespace ZeeKer.Crafty.Abstractions.Configuration;

public record class CraftyControllerOptions
{
    public string BaseUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;
}
