# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ZeeKer.Crafty.Bot/ZeeKer.Crafty.Bot.csproj", "ZeeKer.Crafty.Bot/"]
COPY ["ZeeKer.Crafty.Client/ZeeKer.Crafty.Client.csproj", "ZeeKer.Crafty.Client/"]
COPY ["ZeeKer.Crafty.Abstractions/ZeeKer.Crafty.Abstractions.csproj", "ZeeKer.Crafty.Abstractions/"]
COPY ["ZeeKer.Crafty.Infrastructure/ZeeKer.Crafty.Infrastructure.csproj", "ZeeKer.Crafty.Infrastructure/"]
COPY ["ZeeKer.Crafty/ZeeKer.Crafty.csproj", "ZeeKer.Crafty/"]
COPY ["ZeeKer.Crafty.Storage/ZeeKer.Crafty.Storage.csproj", "ZeeKer.Crafty.Storage/"]
RUN dotnet restore "./ZeeKer.Crafty.Bot/ZeeKer.Crafty.Bot.csproj"
COPY . .
WORKDIR "/src/ZeeKer.Crafty.Bot"
RUN dotnet build "./ZeeKer.Crafty.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ZeeKer.Crafty.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZeeKer.Crafty.Bot.dll"]