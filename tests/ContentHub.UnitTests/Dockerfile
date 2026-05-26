FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/ContentHub.UnitTests/ContentHub.UnitTests.csproj", "tests/ContentHub.UnitTests/"]
RUN dotnet restore "tests/ContentHub.UnitTests/ContentHub.UnitTests.csproj"
COPY . .
WORKDIR "/src/tests/ContentHub.UnitTests"
RUN dotnet build "./ContentHub.UnitTests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ContentHub.UnitTests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContentHub.UnitTests.dll"]
