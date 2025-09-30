namespace ZeeKer.Crafty.Configuration;

public record class CraftyControllerOptions
{
    public string BaseUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;
}
