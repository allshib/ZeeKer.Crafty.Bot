namespace ZeeKer.Crafty.Configuration;

using System.ComponentModel.DataAnnotations;

public record class TelegramBotOptions
{
    [Required]
    public string Token { get; init; } = string.Empty;

    [Required]
    public string ChatId { get; init; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "UpdateIntervalMinutes must be greater than zero.")]
    public int UpdateIntervalMinutes { get; init; } = 1;
}
