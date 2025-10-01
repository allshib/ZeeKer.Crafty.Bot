# ZeeKer Crafty Bot

ZeeKer Crafty Bot is an ASP.NET Core worker + API that periodically polls the [Crafty Controller](https://craftycontrol.com/) REST API and broadcasts consolidated Minecraft server statistics to Telegram chats. It keeps track of chat subscriptions, edits the last broadcast when possible, and exposes Swagger-enabled endpoints for future extensions.

## Features

- ⏱️ Background service that collects server statistics from Crafty on a configurable interval.
- 🤖 Telegram bot integration that supports `/showstatistic` and `/dontshowstatistic` commands to manage subscriptions.
- 🧠 Smart message updates: edits the previous status message per chat when possible, falling back to new messages when required.
- 💾 Persistent chat state stored in SQLite via Entity Framework Core migrations.
- 📦 Clean solution split into reusable projects (abstractions, client, storage, bot host, and tests).

## Solution structure

| Project | Description |
|---------|-------------|
| `ZeeKer.Crafty.Bot` | ASP.NET Core host that wires up configuration, DI, hosted service, and Swagger UI. |
| `ZeeKer.Crafty.Client` | Typed HTTP client for the Crafty Controller API. |
| `ZeeKer.Crafty.Storage` | EF Core storage for Telegram chat subscription state. |
| `ZeeKer.Crafty` & `ZeeKer.Crafty.Abstractions` | Shared models, options, and interfaces consumed by the bot and storage layers. |
| `ZeeKer.Crafty.Bot.Tests` | xUnit tests for bot-specific components (e.g., message formatting). |

## Prerequisites

- [.NET SDK 9.0](https://dotnet.microsoft.com/) or later.
- A Crafty Controller instance with an API key enabled.
- A Telegram bot token obtained from [@BotFather](https://t.me/BotFather).

## Configuration

Configuration values live in `appsettings.json`, `appsettings.Development.json`, or environment variables.

```json
{
  "ConnectionStrings": {
    "TelegramBot": "Data Source=craftybot.db"
  },
  "CraftyController": {
    "BaseUrl": "https://crafty.example/api/v2",
    "ApiKey": "<crafty-api-key>"
  },
  "TelegramBot": {
    "Token": "<telegram-bot-token>",
    "UpdateIntervalMinutes": 1
  }
}
```

Environment variables follow the standard ASP.NET Core conventions, for example:

```bash
export ConnectionStrings__TelegramBot="Data Source=/data/craftybot.db"
export CraftyController__BaseUrl="https://crafty.example/api/v2"
export CraftyController__ApiKey="${CRAFTY_API_KEY}"
export TelegramBot__Token="${TELEGRAM_TOKEN}"
export TelegramBot__UpdateIntervalMinutes=5
```

When the host starts it automatically applies pending EF Core migrations, creating the SQLite database file if it does not exist.

## Running locally

```bash
# Restore dependencies
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 dotnet restore

# Run the bot (uses appsettings.Development.json when ASPNETCORE_ENVIRONMENT=Development)
dotnet run --project ZeeKer.Crafty.Bot/ZeeKer.Crafty.Bot.csproj
```

The bot will:

1. Migrate the SQLite database.
2. Start a background loop that polls Crafty using the configured interval.
3. Start receiving Telegram updates to handle `/showstatistic` and `/dontshowstatistic` commands.
4. Publish or edit status messages in subscribed chats.

Swagger UI is available at `https://localhost:5001/swagger` (or `http://localhost:5000/swagger` when running without HTTPS).

## Docker

A `Dockerfile` is provided under `ZeeKer.Crafty.Bot/`. Build and run with:

```bash
docker build -t zeeker-crafty-bot ZeeKer.Crafty.Bot

docker run --rm \
  -e CraftyController__BaseUrl="https://crafty.example/api/v2" \
  -e CraftyController__ApiKey="${CRAFTY_API_KEY}" \
  -e TelegramBot__Token="${TELEGRAM_TOKEN}" \
  -e TelegramBot__UpdateIntervalMinutes=5 \
  -v $(pwd)/data:/app/data \
  zeeker-crafty-bot
```

Mount a host volume if you want to persist the SQLite database between container restarts.

## Tests

Run the automated tests with:

```bash
dotnet test
```

This executes the unit tests in `ZeeKer.Crafty.Bot.Tests`, covering components such as the `ServerStatisticsMessageBuilder` that formats Crafty statistics into human-friendly Telegram messages.

## Contributing

1. Fork the repository and create a feature branch.
2. Make your changes and add tests when possible.
3. Run `dotnet format` (optional) and `dotnet test` to verify your work.
4. Submit a pull request describing your changes.

## License

This project is licensed under the [MIT License](LICENSE.txt).

