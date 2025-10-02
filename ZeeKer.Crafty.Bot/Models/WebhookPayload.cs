
using System.Text.Json.Serialization;

namespace ZeeKer.Crafty.Bot.Models;

public class WebhookPayload
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = null!;

    [JsonPropertyName("embeds")]
    public List<Embed> Embeds { get; set; } = null!;
}

public class Author
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}

public class Embed
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("color")]
    public int Color { get; set; }

    [JsonPropertyName("author")]
    public Author Author { get; set; } = null!;

    [JsonPropertyName("footer")]
    public Footer Footer { get; set; } = null!;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class Footer
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;
}
