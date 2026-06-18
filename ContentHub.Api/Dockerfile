FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
RUN mkdir -p /app/storage/assets && chown -R $APP_UID:$APP_UID /app/storage
USER $APP_UID
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ContentHub.Contracts/ContentHub.Contracts.csproj", "ContentHub.Contracts/"]
COPY ["ContentHub.Domain/ContentHub.Domain.csproj", "ContentHub.Domain/"]
COPY ["ContentHub.Data/ContentHub.Data.csproj", "ContentHub.Data/"]
COPY ["ContentHub.Application/ContentHub.Application.csproj", "ContentHub.Application/"]
COPY ["ContentHub.Infrastructure/ContentHub.Infrastructure.csproj", "ContentHub.Infrastructure/"]
COPY ["ContentHub.Api/ContentHub.Api.csproj", "ContentHub.Api/"]
RUN dotnet restore "ContentHub.Api/ContentHub.Api.csproj"
COPY . .
WORKDIR "/src/ContentHub.Api"
RUN dotnet build "./ContentHub.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ContentHub.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContentHub.Api.dll"]
