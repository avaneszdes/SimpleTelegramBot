FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SimpleTelegramBot/SimpleTelegramBot.csproj", "SimpleTelegramBot/"]
RUN dotnet restore "SimpleTelegramBot/SimpleTelegramBot.csproj"
COPY . .
WORKDIR "/src/SimpleTelegramBot"
RUN dotnet build "SimpleTelegramBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleTelegramBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleTelegramBot.dll"]
