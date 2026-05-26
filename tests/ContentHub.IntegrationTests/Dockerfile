FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/ContentHub.IntegrationTests/ContentHub.IntegrationTests.csproj", "tests/ContentHub.IntegrationTests/"]
RUN dotnet restore "tests/ContentHub.IntegrationTests/ContentHub.IntegrationTests.csproj"
COPY . .
WORKDIR "/src/tests/ContentHub.IntegrationTests"
RUN dotnet build "./ContentHub.IntegrationTests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ContentHub.IntegrationTests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContentHub.IntegrationTests.dll"]
